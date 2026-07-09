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

    private ILocator SearchInput => _page.Locator("input[name='query']");

    private ILocator SearchButton => _page.Locator(".tm-header__search button[type='Submit']");

    private ILocator HamburgerMenu => _page.Locator(".hamburger");

    private ILocator GetRecommendationLink(string linkName) =>
        _page.Locator($".recommendation a[title='{linkName}']");

    // Opens a recommendation link from the homepage via the hamburger menu.
    public async Task<LeaguePage> OpenRecommendation(string linkTitle)
    {
        await HamburgerMenu.ClickAsync();
        await GetRecommendationLink(linkTitle).ClickAsync();

        var clubsPage = new LeaguePage(_page);
        return clubsPage;
    }

    // Submits a quick-search query via the header search box and returns the results page.
    public async Task<SearchResultsPage> Search(string query)
    {
        await SearchInput.FillAsync(query);
        await SearchButton.ClickAsync();
        await _page.WaitForURLAsync(url => url.Contains($"query={Uri.EscapeDataString(query)}"));

        return new SearchResultsPage(_page);
    }
}
