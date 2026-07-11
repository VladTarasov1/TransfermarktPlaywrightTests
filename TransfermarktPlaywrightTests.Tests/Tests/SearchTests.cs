namespace TransfermarktPlaywrightTests.Tests.Tests;

// "Search Engine" tests
[TestFixture]
public class SearchTests : BaseTest
{
    [Test]
    public async Task Search_WithResults_ReturnsMatchesAndAllowsNavigation()
    {
        var resultsPage = await HomePage.Header.Search("Messi");
        await resultsPage.WaitForResults();

        // check results not empty
        Assert.That(await resultsPage.HasNoResults(), Is.False,
            "Expected 'Messi' to return search results.");

        // check that every visible category has a positive hit count
        var hitCounts = await resultsPage.GetCategoryHitCounts();
        Assert.That(hitCounts, Has.All.GreaterThan(0),
            "Expected every visible category's hit count to be positive.");

        // check players category results
        var players = await resultsPage.GetPlayerResults();
        Assert.Multiple(() =>
        {
            Assert.That(players, Is.Not.Empty,
                "Expected the Players category to have at least one result.");
            Assert.That(players.Select(player => player.Name), Does.Contain("Lionel Messi"),
                "Expected 'Lionel Messi' to appear among the player results.");
            Assert.That(players.Select(player => player.ProfileUrl), Has.All.Contains("/profil/"),
                "Expected every player result to link to a player profile page.");
        });

        // check that clicking a player result navigates to its profile page
        var profilePage = await resultsPage.OpenFirstPlayerResult();
        var profileName = await profilePage.GetPlayerName();
        var clickedPlayerSurname = players.First().Name.Split(' ').Last();

        Assert.Multiple(() =>
        {
            Assert.That(profilePage.CurrentUrl, Does.Contain("/profil/"),
                "Expected navigating to the first player result to reach a profile page.");
            Assert.That(profileName, Does.Contain(clickedPlayerSurname),
                "Expected the profile page heading to reflect the clicked player.");
        });
    }

    [Test]
    public async Task Search_IsCaseInsensitive()
    {
        var resultsPage = await HomePage.Header.Search("messi");
        await resultsPage.WaitForResults();

        var players = await resultsPage.GetPlayerResults();
        Assert.That(players.Select(player => player.Name), Does.Contain("Lionel Messi"),
            "Expected a lowercase query to still match 'Lionel Messi'.");
    }

    [Test]
    public async Task Search_WithNoMatches_ShowsNothingFoundGuidance()
    {
        var resultsPage = await HomePage.Header.Search("NonExistentQueryWithNoMatches");

        await resultsPage.WaitForNothingFound();
        Assert.That(await resultsPage.HasNoResults(), Is.True,
            "Expected an unmatched query to land on the 'no results' page.");
    }
}
