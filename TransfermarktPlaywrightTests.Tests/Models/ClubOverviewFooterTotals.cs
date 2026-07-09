namespace TransfermarktPlaywrightTests.Tests.Models;

public record ClubsTableSummaryColumns(
    int SquadTotal,
    double AverageAge,
    int ForeignersTotal,
    decimal AverageMarketValueEur,
    decimal TotalMarketValueEur);
