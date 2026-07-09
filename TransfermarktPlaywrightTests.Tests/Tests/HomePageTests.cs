using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Pages;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// Test 1: "Premier League Table on HomePage"
[TestFixture]
public class HomePageTests : PageTest
{
    private HomePage _homePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        await _homePage.Navigate();
        await _homePage.DismissCookieBannerIfPresent();
    }

    [Test]
    public async Task PremierLeagueClubOverview_HasConsistentDataForAllTwentyClubs()
    {
        var leaguePage = await _homePage.OpenRecommendation("Premier League");
        await leaguePage.DismissCookieBannerIfPresent();
        await leaguePage.FilterBySeason("25/26");

        // Premier League has 20 clubs per season; waiting for this confirms the table has rendered.
        await leaguePage.AssertClubCount(20);

        var headers = await leaguePage.GetColumnHeaders();
        Assert.That(headers, Is.EquivalentTo(new[]
        {
            "Club", "Squad", "ø age", "Foreigners", "ø market value", "Total market value"
        }), "Expected the club overview table to expose these columns.");

        var crestSources = await leaguePage.GetCrestImageSources();
        Assert.That(crestSources, Has.All.Not.Empty,
            "Expected every club row to have a non-empty crest image src.");

        var rows = await leaguePage.GetClubOverviewRows();
        Assert.That(rows.Select(r => r.ClubName), Does.Contain("Manchester City"),
            "Expected 'Manchester City' to appear among the listed clubs.");

        foreach (var row in rows)
        {
            Assert.That(row.ClubName, Is.Not.Empty,
                "Expected every row to have a club name.");
            Assert.That(row.ClubProfileUrl, Does.Contain("/verein/"),
                $"Expected '{row.ClubName}' to link to a club profile page.");
            Assert.That(row.SquadSize, Is.GreaterThan(0),
                $"Expected '{row.ClubName}' to have a positive squad size.");
            Assert.That(row.AverageAge, Is.InRange(15d, 45d),
                $"Expected '{row.ClubName}' average age to be plausible.");
            Assert.That(row.ForeignersCount, Is.InRange(0, row.SquadSize),
                $"Expected '{row.ClubName}' foreigner count to not exceed squad size.");

            /* Total market value should roughly equal average value x squad size;
               tolerance covers independent rounding of the two displayed figures. */
            var expectedTotal = row.AverageMarketValueEur * row.SquadSize;
            var tolerance = expectedTotal * 0.15m;
            Assert.That(row.TotalMarketValueEur,
                Is.InRange(expectedTotal - tolerance, expectedTotal + tolerance),
                $"Expected '{row.ClubName}' total market value to roughly equal average value x squad size.");
        }

        Assert.That(rows.Select(r => r.TotalMarketValueEur), Is.Ordered.Descending,
            "Expected clubs to be sorted by total market value, highest first.");

        var footer = await leaguePage.GetFooterTotals();

        Assert.That(footer.SquadTotal, Is.EqualTo(rows.Sum(r => r.SquadSize)),
            "Expected the footer squad total to equal the sum of each club's squad size.");
        Assert.That(footer.ForeignersTotal, Is.EqualTo(rows.Sum(r => r.ForeignersCount)),
            "Expected the footer foreigners total to equal the sum of each club's foreigners count.");

        var totalMarketValueSum = rows.Sum(r => r.TotalMarketValueEur);
        var totalTolerance = totalMarketValueSum * 0.05m;
        Assert.That(footer.TotalMarketValueEur,
            Is.InRange(totalMarketValueSum - totalTolerance, totalMarketValueSum + totalTolerance),
            "Expected the footer total market value to equal the sum of each club's total market value.");

        // ø market value is defined as total value / total squad size, not the mean of the per-club averages.
        var expectedFooterAverageValue = footer.TotalMarketValueEur / footer.SquadTotal;
        var averageValueTolerance = expectedFooterAverageValue * 0.05m;
        Assert.That(footer.AverageMarketValueEur,
            Is.InRange(expectedFooterAverageValue - averageValueTolerance, expectedFooterAverageValue + averageValueTolerance),
            "Expected the footer average market value to equal total market value / squad total.");

        // ø age is the squad-size-weighted mean age across all players, not a simple mean of club averages.
        var weightedAverageAge = rows.Sum(r => r.AverageAge * r.SquadSize) / footer.SquadTotal;
        Assert.That(footer.AverageAge, Is.EqualTo(weightedAverageAge).Within(0.2),
            "Expected the footer average age to equal the squad-weighted mean of each club's average age.");
    }
}
