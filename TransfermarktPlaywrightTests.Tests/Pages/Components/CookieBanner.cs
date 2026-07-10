using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages.Components;

// The Sourcepoint cookie-consent overlay, shown on first load and liable to reappear after
// subsequent navigations/interactions.
public class CookieBanner(IPage page)
{
    private ILocator AcceptButton =>
        page.FrameLocator("iframe[title=\"Iframe title\"]")
            .GetByRole(AriaRole.Button, new() { Name = "Accept & continue" });

    // Dismisses the banner, waiting for it to appear first.
    public async Task Dismiss()
    {
        await AcceptButton.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await AcceptButton.ClickAsync();
    }

    // Dismisses the banner if it has reappeared, without waiting when it hasn't.
    public async Task DismissIfShown()
    {
        if (await AcceptButton.IsVisibleAsync())
        {
            await AcceptButton.ClickAsync();
        }
    }
}
