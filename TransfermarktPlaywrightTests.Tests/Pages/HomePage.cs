using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

/// <summary>
/// Represents the transfermarkt.com homepage and the navigation flow
/// </summary>
public class HomePage
{
    private readonly IPage _page;
    public const string Url = "https://www.transfermarkt.com/";

    private ILocator AcceptCookieButton =>
        _page.FrameLocator("iframe[title=\"Iframe title\"]")
             .GetByRole(AriaRole.Button, new() { Name = "Accept & continue" });

    private ILocator HamburgerMenu => _page.Locator(".hamburger");
    
    private ILocator GetRecommendationLink(string linkName) =>
         _page.Locator($".recommendation a[title='{linkName}']");

    public HomePage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(Url);
    }

    /// <summary>
    /// Dismisses the cookie/consent banner if present.
    /// </summary>
    public async Task DismissCookieBannerIfPresent()
    {
        try
        {
            await AcceptCookieButton.ClickAsync(new() { Timeout = 5000 });
        }
        catch (TimeoutException)
        {
            // Banner didn't appear - safe to ignore.
        }
    }

    /// <summary>
    /// Navigates from the homepage to the Premier League clubs page via the hamburger menu.
    /// </summary>
    public async Task<PremierLeaguePage> GoToPremierLeagueTable()
    {
        await HamburgerMenu.ClickAsync();
        await GetRecommendationLink("Premier League").ClickAsync();

        var clubsPage = new PremierLeaguePage(_page);
        await clubsPage.DismissAdIfPresent();
        return clubsPage;
    }
}
