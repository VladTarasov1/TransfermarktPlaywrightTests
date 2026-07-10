# TransfermarktPlaywrightTests

Automated UI tests for [transfermarkt.com](https://www.transfermarkt.com/), built with C# and Playwright.

## Tech stack

- **.NET 10** / C# (latest language version, nullable reference types enabled)
- **[Microsoft.Playwright](https://playwright.dev/dotnet/)** + **Microsoft.Playwright.NUnit** — browser automation and the NUnit test adapter (`PageTest` base class gives each test its own isolated browser context)
- **NUnit** — test framework and runner
- **[DotNetEnv](https://github.com/tonerdo/dotnet-env)** — loads credentials from a local `.env` file at test startup

## Project structure

```
TransfermarktPlaywrightTests.Tests/
├── Pages/                # Page Object Model
│   ├── Components/       # Header.cs - the site header, shared across every page
│   ├── BasePage.cs       # Page-agnostic utilities + Header, inherited by every page object
│   ├── HomePage.cs, LoginPage.cs, LeaguePage.cs, SearchResultsPage.cs, ...
├── PageModels/            # Plain data records returned by page objects (ClubRow, PlayerSearchResult, ...)
├── Helpers/                # Test-infrastructure helpers not tied to any one page (ConsentCookies.cs)
├── Tests/                  # NUnit test fixtures (one per feature area)
└── playwright.runsettings  # Browser/headless configuration
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Playwright's browser binaries (installed once, see below)

## Setup

1. Restore dependencies:
   ```
   dotnet restore
   ```
2. Install the Playwright browsers (one-time, after the first build):
   ```
   dotnet build
   pwsh TransfermarktPlaywrightTests.Tests/bin/Debug/net10.0/playwright.ps1 install
   ```
3. Set up your `.env` file (required for the login tests — see below).

## Environment variables (`.env`)

The login tests (`LoginTests.cs`) sign in with a real Transfermarkt test account. Credentials are never hardcoded or committed — they're loaded from a `.env` file at the repo root, which is gitignored.

1. Copy `.env.example` to a new file named `.env` at the repo root.
2. Fill in your test account's credentials in `.env`:
   ```
   TM_TEST_USERNAME=your-test-username
   TM_TEST_PASSWORD=your-test-password
   ```

If `.env` is missing or incomplete, the login tests fail fast at setup with a clear error instead of failing confusingly partway through a test.

## Running tests

Run the whole suite:
```
dotnet test
```

Run a single fixture:
```
dotnet test --filter "FullyQualifiedName~LoginTests"
```

Run a single test:
```
dotnet test --filter "Login_WithValidCredentials_LogsUserIn"
```

Browser behavior (headless/headed, browser engine) is configured in `TransfermarktPlaywrightTests.Tests/playwright.runsettings` and picked up automatically — pass it explicitly if your tooling doesn't:
```
dotnet test --settings TransfermarktPlaywrightTests.Tests/playwright.runsettings
```

## Test coverage

Each feature area has its own NUnit fixture under `Tests/`:

- **`TableTests`** — Premier League table data (columns, club rows, sorting, footer totals)
- **`SearchTests`** — quick-search results (matches, case-insensitivity, no-results state, navigating to a result)
- **`NavigationTests`** — top nav links, hamburger menu recommendations, logo/back navigation
- **`LoginTests`** — login form happy path, profile verification, username case-insensitivity, "remember me", validation errors (wrong/blank credentials)

## MCP servers

`.mcp.json` configures two project-scoped MCP servers for AI-assisted development (e.g. Claude Code) — approve them when prompted on session start:

- **`playwright`** ([`@playwright/mcp`](https://github.com/microsoft/playwright-mcp)) — semantic browser automation (navigate, click, fill, snapshot, read cookies). Useful for exploring the live site's DOM/shadow-DOM structure when writing or debugging page objects.
- **`chrome-devtools`** ([`chrome-devtools-mcp`](https://github.com/ChromeDevTools/chrome-devtools-mcp)) — CDP-level inspection (network requests, console messages, performance traces). Requires Node ≥20.19.0 LTS.

Both are optional.

## Notes

- These tests run against the **live site**, not a mock — occasional failures unrelated to the code under test (e.g. a transient `504 Gateway Time-out`) can happen and are worth re-running before assuming a regression.
- The Sourcepoint cookie-consent banner is suppressed by seeding cookies (`Helpers/ConsentCookies.cs`) rather than clicking through it, to avoid a flaky UI interaction.
