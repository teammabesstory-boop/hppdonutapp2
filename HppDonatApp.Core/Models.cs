using System;
using System.Collections.Generic;

namespace HppDonatApp.Core;

public sealed record RecipeItem(Guid IngredientId, decimal Quantity, string Unit, decimal PricePerUnit);

public sealed record LaborRole(string Name, decimal Hours, decimal Rate);

public sealed record BatchRequest(
    IEnumerable<RecipeItem> Items,
    decimal BatchMultiplier,
    decimal OilUsedLiters,
    decimal OilPricePerLiter,
    decimal OilChangeCost,
    int BatchesPerOilChange,
    decimal EnergyKwh,
    decimal EnergyRatePerKwh,
    IEnumerable<LaborRole> Labor,
    decimal OverheadAllocated,
    int TheoreticalOutput,
    decimal WastePercent,
    decimal PackagingPerUnit,
    decimal Markup,
    decimal VatPercent,
    string RoundingRule);

public sealed record BatchCostResult(
    decimal IngredientCost,
    decimal OilCost,
    decimal OilAmortization,
    decimal EnergyCost,
    decimal LaborCost,
    decimal OverheadCost,
    decimal PackagingCost,
    decimal TotalBatchCost,
    decimal UnitCost,
    int SellableUnits,
    decimal SuggestedPrice,
    decimal PriceIncVat,
    decimal Margin,
    IReadOnlyDictionary<string, decimal> BreakdownDictionary);

public enum RoundingRuleType
{
    None,
    Nearest100,
    Nearest500,
    Nearest1000,
    Psychological990,
    Significant2
}
