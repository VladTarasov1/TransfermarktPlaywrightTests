using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

/// <summary>
/// Represents the "Clubs - Premier League" overview table
/// </summary>
public class PremierLeaguePage
{
    private readonly IPage _page;

    public const string Url = "https://www.transfermarkt.com/premier-league/startseite/wettbewerb/GB1";

    private ILocator Table => _page.Locator("table.items").First;
    private ILocator HeaderCells => Table.Locator("thead th");
    private ILocator BodyRows => Table.Locator("tbody tr");

    public PremierLeaguePage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Dismisses an ad if one appears.
    /// </summary>
    public async Task DismissAdIfPresent()
    {
        foreach (var frame in _page.Frames)
        {
            var closeAdButton = frame.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("Close") });
            try
            {
                await closeAdButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1500 });
                await closeAdButton.ClickAsync();
                return;
            }
            catch (TimeoutException)
            {
                // No ad in this frame - check the next one.
            }
        }
    }

    public async Task AssertClubCountAsync(int expectedCount)
    {
        await Expect(BodyRows).ToHaveCountAsync(expectedCount);
    }

    public ILocator GetRows() => BodyRows;

    public async Task<List<string>> GetColumnHeaders()
    {
        return (await HeaderCells.AllInnerTextsAsync())
            .Select(h => h.Trim())
            .Where(h => !string.IsNullOrEmpty(h))
            .ToList();
    }

    public async Task<List<string>> GetClubNames()
    {
        var links = BodyRows.Locator("td.hauptlink.no-border-links a");
        return (await links.AllInnerTextsAsync())
            .Select(n => n.Trim())
            .ToList();
    }
}
