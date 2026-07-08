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
    public async Task PremierLeagueTable_DisplaysAllTwentyClubsWithExpectedData()
    {
        var leaguePage = await _homePage.GoToPremierLeagueTable();

        var headers = await leaguePage.GetColumnHeaders();
        Assert.That(headers, Does.Contain("Club"),
            "Expected a 'Club' column header in the table.");

        // Premier League has 20 clubs per season.
        await leaguePage.AssertClubCountAsync(20);

        var clubNames = await leaguePage.GetClubNames();
        Assert.That(clubNames, Does.Contain("Manchester City"),
            "Expected 'Manchester City' to appear among the listed clubs.");
    }
}
