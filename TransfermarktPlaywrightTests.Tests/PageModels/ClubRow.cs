namespace TransfermarktPlaywrightTests.Tests.PageModels;

public record ClubRow(
    string ClubName,
    string ClubProfileUrl,
    int SquadSize,
    double AverageAge,
    int ForeignersCount,
    decimal AverageMarketValueEur,
    decimal TotalMarketValueEur);
