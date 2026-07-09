using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the transfermarkt.com homepage and the navigation flow
public class HomePage : BasePage
{
    public const string Url = "https://www.transfermarkt.com/";

    private ILocator GetRecommendationLink(string linkName) =>
         _page.Locator($".recommendation a[title='{linkName}']");

    public HomePage(IPage page) : base(page)
    {
    }

    public async Task Navigate()
    {
        await _page.GotoAsync(Url);
    }

    // Opens a recommendation link from the homepage via the hamburger menu.
    public async Task<PremierLeaguePage> OpenRecommendation(string linkTitle)
    {
        await HamburgerMenu.ClickAsync();
        await GetRecommendationLink(linkTitle).ClickAsync();

        var clubsPage = new PremierLeaguePage(_page);
        return clubsPage;
    }
}
