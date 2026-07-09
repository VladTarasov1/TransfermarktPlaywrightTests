using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Pages;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// League table tests
[TestFixture]
public class TableTests : PageTest
{
    private HomePage _homePage = null!;

    private static readonly string[] ExpectedColumnHeaders =
        ["Club", "Squad", "ø age", "Foreigners", "ø market value", "Total market value"];
    private static readonly string[] ExpectedClubs2526 =
        ["AFC Bournemouth", "Arsenal FC", "Aston Villa", "Brentford FC", "Brighton & Hove Albion",
         "Burnley FC", "Chelsea FC", "Crystal Palace", "Everton FC", "Fulham FC",
         "Leeds United", "Liverpool FC", "Manchester City", "Manchester United", "Newcastle United",
         "Nottingham Forest", "Sunderland AFC", "Tottenham Hotspur",
         "West Ham United", "Wolverhampton Wanderers"];

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        await _homePage.Navigate();
        await _homePage.DismissCookieBanner();
    }

    [Test]
    public async Task PremierLeagueTable_HasConsistentDataForAllClubs()
    {
        var leaguePage = await _homePage.OpenRecommendation("Premier League");

        // check that the league page is showing the expected season and number of clubs
        await leaguePage.FilterBySeason("25/26");
        await leaguePage.AssertClubCount(20);

        // check table columns
        Assert.That(await leaguePage.GetColumnHeaders(), Is.EquivalentTo(ExpectedColumnHeaders),
            "Expected the club overview table to expose these columns.");

        // check each club's data
        var rows = await leaguePage.GetClubRows();
        Assert.That(rows.Select(row => row.ClubName), Is.EquivalentTo(ExpectedClubs2526),
            "Expected the listed clubs to match the expected list.");
        foreach (var row in rows)
        {
            Assert.Multiple(() =>
            {
                Assert.That(row.ClubProfileUrl, Is.Not.Empty,
                    $"Expected '{row.ClubName}' to have a club profile link.");
                Assert.That(row.SquadSize, Is.GreaterThan(0),
                    $"Expected '{row.ClubName}' to have a positive squad size.");
                Assert.That(row.AverageAge, Is.InRange(15d, 45d),
                    $"Expected '{row.ClubName}' average age to be truthy.");
                Assert.That(row.ForeignersCount, Is.InRange(0, row.SquadSize),
                    $"Expected '{row.ClubName}' foreigner count to not exceed squad size.");
                Assert.That(row.AverageMarketValueEur, Is.Not.Zero,
                    $"Expected '{row.ClubName}' to have an average market value.");
                Assert.That(row.TotalMarketValueEur, Is.Not.Zero,
                    $"Expected '{row.ClubName}' to have a total market value.");
                Assert.That(row.TotalMarketValueEur, Is.GreaterThan(row.AverageMarketValueEur),
                    $"Expected '{row.ClubName}' total market value to be greater than its average market value.");
            });
        }

        // check that the clubs are sorted by total market value DESC
        Assert.That(rows.Select(row => row.TotalMarketValueEur), Is.Ordered.Descending,
            "Expected clubs to be sorted by total market value, highest first.");

        // check that sorting by average age (DESC) puts the oldest and youngest squads in the right place
        await leaguePage.SortByColumn("ø age");
        var rowsByAge = await leaguePage.GetClubRows();
        Assert.Multiple(() =>
        {
            Assert.That(rowsByAge.First().ClubName, Is.EqualTo("Fulham FC"),
                "Expected Fulham FC to have the oldest average squad age.");
            Assert.That(rowsByAge.Last().ClubName, Is.EqualTo("Chelsea FC"),
                "Expected Chelsea FC to have the youngest average squad age.");
        });

        // check that the footer summary row
        var summaryData = await leaguePage.GetFooterTotals();

        // check squad and foreigners totals
        Assert.Multiple(() =>
        {
            Assert.That(summaryData.SquadTotal, Is.EqualTo(rows.Sum(row => row.SquadSize)),
                    "Expected the footer squad total to equal the sum of each club's squad size.");
            Assert.That(summaryData.ForeignersTotal, Is.EqualTo(rows.Sum(row => row.ForeignersCount)),
                "Expected the footer foreigners total to equal the sum of each club's foreigners count.");
        });

        // check total market value
        var totalMarketValueSum = rows.Sum(row => row.TotalMarketValueEur);
        Assert.That(summaryData.TotalMarketValueEur, Is.EqualTo(totalMarketValueSum).Within(0.5m).Percent,
            "Expected the footer total market value to equal the sum of each club's total market value.");

        // check average market value
        // ø market value is defined as total value / total squad size, not the mean of the per-club averages.
        var expectedAverageValue = summaryData.TotalMarketValueEur / summaryData.SquadTotal;
        Assert.That(summaryData.AverageMarketValueEur, Is.EqualTo(expectedAverageValue).Within(0.5m).Percent,
            "Expected the footer average market value to equal total market value / squad total.");

        // check average age
        // ø age is the squad-size-weighted mean age across all players, not a simple mean of club averages.
        var weightedAverageAge = rows.Sum(r => r.AverageAge * r.SquadSize) / summaryData.SquadTotal;
        Assert.That(summaryData.AverageAge, Is.EqualTo(weightedAverageAge).Within(0.2),
            "Expected the footer average age to equal the squad-weighted mean of each club's average age.");
    }
}
