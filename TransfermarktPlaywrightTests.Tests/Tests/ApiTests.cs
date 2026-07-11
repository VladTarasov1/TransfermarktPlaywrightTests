using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Helpers;
using TransfermarktPlaywrightTests.Tests.Models.ApiModels;
using TransfermarktPlaywrightTests.Tests.Pages;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// API tests
[TestFixture]
public class ApiTests : PageTest
{
    // Transfermarkt's "England" quickselect competitions list.
    private const string EnglishCompetitionsUrl = "https://www.transfermarkt.com/quickselect/competitions/189";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // England's top four league tiers, in ascending order, mapped to their expected names.
    private static readonly Dictionary<string, string> TopFourLeagueNames = new()
    {
        ["GB1"] = "Premier League",
        ["GB2"] = "Championship",
        ["GB3"] = "League One",
        ["GB4"] = "League Two",
    };

    [Test]
    public async Task Get_EnglishCompetitions_RestRequest()
    {
        var response = await Context.APIRequest.GetAsync(EnglishCompetitionsUrl);

        // No server errors.
        Assert.That(response.Status, Is.EqualTo(200),
            $"Expected a successful response from '{EnglishCompetitionsUrl}', but got status {response.Status}.");

        // Competitions were returned.
        var competitions = JsonSerializer.Deserialize<List<CompetitionEntry>>(await response.TextAsync(), JsonOptions);
        Assert.That(competitions, Is.Not.Null.And.Not.Empty,
            "Expected the response to contain at least one competition.");

        Assert.Multiple(() =>
        {
            // Check duplicate competition ids.
            Assert.That(competitions.Select(competition => competition.Id).Distinct().Count(), Is.EqualTo(competitions.Count),
                "Expected no duplicate competition ids.");

            // Check each competition has a name and link.
            Assert.That(competitions, Has.All.Matches<CompetitionEntry>(
                    competition => !string.IsNullOrWhiteSpace(competition.Name) && !string.IsNullOrWhiteSpace(competition.Link)),
                "Expected every competition to have a non-empty name and link.");

            // Check exact names of England's top four league tiers.
            foreach (var (id, expectedName) in TopFourLeagueNames)
            {
                Assert.That(competitions.Single(competition => competition.Id == id).Name, Is.EqualTo(expectedName),
                    $"Expected '{id}' to be the {expectedName}.");
            }

            // GB1-GB4 must appear in ascending tier order.
            var topTierIds = competitions
                .Select(competition => competition.Id)
                .Where(TopFourLeagueNames.ContainsKey);
            Assert.That(topTierIds, Is.EqualTo(TopFourLeagueNames.Keys),
                "Expected England's top four league tiers to be listed in ascending tier order.");
        });
    }

    [Test]
    public async Task PremierLeagueTable_DuringLazyLoadedRequests_HasNoServerErrors()
    {
        await ConsentCookies.Seed(Context);
        var homePage = new HomePage(Page);
        await homePage.Navigate();
        await ConsentCookies.EnsureAccepted(Page);

        var leaguePage = await homePage.Header.OpenRecommendation("Premier League");
        await leaguePage.FilterBySeason("25/26");

        // Record responses triggered by the lazy-loaded sort request
        var responses = new List<IResponse>();
        Page.Response += (sender, response) => responses.Add(response);
        await leaguePage.SortByColumn("ø age");

        var serverErrors = responses.Where(response => response.Status >= 500).ToList();
        Assert.That(serverErrors, Is.Empty,
            $"Expected no server errors, but got: {string.Join(", ", serverErrors.Select(response => $"{response.Status} {response.Url}"))}");
    }
}
