using Microsoft.Playwright;

namespace TransfermarktPlaywrightTests.Tests.Pages;

/// <summary>
/// Represents the "Clubs - Premier League" overview table
/// </summary>
public class PremierLeaguePage
{
    private readonly IPage _page;

    private ILocator PageHeading =>
        _page.GetByRole(AriaRole.Heading, new() { Name = "Clubs - Premier League" });

    private ILocator Table => _page.Locator("table.items").First;
    private ILocator HeaderCells => Table.Locator("thead th");
    private ILocator BodyRows => Table.Locator("tbody tr");

    // Ad interstitials use a dynamic google_ads_iframe_ id suffix, so match by prefix.
    private IFrameLocator AdInterstitialFrame =>
        _page.FrameLocator("iframe[id^='google_ads_iframe_']");

    public PremierLeaguePage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Dismisses an ad interstitial if one appears. These are
    /// non-deterministic (ad-serving dependent), so tolerate absence.
    /// </summary>
    public async Task DismissAdInterstitialIfPresentAsync()
    {
        try
        {
            await AdInterstitialFrame
                .GetByRole(AriaRole.Button, new() { Name = "Close ad" })
                .ClickAsync(new() { Timeout = 5000 });
        }
        catch (TimeoutException)
        {
            // No ad shown this run - safe to ignore.
        }
    }

    public async Task<bool> IsDisplayedAsync()
    {
        return await PageHeading.IsVisibleAsync();
    }

    public async Task<int> GetRowCountAsync()
    {
        return await BodyRows.CountAsync();
    }

    public ILocator GetRows() => BodyRows;

    public async Task<List<string>> GetColumnHeadersAsync()
    {
        return (await HeaderCells.AllInnerTextsAsync())
            .Select(h => h.Trim())
            .Where(h => !string.IsNullOrEmpty(h))
            .ToList();
    }

    public async Task<List<string>> GetClubNamesAsync()
    {
        var links = BodyRows.Locator("td.hauptlink.no-border-links a");
        return (await links.AllInnerTextsAsync())
            .Select(n => n.Trim())
            .ToList();
    }
}
