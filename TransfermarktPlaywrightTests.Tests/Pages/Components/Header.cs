using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages.Components;

// The site header (tm-header / tm-userbox), present on every page
public class Header(IPage page)
{
    // Links back to the homepage.
    private ILocator MainLogo => page.Locator("a.tm-header__logo");

    // The search text box.
    private ILocator SearchInput => page.Locator("input[name='query']");

    // Submits the search box.
    private ILocator SearchButton => page.Locator(".tm-header__search button[type='Submit']");

    // Opens the menu containing the recommendation links.
    private ILocator HamburgerMenu => page.Locator(".hamburger");

    // A recommendation link inside the (already-open) hamburger menu.
    private ILocator GetRecommendationLink(string linkName) =>
        page.Locator($".recommendation a[title='{linkName}']");

    // A top-level nav bar link, e.g. "COMPETITIONS".
    private ILocator TopNavLink(string linkName) =>
        page.Locator("nav.main-navbar").GetByRole(AriaRole.Link, new() { Name = linkName, Exact = true });

    // "Log in" when logged out, "My profile" once authenticated.
    private ILocator MyProfileButton => page.Locator("button[title='My profile']");

    // Clicking MyProfileButton opens a dropdown panel containing this link.
    private ILocator ProfileOptionLink =>
        page.GetByRole(AriaRole.Link, new() { Name = "Profile", Exact = true });

    // Clicks the header logo, present on every page, returning to the homepage.
    public async Task<HomePage> OpenHomePage()
    {
        await MainLogo.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        return new HomePage(page);
    }

    // Clicks a top-level nav bar link (e.g. "COMPETITIONS") and waits for the resulting page to finish loading.
    public async Task NavigateViaTopNav(string linkName)
    {
        await TopNavLink(linkName).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    // Opens a recommendation link from the hamburger menu.
    public async Task<LeaguePage> OpenRecommendation(string linkTitle)
    {
        await HamburgerMenu.ClickAsync();
        await GetRecommendationLink(linkTitle).ClickAsync();

        var leaguePage = new LeaguePage(page);
        await leaguePage.AssertTableVisible();

        return leaguePage;
    }

    // Submits a search query via the header search box and returns the results page.
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

    // Opens the "My profile" dropdown and follows its "Profile" link to the profile settings page.
    public async Task<ProfileSettingsPage> OpenProfileSettings()
    {
        await MyProfileButton.ClickAsync();
        await ProfileOptionLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        return new ProfileSettingsPage(page);
    }
}
