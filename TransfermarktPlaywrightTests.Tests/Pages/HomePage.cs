using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the transfermarkt.com homepage and the navigation flow
public class HomePage(IPage page) : BasePage(page)
{
    // The homepage's URL.
    public const string Url = "https://www.transfermarkt.com/";

    // Navigates to the homepage.
    public async Task Navigate()
    {
        await _page.GotoAsync(Url);
    }

    // Opens the login form directly by URL.
    public async Task<LoginPage> OpenLogin()
    {
        await _page.GotoAsync(LoginPage.Url);
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        return new LoginPage(_page);
    }
}
