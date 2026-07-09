using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Pages;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// "Navigation" tests
[TestFixture]
public class NavigationTests : PageTest
{
    private HomePage _homePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _homePage = new HomePage(Page);
        await _homePage.Navigate();
        await _homePage.DismissCookieBanner();
    }

    [TestCase("COMPETITIONS", "COMPETITIONS | Transfermarkt")]
    [TestCase("STATISTICS", "STATISTICS | Transfermarkt")]
    [TestCase("FORUM", "FORUM | Transfermarkt")]
    [TestCase("TRANSFERS & RUMOURS", "TRANSFERS & RUMOURS | Transfermarkt")]
    public async Task TopNavLink_NavigatesToExpectedSection(string linkName, string expectedTitle)
    {
        await _homePage.NavigateViaTopNav(linkName);

        Assert.That(await Page.TitleAsync(), Is.EqualTo(expectedTitle),
            $"Expected the '{linkName}' nav link to navigate to a page titled '{expectedTitle}'.");
    }

    [Test]
    public async Task HamburgerMenu_RecommendationLink_NavigatesToCompetitionPage()
    {
        var leaguePage = await _homePage.OpenRecommendation("Bundesliga");

        Assert.That(Page.Url, Does.Contain("/bundesliga/"),
            "Expected the 'Bundesliga' recommendation link to navigate to the Bundesliga competition page.");
        await leaguePage.AssertTableVisible();
    }

    [Test]
    public async Task Logo_ReturnsToHomepageFromDeepPage()
    {
        var leaguePage = await _homePage.OpenRecommendation("Bundesliga");
        await leaguePage.OpenHomePageFromLogo();

        Assert.That(Page.Url, Is.EqualTo(HomePage.Url),
            "Expected clicking the logo from a competition page to return to the homepage.");
    }

    [Test]
    public async Task BrowserBack_ReturnsToPreviousPage()
    {
        await _homePage.OpenRecommendation("Bundesliga");
        await Page.GoBackAsync();

        Assert.That(Page.Url, Is.EqualTo(HomePage.Url),
            "Expected the browser back button to return to the homepage.");
    }
}
