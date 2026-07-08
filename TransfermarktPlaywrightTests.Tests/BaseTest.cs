using Microsoft.Playwright.NUnit;

namespace TransfermarktPlaywrightTests.Tests;

[TestFixture]
public class SmokeTest : PageTest
{
    [Test]
    public async Task HomepageLoads()
    {
        await Page.GotoAsync("https://www.transfermarkt.com/");
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Transfermarkt.*"));
    }
}