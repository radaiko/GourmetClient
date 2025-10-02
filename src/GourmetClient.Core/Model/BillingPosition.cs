using System;

namespace GourmetClient.Core.Model;

public record BillingPosition(
    DateTime Date,
    BillingPositionType PositionType,
    string PositionName,
    int Count,
    double SumCost);