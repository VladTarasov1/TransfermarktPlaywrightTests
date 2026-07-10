using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Pages;
using TransfermarktPlaywrightTests.Tests.Helpers;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// "Navigation" tests
[TestFixture]
public class NavigationTests : PageTest
{
    private HomePage _homePage = null!;

    [SetUp]
    public async Task SetUp()
    {
        await ConsentCookies.Seed(Context);
        _homePage = new HomePage(Page);
        await _homePage.Navigate();
    }

    [TestCase("COMPETITIONS", "COMPETITIONS | Transfermarkt")]
    [TestCase("STATISTICS", "STATISTICS | Transfermarkt")]
    [TestCase("FORUM", "FORUM | Transfermarkt")]
    [TestCase("TRANSFERS & RUMOURS", "TRANSFERS & RUMOURS | Transfermarkt")]
    public async Task TopNavLink_NavigatesToExpectedSection(string linkName, string expectedTitle)
    {
        await _homePage.Header.NavigateViaTopNav(linkName);

        Assert.That(await _homePage.GetTitle(), Is.EqualTo(expectedTitle),
            $"Expected the '{linkName}' nav link to navigate to a page titled '{expectedTitle}'.");
    }

    [Test]
    public async Task HamburgerMenu_RecommendationLink_NavigatesToCompetitionPage()
    {
        var leaguePage = await _homePage.Header.OpenRecommendation("Bundesliga");

        Assert.That(leaguePage.CurrentUrl, Does.Contain("/bundesliga/"),
            "Expected the 'Bundesliga' recommendation link to navigate to the Bundesliga competition page.");
        await leaguePage.AssertTableVisible();
    }

    [Test]
    public async Task Logo_ReturnsToHomepageFromDeepPage()
    {
        var leaguePage = await _homePage.Header.OpenRecommendation("Bundesliga");
        var homePage = await leaguePage.Header.OpenHomePage();

        Assert.That(homePage.CurrentUrl, Is.EqualTo(HomePage.Url),
            "Expected clicking the logo from a competition page to return to the homepage.");
    }

    [Test]
    public async Task BrowserBack_ReturnsToPreviousPage()
    {
        await _homePage.Header.OpenRecommendation("Bundesliga");
        await _homePage.GoBack();

        Assert.That(_homePage.CurrentUrl, Is.EqualTo(HomePage.Url),
            "Expected the browser back button to return to the homepage.");
    }
}
