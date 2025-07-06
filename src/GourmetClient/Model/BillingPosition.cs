namespace GourmetClient.Model
{
    using System;

    public record BillingPosition(
        DateTime Date,
        BillingPositionType PositionType,
        string PositionName,
        int Count,
        double SumCost);
}
