using Microsoft.Playwright;
using TransfermarktPlaywrightTests.Tests.PageModels;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Search results page.
public class SearchResultsPage(IPage page) : BasePage(page)
{
    // Result category tables.
    private ILocator ResultTables => _page.Locator("table.items");

    // "PLAYERS" results box.
    private ILocator PlayersBox => _page.Locator(".box", new() { HasText = "PLAYERS" });

    // Players results table.
    private ILocator PlayersTable => PlayersBox.Locator("table.items");

    // Each visible category's headline, e.g. "... - 10 HITS".
    private ILocator CategoryHeadlines => _page.Locator(".box .content-box-headline");

    // Heading shown when a search query has no matches.
    private ILocator NothingFoundHeading => _page.GetByText("Nothing found?");

    // Player name links within the players results table.
    private ILocator PlayerNameLinks => PlayersTable.Locator("tbody tr td.hauptlink a");

    // True when no result tables were rendered for the query.
    public async Task<bool> HasNoResults()
    {
        return await ResultTables.CountAsync() == 0;
    }

    // Waits for the "Nothing found?" panel.
    public async Task WaitForNothingFound()
    {
        await Expect(NothingFoundHeading).ToBeVisibleAsync();
    }

    // Waits for at least one result category.
    public async Task WaitForResults()
    {
        await Expect(CategoryHeadlines.First).ToBeVisibleAsync();
    }

    // Returns the hit count stated in each visible category headline, e.g. "... - 10 HITS".
    public async Task<List<int>> GetCategoryHitCounts()
    {
        var headlines = await CategoryHeadlines.AllInnerTextsAsync();
        return [.. headlines.Select(ParseHitCount)];
    }

    // Returns the Players category results as a typed objects.
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

    // Clicks the first Player result and returns the resulting profile page.
    public async Task<PlayerProfilePage> OpenFirstPlayerResult()
    {
        await PlayerNameLinks.First.ClickAsync();

        var profilePage = new PlayerProfilePage(_page);
        await profilePage.WaitForLoad();

        return profilePage;
    }

    // Gets the integer hit count from the headline text.
    private static int ParseHitCount(string headlineText)
    {
        var countText = headlineText.Split(" - ").Last().Split(' ').First();
        return int.Parse(countText);
    }
}
