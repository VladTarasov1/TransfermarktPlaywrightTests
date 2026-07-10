using System.Text.Json;
using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Models.ApiModels;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// API tests
[TestFixture]
public class ApiTests : PageTest
{
    // Transfermarkt's own JSON endpoint backing the "England" quickselect competitions list.
    private const string EnglishCompetitionsUrl = "https://www.transfermarkt.com/quickselect/competitions/189";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // England's top four league tiers, in ascending order.
    private static readonly string[] TopFourTierIds = ["GB1", "GB2", "GB3", "GB4"];

    [Test]
    public async Task EnglishCompetitions_RestRequest_HasNoServerErrorAndTopFourTiersInOrder()
    {
        var response = await Context.APIRequest.GetAsync(EnglishCompetitionsUrl);

        Assert.That(response.Status, Is.EqualTo(200),
            $"Expected a successful response from '{EnglishCompetitionsUrl}', but got status {response.Status}.");

        var competitions = JsonSerializer.Deserialize<List<CompetitionEntry>>(await response.TextAsync(), JsonOptions);
        Assert.That(competitions, Is.Not.Null.And.Not.Empty,
            "Expected the response to contain at least one competition.");

        // GB1-GB4 are the Premier League, Championship, League One and League Two - England's top four league tiers, in that order.
        var topTierIds = competitions!
            .Select(competition => competition.Id)
            .Where(TopFourTierIds.Contains);

        Assert.That(topTierIds, Is.EqualTo(TopFourTierIds),
            "Expected England's top four league tiers to be listed in ascending tier order.");
    }
}
