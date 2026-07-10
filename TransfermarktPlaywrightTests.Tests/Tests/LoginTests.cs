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
                "TM_TEST_USERNAME not set. Copy .env.example to .env at the repo root and fill in the test account credentials.");
        TestPassword = Environment.GetEnvironmentVariable("TM_TEST_PASSWORD")
            ?? throw new InvalidOperationException(
                "TM_TEST_PASSWORD not set. Copy .env.example to .env at the repo root and fill in the test account credentials.");
    }

    [SetUp]
    public async Task SetUp()
    {
        await ConsentCookies.Seed(Context);
        _homePage = new HomePage(Page);
        await _homePage.Navigate();
        await ConsentCookies.EnsureAccepted(Page);
        _loginPage = await _homePage.OpenLogin();
    }

    [Test]
    public async Task Login_WithValidCredentials_LogsUserIn()
    {
        await _loginPage.Login(TestUsername, TestPassword);

        Assert.That(_homePage.CurrentUrl, Is.EqualTo(HomePage.Url),
            "Expected a successful login to redirect to the homepage.");
        await _homePage.Header.WaitForLoggedIn();

        var profilePage = await _homePage.Header.OpenProfileSettings();

        Assert.That(await profilePage.GetTitle(), Is.EqualTo($"{TestUsername} - Profile settings | Transfermarkt"),
            "Expected the profile settings page to be reachable and to display the logged-in username.");
    }

    // TODO: not an expected behavior - make it's failing to highlight the issue
    [Test]
    public async Task Login_WithUsernameInDifferentCase_StillLogsUserIn()
    {
        await _loginPage.Login(TestUsername.ToUpperInvariant(), TestPassword);

        Assert.That(_homePage.CurrentUrl, Is.EqualTo(HomePage.Url),
            "Expected login to succeed even when the username's case doesn't match how it was registered.");
        await _homePage.Header.WaitForLoggedIn();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Login_RememberMe_ControlsSessionPersistence(bool rememberMe)
    {
        await _loginPage.Login(TestUsername, TestPassword, rememberMe);
        await _homePage.Header.WaitForLoggedIn();

        Assert.That(await _loginPage.HasPersistentSession(), Is.EqualTo(rememberMe),
            $"Expected the session cookie to {(rememberMe ? "persist" : "be session-only")} when 'Remember me' is {(rememberMe ? "checked" : "unchecked")}.");
    }

    [Test]
    public async Task Login_WithWrongCredentials_ShowsError()
    {
        await _loginPage.Login("WrongUser1", "WrongPass1!");

        var errors = await _loginPage.GetErrorMessages();
        Assert.That(errors, Has.Some.Contains("Incorrect username or password."),
            "Expected wrong credentials to be rejected with an 'incorrect username or password' error.");
    }

    [Test]
    public async Task Login_WithBothFieldsBlank_ShowsBothValidationErrors()
    {
        await _loginPage.Login("", "");

        var errors = await _loginPage.GetErrorMessages();
        Assert.That(errors, Has.Some.Contains("Username cannot be blank."));
        Assert.That(errors, Has.Some.Contains("Password cannot be blank."));
    }

    [TestCase("SomeUser", "", "Password cannot be blank.")]
    [TestCase("", "SomePass1!", "Username cannot be blank.")]
    public async Task Login_WithOneFieldBlank_ShowsValidationError(string username, string password, string expectedMessage)
    {
        await _loginPage.Login(username, password);

        var errors = await _loginPage.GetErrorMessages();
        Assert.That(errors, Has.Some.Contains(expectedMessage));
    }
}
