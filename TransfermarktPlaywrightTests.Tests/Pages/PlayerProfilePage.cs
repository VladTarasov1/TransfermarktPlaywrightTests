using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Player profile page.
public class PlayerProfilePage(IPage page) : BasePage(page)
{
    // Name heading.
    private ILocator NameHeading => _page.Locator(".data-header h1");

    // Waits for the profile page's name heading to render.
    public async Task WaitForLoad()
    {
        await Expect(NameHeading).ToBeVisibleAsync();
    }

    // Returns the player's name.
    public async Task<string> GetPlayerName()
    {
        return (await NameHeading.InnerTextAsync()).Trim();
    }
}
