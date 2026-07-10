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

    private ILocator TopNavLink(string linkName) =>
        _page.Locator("nav.main-navbar").GetByRole(AriaRole.Link, new() { Name = linkName, Exact = true });

    // Clicks a top-level nav bar link (e.g. "COMPETITIONS") and waits for the resulting page to finish loading.
    public async Task NavigateViaTopNav(string linkName)
    {
        await TopNavLink(linkName).ClickAsync();
        // DOMContentLoaded - waits for every subresource
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await DismissCookieBannerIfShown();
    }

    // Opens a recommendation link from the homepage via the hamburger menu.
    public async Task<LeaguePage> OpenRecommendation(string linkTitle)
    {
        await HamburgerMenu.ClickAsync();
        await GetRecommendationLink(linkTitle).ClickAsync();

        var clubsPage = new LeaguePage(_page);
        await clubsPage.AssertTableVisible();
        await DismissCookieBannerIfShown();

        return clubsPage;
    }

    // Opens the login form. The header's login control is a plain shadow-DOM button with no
    // href/navigation wired to it that Playwright can drive, so this navigates directly instead.
    public async Task<LoginPage> OpenLogin()
    {
        await _page.GotoAsync(LoginPage.Url);
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await DismissCookieBannerIfShown();

        return new LoginPage(_page);
    }

    // Submits a quick-search query via the header search box and returns the results page.
    public async Task<SearchResultsPage> Search(string query)
    {
        await SearchInput.FillAsync(query);
        await SearchButton.ClickAsync();
        // Wait for the URL change and the page to finish loading
        await _page.WaitForURLAsync(url => url.Contains($"query={Uri.EscapeDataString(query)}"),
            new PageWaitForURLOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        return new SearchResultsPage(_page);
    }
}
