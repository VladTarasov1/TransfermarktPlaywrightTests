using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages.Components;

// The site header (tm-header / tm-userbox), present on every page. tm-userbox is a Svelte
// custom element with an open shadow DOM; Playwright locators pierce it automatically.
public class Header(IPage page)
{
    private readonly CookieBanner _cookieBanner = new(page);

    private ILocator MainLogo => page.Locator("a.tm-header__logo");

    private ILocator SearchInput => page.Locator("input[name='query']");

    private ILocator SearchButton => page.Locator(".tm-header__search button[type='Submit']");

    private ILocator HamburgerMenu => page.Locator(".hamburger");

    private ILocator GetRecommendationLink(string linkName) =>
        page.Locator($".recommendation a[title='{linkName}']");

    private ILocator TopNavLink(string linkName) =>
        page.Locator("nav.main-navbar").GetByRole(AriaRole.Link, new() { Name = linkName, Exact = true });

    // "Log in" when logged out, "My profile" once authenticated.
    private ILocator MyProfileButton => page.Locator("button[title='My profile']");

    // Clicking MyProfileButton opens a dropdown panel (not a navigation) containing this link.
    private ILocator ProfileOptionLink =>
        page.GetByRole(AriaRole.Link, new() { Name = "Profile", Exact = true });

    // Clicks the header logo, present on every page, returning to the homepage.
    public async Task<HomePage> OpenHomePage()
    {
        await MainLogo.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _cookieBanner.DismissIfShown();

        return new HomePage(page);
    }

    // Clicks a top-level nav bar link (e.g. "COMPETITIONS") and waits for the resulting page to finish loading.
    public async Task NavigateViaTopNav(string linkName)
    {
        await TopNavLink(linkName).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _cookieBanner.DismissIfShown();
    }

    // Opens a recommendation link from the hamburger menu.
    public async Task<LeaguePage> OpenRecommendation(string linkTitle)
    {
        await HamburgerMenu.ClickAsync();
        await GetRecommendationLink(linkTitle).ClickAsync();

        var leaguePage = new LeaguePage(page);
        await leaguePage.AssertTableVisible();
        await _cookieBanner.DismissIfShown();

        return leaguePage;
    }

    // Submits a quick-search query via the header search box and returns the results page.
    public async Task<SearchResultsPage> Search(string query)
    {
        await SearchInput.FillAsync(query);
        await SearchButton.ClickAsync();
        // Wait for the URL change and the page to finish loading
        await page.WaitForURLAsync(url => url.Contains($"query={Uri.EscapeDataString(query)}"),
            new PageWaitForURLOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        return new SearchResultsPage(page);
    }

    // Waits for the header to show the logged-in "My profile" control.
    public async Task WaitForLoggedIn()
    {
        await Expect(MyProfileButton).ToBeVisibleAsync();
    }

    // Opens the "My profile" dropdown and follows its "Profile" link to the profile settings page,
    // whose title embeds the logged-in username.
    public async Task<ProfileSettingsPage> OpenProfileSettings()
    {
        await _cookieBanner.DismissIfShown();
        await MyProfileButton.ClickAsync();
        await _cookieBanner.DismissIfShown();
        await ProfileOptionLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _cookieBanner.DismissIfShown();

        return new ProfileSettingsPage(page);
    }
}
