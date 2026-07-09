using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

public abstract class BasePage(IPage page)
{
    protected readonly IPage _page = page;

    private ILocator AcceptCookieButton =>
        _page.FrameLocator("iframe[title=\"Iframe title\"]")
             .GetByRole(AriaRole.Button, new() { Name = "Accept & continue" });

    private ILocator MainLogo => _page.Locator("a.tm-header__logo");

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
}
