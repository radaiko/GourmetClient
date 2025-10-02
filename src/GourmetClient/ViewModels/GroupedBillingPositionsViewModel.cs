using GourmetClient.Core.Model;

namespace GourmetClient.ViewModels;

public record GroupedBillingPositionsViewModel(
    BillingPositionType PositionType,
    string PositionName,
    int Count,
    double SingleCost,
    double SumCost);