using Microsoft.Playwright;
using TransfermarktPlaywrightTests.Tests.Models.PageModels;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents each country's league page with a table.
public class LeaguePage(IPage page) : BasePage(page)
{
    // The club overview table.
    private ILocator Table => _page.Locator(".responsive-table table.items");

    // The table's column header cells.
    private ILocator ColumnHeaders => Table.Locator("thead th:not(.hide)");

    // The table's club rows.
    private ILocator Rows => Table.Locator("tbody tr");

    // The table's footer summary cells.
    private ILocator FooterSummaryCells => Table.Locator("tfoot td");

    // The season-picker dropdown.
    private ILocator SeasonDropdown => _page.Locator("select[name='saison_id'] + div.chzn-container");

    // Submits the season filter form.
    private ILocator ShowButton => _page.Locator("input[type='submit'][value='Show']");

    // The sortable link in a column header, e.g. "ø age".
    private ILocator SortLink(string columnName) => Table.Locator("thead th a.sort-link", new() { HasText = columnName });

    // Opens the season dropdown, picks the given season (its display text must match exactly, e.g. "25/26"), and submits the form.
    public async Task FilterBySeason(string season)
    {
        await SeasonDropdown.Locator("a.chzn-single").ClickAsync();
        await SeasonDropdown.Locator("li.active-result", new() { HasText = season }).ClickAsync();
        await ShowButton.ClickAsync();
        await Expect(Rows.First).ToBeVisibleAsync();
    }

    // Sorts by the given column (e.g. "ø age"); waits for the old rows to detach.
    public async Task SortByColumn(string columnName)
    {
        var sortLink = SortLink(columnName);
        var sortKey = (await sortLink.GetAttributeAsync("href"))?.Split('/').Last() ?? string.Empty;
        var previousRows = await Rows.First.ElementHandleAsync();

        var sortResponse = _page.WaitForResponseAsync(response => response.Url.Contains($"sort/{sortKey}"));
        await sortLink.ClickAsync();
        await sortResponse;

        await previousRows.WaitForElementStateAsync(ElementState.Hidden);
    }

    // Asserts that the number of club rows matches the expected count.
    public async Task AssertClubCount(int expectedCount)
    {
        await Expect(Rows).ToHaveCountAsync(expectedCount);
    }

    // Asserts that the club table has rendered with at least one row.
    public async Task AssertTableVisible()
    {
        await Expect(Rows.First).ToBeVisibleAsync();
    }

    // Returns the column headers list in the order they appear in the table.
    public async Task<List<string>> GetColumnHeaders()
    {
        return [.. (await ColumnHeaders.AllInnerTextsAsync())
            .Select(header => header.Trim())
            .Where(header => !string.IsNullOrEmpty(header))];
    }

    // Gets the club overview table rows as a list of typed objects.
    public async Task<List<ClubRow>> GetClubRows()
    {
        var rowCount = await Rows.CountAsync();
        var rows = new List<ClubRow>(rowCount);

        for (var i = 0; i < rowCount; i++)
        {
            var cells = Rows.Nth(i).Locator("td");
            var nameLink = cells.Nth(1).Locator("a").First;

            rows.Add(new ClubRow(
                ClubName: (await nameLink.InnerTextAsync()).Trim(),
                ClubProfileUrl: await nameLink.GetAttributeAsync("href") ?? string.Empty,
                SquadSize: int.Parse((await cells.Nth(2).InnerTextAsync()).Trim()),
                AverageAge: double.Parse((await cells.Nth(3).InnerTextAsync()).Trim()),
                ForeignersCount: int.Parse((await cells.Nth(4).InnerTextAsync()).Trim()),
                AverageMarketValueEur: ParseMarketValue(await cells.Nth(5).InnerTextAsync()),
                TotalMarketValueEur: ParseMarketValue(await cells.Nth(6).InnerTextAsync())));
        }

        return rows;
    }

    // Gets the footer summary row as a typed object.
    public async Task<ClubFooterTotals> GetFooterTotals()
    {
        var ageText = (await FooterSummaryCells.Nth(3).InnerTextAsync()).Trim();

        return new ClubFooterTotals(
            SquadTotal: int.Parse((await FooterSummaryCells.Nth(2).InnerTextAsync()).Trim()),
            AverageAge: double.Parse(ageText.Split(' ')[0]),
            ForeignersTotal: int.Parse((await FooterSummaryCells.Nth(4).InnerTextAsync()).Trim()),
            AverageMarketValueEur: ParseMarketValue(await FooterSummaryCells.Nth(5).InnerTextAsync()),
            TotalMarketValueEur: ParseMarketValue(await FooterSummaryCells.Nth(6).InnerTextAsync()));
    }

    // Parses a market value string like "€1.23bn" or "€456m" into a decimal number of euros.
    private static decimal ParseMarketValue(string rawText)
    {
        var text = rawText.Trim().TrimStart('€');
        if (text is "-" or "")
        {
            return 0m;
        }

        decimal multiplier;
        if (text.EndsWith("bn", StringComparison.Ordinal))
        {
            multiplier = 1_000_000_000m;
            text = text[..^2];
        }
        else if (text.EndsWith('m'))
        {
            multiplier = 1_000_000m;
            text = text[..^1];
        }
        else
        {
            multiplier = 1m;
        }

        return decimal.Parse(text) * multiplier;
    }
}
