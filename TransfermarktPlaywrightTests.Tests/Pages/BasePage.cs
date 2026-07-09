using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

public abstract class BasePage(IPage page)
{
    protected readonly IPage _page = page;

    private ILocator AcceptCookieButton =>
        _page.FrameLocator("iframe[title=\"Iframe title\"]")
             .GetByRole(AriaRole.Button, new() { Name = "Accept & continue" });

    // Dismisses the cookie/consent banner if present.
    public async Task DismissCookieBanner()
    {
        await AcceptCookieButton.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await AcceptCookieButton.ClickAsync();
    }
}
