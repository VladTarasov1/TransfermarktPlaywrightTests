using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

public abstract class BasePage(IPage page)
{
    protected readonly IPage _page = page;

    private ILocator AcceptCookieButton =>
        _page.FrameLocator("iframe[title=\"Iframe title\"]")
             .GetByRole(AriaRole.Button, new() { Name = "Accept & continue" });

    private ILocator MainLogo => _page.Locator("a.tm-header__logo");

    // Dismisses the cookie/consent banner if present.
    public async Task DismissCookieBanner()
    {
        await AcceptCookieButton.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await AcceptCookieButton.ClickAsync();
    }

    // Clicks the header logo, present on every page, returning to the homepage.
    public async Task<HomePage> OpenHomePageFromLogo()
    {
        await MainLogo.ClickAsync();
        return new HomePage(_page);
    }
}
