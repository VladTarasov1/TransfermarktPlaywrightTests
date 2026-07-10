namespace TransfermarktPlaywrightTests.Tests.Models.PageModels;

// A single club's row in a league overview table.
public record ClubRow(
    string ClubName,
    string ClubProfileUrl,
    int SquadSize,
    double AverageAge,
    int ForeignersCount,
    decimal AverageMarketValueEur,
    decimal TotalMarketValueEur);
