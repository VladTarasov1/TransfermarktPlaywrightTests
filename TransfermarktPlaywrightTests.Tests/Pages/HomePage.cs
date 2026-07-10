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

    // Opens the login form. The header's login control is a plain shadow-DOM button with no
    // href/navigation wired to it that Playwright can drive, so this navigates directly instead.
    public async Task<LoginPage> OpenLogin()
    {
        await _page.GotoAsync(LoginPage.Url);
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await DismissCookieBannerIfShown();

        return new LoginPage(_page);
    }
}
