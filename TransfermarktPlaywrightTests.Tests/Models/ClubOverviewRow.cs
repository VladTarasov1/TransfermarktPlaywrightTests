namespace TransfermarktPlaywrightTests.Tests.Models;

public record ClubsTableColumns(
    string ClubName,
    string ClubProfileUrl,
    int SquadSize,
    double AverageAge,
    int ForeignersCount,
    decimal AverageMarketValueEur,
    decimal TotalMarketValueEur);
