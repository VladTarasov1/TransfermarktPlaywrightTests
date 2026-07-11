using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Helpers;
using TransfermarktPlaywrightTests.Tests.Pages;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// Base fixture for UI tests: every test starts on the homepage with cookie consent pre-seeded.
public abstract class BaseTest : PageTest
{
    // The homepage page object.
    protected HomePage HomePage => new(Page);

    // Runs before any derived fixture's own [SetUp] (NUnit runs base-class setups first).
    [SetUp]
    public async Task OpenHomePage()
    {
        await ConsentCookies.Seed(Context);
        await HomePage.Navigate();
        await ConsentCookies.EnsureAccepted(Page);
    }
}
