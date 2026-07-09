using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the transfermarkt.com homepage and the navigation flow
public class HomePage(IPage page) : BasePage(page)
{
    public const string Url = "https://www.transfermarkt.com/";

    public async Task Navigate()
    {
        await _page.GotoAsync(Url);
    }

    private ILocator GetRecommendationLink(string linkName) =>
         _page.Locator($".recommendation a[title='{linkName}']");

    // Opens a recommendation link from the homepage via the hamburger menu.
    public async Task<PremierLeaguePage> OpenRecommendation(string linkTitle)
    {
        await HamburgerMenu.ClickAsync();
        await GetRecommendationLink(linkTitle).ClickAsync();

        var clubsPage = new PremierLeaguePage(_page);
        return clubsPage;
    }
}
