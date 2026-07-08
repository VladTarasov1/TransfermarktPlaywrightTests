using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Pages;

namespace TransfermarktPlaywrightTests.Tests.Tests;

/// <summary>
/// Test 1: "Premier League Table on HomePage"
/// </summary>
[TestFixture]
public class HomePageTests : PageTest
{
    private HomePage _homePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        await _homePage.NavigateAsync();
        await _homePage.DismissCookieBannerIfPresent();
    }

    [Test]
    public async Task HomepageLoads()
    {
        await Page.GotoAsync("https://www.transfermarkt.com/");
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Transfermarkt.*"));
    }

    [Test]
    public async Task PremierLeagueTable_IsDisplayed_AfterNavigatingFromHomePage()
    {
        var leaguePage = await _homePage.GoToPremierLeagueTable();

        Assert.That(await leaguePage.IsDisplayed(), Is.True,
            "Expected the Premier League clubs/table page heading to be visible.");
    }

    [Test]
    public async Task PremierLeagueTable_HasExpectedColumnHeaders()
    {
        var leaguePage = await _homePage.GoToPremierLeagueTable();
        var headers = await leaguePage.GetColumnHeaders();

        Assert.That(headers, Does.Contain("Club"),
            "Expected a 'Club' column header in the table.");
        Assert.That(headers, Is.Not.Empty,
            "Expected at least one column header.");
    }

    [Test]
    public async Task PremierLeagueTable_ContainsAllTwentyClubs()
    {
        var leaguePage = await _homePage.GoToPremierLeagueTable();
        var rowCount = await leaguePage.GetTableRowCount();

        // Premier League has 20 clubs per season.
        Assert.That(rowCount, Is.EqualTo(20),
            $"Expected 20 clubs in the Premier League table, found {rowCount}.");
    }

    [Test]
    public async Task PremierLeagueTable_ContainsKnownClub()
    {
        var leaguePage = await _homePage.GoToPremierLeagueTable();
        var clubNames = await leaguePage.GetClubNames();

        Assert.That(clubNames, Does.Contain("Manchester City"),
            "Expected 'Manchester City' to appear among the listed clubs.");
    }
}
