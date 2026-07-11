using DotNetEnv;
using Microsoft.Playwright.NUnit;
using TransfermarktPlaywrightTests.Tests.Pages;
using TransfermarktPlaywrightTests.Tests.Helpers;

namespace TransfermarktPlaywrightTests.Tests.Tests;

// "Login form" tests
[TestFixture]
public class LoginTests : PageTest
{
    private static string TestUsername = null!;
    private static string TestPassword = null!;

    private HomePage _homePage = null!;
    private LoginPage _loginPage = null!;

    [OneTimeSetUp]
    public void LoadCredentials()
    {
        Env.TraversePath().Load();

        TestUsername = Environment.GetEnvironmentVariable("TM_TEST_USERNAME")
            ?? throw new InvalidOperationException(
                "TM_TEST_USERNAME not set.");
        TestPassword = Environment.GetEnvironmentVariable("TM_TEST_PASSWORD")
            ?? throw new InvalidOperationException(
                "TM_TEST_PASSWORD not set.");
    }

    [SetUp]
    public async Task SetUp()
    {
        await ConsentCookies.Seed(Context);
        _homePage = new HomePage(Page);
        await _homePage.Navigate();
        await ConsentCookies.EnsureAccepted(Page);
        _loginPage = await _homePage.Header.OpenLogin();
    }

    [Test]
    public async Task Login_WithValidCredentials()
    {
        await _loginPage.Login(TestUsername, TestPassword);

        await _homePage.Header.WaitForLoggedIn();
        var profilePage = await _homePage.Header.OpenProfileSettings();

        Assert.That(await profilePage.GetTitle(), Is.EqualTo($"{TestUsername} - Profile settings | Transfermarkt"),
            "Expected the profile settings page to be reachable and to display the logged-in username.");
    }

    // BUG: usernames should be case-sensitive, but login succeeds regardless of casing.
    [Test]
    public async Task Case_Sensitive_Login()
    {
        await _loginPage.Login(TestUsername.ToUpperInvariant(), TestPassword);

        Assert.That(await _homePage.Header.IsLoggedIn(), Is.False,
            "Expected a username with different casing to be rejected.");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Login_With_RememberMe_Option(bool rememberMe)
    {
        await _loginPage.Login(TestUsername, TestPassword, rememberMe);
        await _homePage.Header.WaitForLoggedIn();

        Assert.That(await _loginPage.HasPersistentSession(), Is.EqualTo(rememberMe),
            $"Expected the session cookie to persist only when 'Remember me' is checked (rememberMe={rememberMe}).");
    }

    [Test]
    public async Task Login_WithWrongCredentials()
    {
        await _loginPage.Login("WrongUser1", "WrongPass1!");

        var error = await _loginPage.GetErrorMessage();
        var isLoggedIn = await _homePage.Header.IsLoggedIn();

        Assert.Multiple(() =>
        {
            Assert.That(error, Does.Contain("Incorrect username or password."),
                "Expected wrong credentials to be rejected with an 'incorrect username or password' error.");
            Assert.That(isLoggedIn, Is.False,
                "Expected wrong credentials to not log the user in.");
        });
    }

    // The overlay validates blank fields client-side by disabling the button, not via a server error.
    [TestCase("", "")]
    [TestCase("SomeUser", "")]
    [TestCase("", "SomePass1!")]
    public async Task Login_WithBlankFields(string username, string password)
    {
        await _loginPage.FillCredentials(username, password);

        Assert.That(await _loginPage.IsSubmitDisabled(), Is.True,
            "Expected the login button to stay disabled until both fields are filled.");
    }
}
