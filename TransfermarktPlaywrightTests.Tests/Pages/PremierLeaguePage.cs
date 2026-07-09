using System.Globalization;
using Microsoft.Playwright;
using TransfermarktPlaywrightTests.Tests.Models;
using static Microsoft.Playwright.Assertions;

namespace TransfermarktPlaywrightTests.Tests.Pages;

// Represents the "Clubs - Premier League" squad/market-value overview table
public class PremierLeaguePage : BasePage
{
    public const string Url = "https://www.transfermarkt.com/premier-league/startseite/wettbewerb/GB1";
    private ILocator Table => _page.Locator("table.items").First;
    private ILocator HeaderCells => Table.Locator("thead th:not(.hide)");
    private ILocator BodyRows => Table.Locator("tbody tr");
    private ILocator FooterCells => Table.Locator("tfoot td");

    // The season <select> is hidden and driven by a "chosen.js" fake dropdown UI next to it.
    private ILocator SeasonDropdown => _page.Locator("select[name='saison_id'] + div.chzn-container");
    private ILocator ShowButton => _page.Locator("input[type='submit'][value='Show']");

    public PremierLeaguePage(IPage page) : base(page)
    {
    }

    public async Task FilterBySeason(string season)
    {
        await SeasonDropdown.Locator("a.chzn-single").ClickAsync();
        await SeasonDropdown.Locator("li.active-result", new() { HasText = season }).ClickAsync();
        await ShowButton.ClickAsync();
        await Expect(BodyRows.First).ToBeVisibleAsync();
    }

    public async Task AssertClubCount(int expectedCount)
    {
        await Expect(BodyRows).ToHaveCountAsync(expectedCount);
    }

    public async Task<List<string>> GetColumnHeaders()
    {
        return (await HeaderCells.AllInnerTextsAsync())
            .Select(h => h.Trim())
            .Where(h => !string.IsNullOrEmpty(h))
            .ToList();
    }

    public async Task<List<ClubsTableColumns>> GetClubOverviewRows()
    {
        var rowCount = await BodyRows.CountAsync();
        var rows = new List<ClubsTableColumns>(rowCount);

        for (var i = 0; i < rowCount; i++)
        {
            var cells = BodyRows.Nth(i).Locator("td");
            var nameLink = cells.Nth(1).Locator("a").First;

            rows.Add(new ClubsTableColumns(
                ClubName: (await nameLink.InnerTextAsync()).Trim(),
                ClubProfileUrl: await nameLink.GetAttributeAsync("href") ?? string.Empty,
                SquadSize: int.Parse((await cells.Nth(2).InnerTextAsync()).Trim(), CultureInfo.InvariantCulture),
                AverageAge: double.Parse((await cells.Nth(3).InnerTextAsync()).Trim(), CultureInfo.InvariantCulture),
                ForeignersCount: int.Parse((await cells.Nth(4).InnerTextAsync()).Trim(), CultureInfo.InvariantCulture),
                AverageMarketValueEur: ParseMarketValue(await cells.Nth(5).InnerTextAsync()),
                TotalMarketValueEur: ParseMarketValue(await cells.Nth(6).InnerTextAsync())));
        }

        return rows;
    }

    public async Task<ClubsTableSummaryColumns> GetFooterTotals()
    {
        var ageText = (await FooterCells.Nth(3).InnerTextAsync()).Trim();

        return new ClubsTableSummaryColumns(
            SquadTotal: int.Parse((await FooterCells.Nth(2).InnerTextAsync()).Trim(), CultureInfo.InvariantCulture),
            AverageAge: double.Parse(ageText.Split(' ')[0], CultureInfo.InvariantCulture),
            ForeignersTotal: int.Parse((await FooterCells.Nth(4).InnerTextAsync()).Trim(), CultureInfo.InvariantCulture),
            AverageMarketValueEur: ParseMarketValue(await FooterCells.Nth(5).InnerTextAsync()),
            TotalMarketValueEur: ParseMarketValue(await FooterCells.Nth(6).InnerTextAsync()));
    }

    public async Task<List<string>> GetCrestImageSources()
    {
        var crests = BodyRows.Locator("td.no-border-rechts img");
        var count = await crests.CountAsync();
        var sources = new List<string>(count);

        for (var i = 0; i < count; i++)
        {
            sources.Add(await crests.Nth(i).GetAttributeAsync("src") ?? string.Empty);
        }

        return sources;
    }

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
        else if (text.EndsWith('k'))
        {
            multiplier = 1_000m;
            text = text[..^1];
        }
        else
        {
            multiplier = 1m;
        }

        return decimal.Parse(text, CultureInfo.InvariantCulture) * multiplier;
    }
}
