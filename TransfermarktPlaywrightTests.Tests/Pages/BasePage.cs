using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Base page, inherited by every page object.
public abstract class BasePage(IPage page)
{
    protected readonly IPage _page = page;

    // The site header, present on every page.
    public Components.Header Header { get; } = new(page);

    // The browser's current URL.
    public string CurrentUrl => _page.Url;

    // Returns the current page's title.
    public Task<string> GetTitle() => _page.TitleAsync();

    // Navigates back to the previous page in browser history.
    public Task GoBack() => _page.GoBackAsync();
}
