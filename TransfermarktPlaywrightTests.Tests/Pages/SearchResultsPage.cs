using Microsoft.Playwright;
using TransfermarktPlaywrightTests.Tests.Models;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the quick-search results page, in both its "has results" and "no results" states.
public class SearchResultsPage(IPage page) : BasePage(page)
{
    private ILocator ResultTables => _page.Locator("table.items");
    private ILocator PlayersBox => _page.Locator(".box", new() { HasText = "PLAYERS" });
    private ILocator PlayersTable => PlayersBox.Locator("table.items");
    private ILocator CategoryHeadlines => _page.Locator(".box .content-box-headline");
    private ILocator NothingFoundHeading => _page.GetByText("Nothing found?");
    private ILocator PlayerNameLinks => PlayersTable.Locator("tbody tr td.hauptlink a");

    // True when no result tables were rendered for the query.
    public async Task<bool> HasNoResults()
    {
        return await ResultTables.CountAsync() == 0;
    }

    // Waits for the "Nothing found?" guidance panel to render.
    public async Task WaitForNothingFound()
    {
        await Expect(NothingFoundHeading).ToBeVisibleAsync();
    }

    // Returns the hit count stated in each visible category headline, e.g. "... - 10 HITS".
    public async Task<List<int>> GetCategoryHitCounts()
    {
        var headlines = await CategoryHeadlines.AllInnerTextsAsync();
        return [.. headlines.Select(ParseHitCount)];
    }

    // Returns the Players category results.
    public async Task<List<PlayerSearchResult>> GetPlayerResults()
    {
        var count = await PlayerNameLinks.CountAsync();
        var results = new List<PlayerSearchResult>(count);

        for (var i = 0; i < count; i++)
        {
            results.Add(new PlayerSearchResult(
                Name: (await PlayerNameLinks.Nth(i).InnerTextAsync()).Trim(),
                ProfileUrl: await PlayerNameLinks.Nth(i).GetAttributeAsync("href") ?? string.Empty));
        }

        return results;
    }

    // Clicks the first Players result and returns the resulting profile page.
    public async Task<PlayerProfilePage> OpenFirstPlayerResult()
    {
        await PlayerNameLinks.First.ClickAsync();

        var profilePage = new PlayerProfilePage(_page);
        await profilePage.WaitForLoad();

        return profilePage;
    }

    // Headline text always ends with "- <count> HIT(S)", e.g. "SEARCH RESULTS FOR PLAYERS - 246 HITS".
    private static int ParseHitCount(string headlineText)
    {
        var countText = headlineText.Split(" - ").Last().Split(' ').First();
        return int.Parse(countText);
    }
}
