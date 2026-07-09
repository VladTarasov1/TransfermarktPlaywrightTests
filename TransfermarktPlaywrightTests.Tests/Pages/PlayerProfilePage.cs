using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents a player profile page, reached via search results navigation.
public class PlayerProfilePage(IPage page) : BasePage(page)
{
    private ILocator NameHeading => _page.Locator(".data-header h1");

    public string CurrentUrl => _page.Url;

    // Waits for the profile page's name heading to render.
    public async Task WaitForLoad()
    {
        await Expect(NameHeading).ToBeVisibleAsync();
    }

    public async Task<string> GetPlayerName()
    {
        return (await NameHeading.InnerTextAsync()).Trim();
    }
}
