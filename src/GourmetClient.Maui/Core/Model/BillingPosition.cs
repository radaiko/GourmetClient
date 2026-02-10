using System;

namespace GourmetClient.Maui.Core.Model;

public record BillingPosition(
    DateTime Date,
    BillingPositionType PositionType,
    string PositionName,
    int Count,
    double SumCost);