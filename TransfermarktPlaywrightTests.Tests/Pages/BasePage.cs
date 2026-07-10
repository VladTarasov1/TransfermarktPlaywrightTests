using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

public abstract class BasePage(IPage page)
{
    protected readonly IPage _page = page;

    private readonly Components.CookieBanner _cookieBanner = new(page);

    public Components.Header Header { get; } = new(page);

    public string CurrentUrl => _page.Url;

    // Dismisses the cookie/consent banner if present.
    public Task DismissCookieBanner() => _cookieBanner.Dismiss();

    // Dismisses the cookie/consent banner if it has reappeared after a subsequent page navigation,
    // without waiting when it doesn't - unlike DismissCookieBanner, its presence here isn't guaranteed.
    public Task DismissCookieBannerIfShown() => _cookieBanner.DismissIfShown();

    // Returns the current page's title.
    public Task<string> GetTitle() => _page.TitleAsync();

    // Navigates back to the previous page in browser history.
    public Task GoBack() => _page.GoBackAsync();
}
