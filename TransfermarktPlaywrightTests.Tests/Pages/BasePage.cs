using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

public abstract class BasePage(IPage page)
{
    protected readonly IPage _page = page;

    private ILocator AcceptCookieButton =>
        _page.FrameLocator("iframe[title=\"Iframe title\"]")
             .GetByRole(AriaRole.Button, new() { Name = "Accept & continue" });

    private ILocator MainLogo => _page.Locator("a.tm-header__logo");
    
    private ILocator MyProfileButton => _page.Locator("button[title='My profile']");

    private ILocator ProfileOptionLink =>
        _page.GetByRole(AriaRole.Link, new() { Name = "Profile", Exact = true });

    public string CurrentUrl => _page.Url;

    // Dismisses the cookie/consent banner if present.
    public async Task DismissCookieBanner()
    {
        await AcceptCookieButton.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await AcceptCookieButton.ClickAsync();
    }

    // Dismisses the cookie/consent banner if it has reappeared after a subsequent page navigation,
    // without waiting when it doesn't - unlike DismissCookieBanner, its presence here isn't guaranteed.
    public async Task DismissCookieBannerIfShown()
    {
        if (await AcceptCookieButton.IsVisibleAsync())
        {
            await AcceptCookieButton.ClickAsync();
        }
    }

    // Returns the current page's title.
    public Task<string> GetTitle() => _page.TitleAsync();

    // Navigates back to the previous page in browser history.
    public Task GoBack() => _page.GoBackAsync();

    // Clicks the header logo, present on every page, returning to the homepage.
    public async Task<HomePage> OpenHomePageFromLogo()
    {
        await MainLogo.ClickAsync();
        // DOMContentLoaded - waits for every subresource
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await DismissCookieBannerIfShown();

        return new HomePage(_page);
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
        await DismissCookieBannerIfShown();
        await MyProfileButton.ClickAsync();
        await DismissCookieBannerIfShown();
        await ProfileOptionLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await DismissCookieBannerIfShown();

        return new ProfileSettingsPage(_page);
    }
}
