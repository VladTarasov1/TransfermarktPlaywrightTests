using TransfermarktPlaywrightTests.Tests.Pages;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// "Navigation" tests
[TestFixture]
public class NavigationTests : BaseTest
{
    [TestCase("COMPETITIONS", "COMPETITIONS | Transfermarkt")]
    [TestCase("STATISTICS", "STATISTICS | Transfermarkt")]
    [TestCase("FORUM", "FORUM | Transfermarkt")]
    [TestCase("TRANSFERS & RUMOURS", "TRANSFERS & RUMOURS | Transfermarkt")]
    public async Task TopNavLink_NavigatesToExpectedSection(string linkName, string expectedTitle)
    {
        await HomePage.Header.NavigateViaTopNav(linkName);

        Assert.That(await HomePage.GetTitle(), Is.EqualTo(expectedTitle),
            $"Expected the '{linkName}' nav link to navigate to a page titled '{expectedTitle}'.");
    }

    [Test]
    public async Task HamburgerMenu_RecommendationLink_NavigatesToCompetitionPage()
    {
        var leaguePage = await HomePage.Header.OpenRecommendation("Bundesliga");

        Assert.That(leaguePage.CurrentUrl, Does.Contain("/bundesliga/"),
            "Expected the 'Bundesliga' recommendation link to navigate to the Bundesliga competition page.");
        await leaguePage.AssertTableVisible();
    }

    [Test]
    public async Task Logo_ReturnsToHomepageFromDeepPage()
    {
        var leaguePage = await HomePage.Header.OpenRecommendation("Bundesliga");
        var homePage = await leaguePage.Header.OpenHomePage();

        Assert.That(homePage.CurrentUrl, Is.EqualTo(HomePage.Url),
            "Expected clicking the logo from a competition page to return to the homepage.");
    }

    [Test]
    public async Task BrowserBack_ReturnsToPreviousPage()
    {
        await HomePage.Header.OpenRecommendation("Bundesliga");
        await HomePage.GoBack();

        Assert.That(HomePage.CurrentUrl, Is.EqualTo(HomePage.Url),
            "Expected the browser back button to return to the homepage.");
    }
}
