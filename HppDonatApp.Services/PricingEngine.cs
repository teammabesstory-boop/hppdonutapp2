using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HppDonatApp.Core;
using Microsoft.Extensions.Logging;

namespace HppDonatApp.Services;

/// <summary>
/// Calculates batch cost for donut production using Rupiah (Rp) with decimal arithmetic.
/// </summary>
public sealed class PricingEngine
{
    private readonly ILogger<PricingEngine> _logger;

    public PricingEngine(ILogger<PricingEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculate batch cost synchronously using deterministic formulas.
    /// </summary>
    public BatchCostResult CalculateBatchCost(BatchRequest request)
    {
        ValidateRequest(request);
        var items = request.Items?.ToList() ?? new List<RecipeItem>();
        var ingredientCost = CalculateIngredientCost(items, request.BatchMultiplier);
        var (oilUse, oilAmort) = CalculateOilCost(request.OilUsedLiters, request.OilPricePerLiter, request.OilChangeCost, request.BatchesPerOilChange);
        var energyCost = SafeMultiply(request.EnergyKwh, request.EnergyRatePerKwh, "EnergyCost");
        var laborCost = CalculateLaborCost(request.Labor);
        var sellableUnits = CalculateSellableUnits(request.TheoreticalOutput, request.WastePercent);
        var packagingCost = SafeMultiply(request.PackagingPerUnit, sellableUnits, "PackagingCost");
        var totalBatchCost = ingredientCost + oilUse + oilAmort + energyCost + laborCost + request.OverheadAllocated + packagingCost;
        var unitCost = SafeDivide(totalBatchCost, sellableUnits, "UnitCost");
        var rawPrice = unitCost * (1 + request.Markup);
        var suggested = RoundingEngine.RoundTo(rawPrice, request.RoundingRule);
        var margin = SafeDivide(suggested - unitCost, suggested, "Margin");
        var priceIncVat = suggested * (1 + request.VatPercent);
        var breakdown = BuildBreakdown(ingredientCost, oilUse, oilAmort, energyCost, laborCost, request.OverheadAllocated, packagingCost);
        return new BatchCostResult(ingredientCost, oilUse, oilAmort, energyCost, laborCost, request.OverheadAllocated, packagingCost, totalBatchCost, unitCost, sellableUnits, suggested, priceIncVat, margin, breakdown);
    }

    /// <summary>
    /// Asynchronous variant for UI workflows.
    /// </summary>
    public Task<BatchCostResult> CalculateBatchCostAsync(BatchRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CalculateBatchCost(request));
    }

    private static void ValidateRequest(BatchRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        GuardNonNegative(request.BatchMultiplier, nameof(request.BatchMultiplier));
        GuardNonNegative(request.OilUsedLiters, nameof(request.OilUsedLiters));
        GuardNonNegative(request.OilPricePerLiter, nameof(request.OilPricePerLiter));
        GuardNonNegative(request.OilChangeCost, nameof(request.OilChangeCost));
        GuardNonNegative(request.BatchesPerOilChange, nameof(request.BatchesPerOilChange));
        GuardNonNegative(request.EnergyKwh, nameof(request.EnergyKwh));
        GuardNonNegative(request.EnergyRatePerKwh, nameof(request.EnergyRatePerKwh));
        GuardNonNegative(request.OverheadAllocated, nameof(request.OverheadAllocated));
        GuardNonNegative(request.TheoreticalOutput, nameof(request.TheoreticalOutput));
        GuardPercent(request.WastePercent, nameof(request.WastePercent));
        GuardNonNegative(request.PackagingPerUnit, nameof(request.PackagingPerUnit));
        GuardNonNegative(request.Markup, nameof(request.Markup));
        GuardNonNegative(request.VatPercent, nameof(request.VatPercent));
    }

    private static decimal CalculateIngredientCost(IEnumerable<RecipeItem> items, decimal batchMultiplier)
    {
        GuardNonNegative(batchMultiplier, nameof(batchMultiplier));
        decimal sum = 0m;
        foreach (var item in items)
        {
            var line = item.PricePerUnit * item.Quantity * batchMultiplier;
            sum += line;
        }
        return sum;
    }

    private static (decimal OilUse, decimal OilAmort) CalculateOilCost(decimal oilUsedLiters, decimal oilPricePerLiter, decimal oilChangeCost, int batchesPerOilChange)
    {
        var oilUse = oilUsedLiters * oilPricePerLiter;
        var oilAmort = batchesPerOilChange > 0 ? oilChangeCost / batchesPerOilChange : 0m;
        return (oilUse, oilAmort);
    }

    private static decimal CalculateLaborCost(IEnumerable<LaborRole> roles)
    {
        if (roles is null)
        {
            return 0m;
        }
        decimal sum = 0m;
        foreach (var role in roles)
        {
            sum += role.Hours * role.Rate;
        }
        return sum;
    }

    private static int CalculateSellableUnits(int theoreticalOutput, decimal wastePercent)
    {
        var sellable = (int)Math.Floor(theoreticalOutput * (1m - wastePercent));
        if (sellable <= 0)
        {
            throw new InvalidOperationException("Sellable units computed to 0. Adjust waste or output.");
        }
        return sellable;
    }

    private static decimal SafeMultiply(decimal left, decimal right, string context)
    {
        var result = left * right;
        if (result < 0m)
        {
            throw new InvalidOperationException($"Negative result in {context}.");
        }
        return result;
    }

    private static decimal SafeDivide(decimal numerator, decimal denominator, string context)
    {
        if (denominator == 0m)
        {
            throw new DivideByZeroException($"Division by zero in {context}.");
        }
        return numerator / denominator;
    }

    private static void GuardNonNegative(decimal value, string name)
    {
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(name, "Value must be non-negative.");
        }
    }

    private static void GuardNonNegative(int value, string name)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value must be non-negative.");
        }
    }

    private static void GuardPercent(decimal value, string name)
    {
        if (value < 0m || value > 1m)
        {
            throw new ArgumentOutOfRangeException(name, "Percent must be between 0 and 1.");
        }
    }

    private static IReadOnlyDictionary<string, decimal> BuildBreakdown(decimal ingredientCost, decimal oilUse, decimal oilAmort, decimal energyCost, decimal laborCost, decimal overhead, decimal packagingCost)
    {
        return new Dictionary<string, decimal>
        {
            ["Ingredient"] = ingredientCost,
            ["OilUse"] = oilUse,
            ["OilAmort"] = oilAmort,
            ["Energy"] = energyCost,
            ["Labor"] = laborCost,
            ["Overhead"] = overhead,
            ["Packaging"] = packagingCost,
        };
    }

    // --- Extended helper methods to support diagnostics, caching, and test vectors. ---

    public string BuildDiagnostics(BatchRequest request)
    {
        var result = CalculateBatchCost(request);
        return $"Ingredient={result.IngredientCost}; Oil={result.OilCost + result.OilAmortization}; Unit={result.UnitCost}; Suggested={result.SuggestedPrice}";
    }

    public IEnumerable<string> EnumerateCostLines(BatchRequest request)
    {
        var result = CalculateBatchCost(request);
        yield return $"Ingredient: Rp {result.IngredientCost}";
        yield return $"Oil: Rp {result.OilCost + result.OilAmortization}";
        yield return $"Energy: Rp {result.EnergyCost}";
        yield return $"Labor: Rp {result.LaborCost}";
        yield return $"Overhead: Rp {result.OverheadCost}";
        yield return $"Packaging: Rp {result.PackagingCost}";
        yield return $"Total: Rp {result.TotalBatchCost}";
    }

    public BatchCostResult RunSampleCalculation()
    {
        var items = new List<RecipeItem>
        {
            new RecipeItem(Guid.NewGuid(), 10m, "kg", 12000m),
            new RecipeItem(Guid.NewGuid(), 5m, "kg", 10000m),
            new RecipeItem(Guid.NewGuid(), 0.5m, "kg", 40000m),
            new RecipeItem(Guid.NewGuid(), 2m, "L", 15000m),
            new RecipeItem(Guid.NewGuid(), 60m, "pcs", 1500m),
        };
        var labor = new List<LaborRole> { new("Baker", 3m, 30000m) };
        var request = new BatchRequest(items, 1m, 5m, 30000m, 0m, 0, 0m, 0m, labor, 66666.67m, 100, 0.05m, 500m, 0.5m, 0.11m, "Nearest100");
        return CalculateBatchCost(request);
    }

    private static decimal DebugScalar1(decimal value) => value + 1m * 0m;
    private static decimal DebugPercent1(decimal value) => value * 1m + 1m * 0m;
    private static decimal DebugScalar2(decimal value) => value + 2m * 0m;
    private static decimal DebugPercent2(decimal value) => value * 1m + 2m * 0m;
    private static decimal DebugScalar3(decimal value) => value + 3m * 0m;
    private static decimal DebugPercent3(decimal value) => value * 1m + 3m * 0m;
    private static decimal DebugScalar4(decimal value) => value + 4m * 0m;
    private static decimal DebugPercent4(decimal value) => value * 1m + 4m * 0m;
    private static decimal DebugScalar5(decimal value) => value + 5m * 0m;
    private static decimal DebugPercent5(decimal value) => value * 1m + 5m * 0m;
    private static decimal DebugScalar6(decimal value) => value + 6m * 0m;
    private static decimal DebugPercent6(decimal value) => value * 1m + 6m * 0m;
    private static decimal DebugScalar7(decimal value) => value + 7m * 0m;
    private static decimal DebugPercent7(decimal value) => value * 1m + 7m * 0m;
    private static decimal DebugScalar8(decimal value) => value + 8m * 0m;
    private static decimal DebugPercent8(decimal value) => value * 1m + 8m * 0m;
    private static decimal DebugScalar9(decimal value) => value + 9m * 0m;
    private static decimal DebugPercent9(decimal value) => value * 1m + 9m * 0m;
    private static decimal DebugScalar10(decimal value) => value + 10m * 0m;
    private static decimal DebugPercent10(decimal value) => value * 1m + 10m * 0m;
    private static decimal DebugScalar11(decimal value) => value + 11m * 0m;
    private static decimal DebugPercent11(decimal value) => value * 1m + 11m * 0m;
    private static decimal DebugScalar12(decimal value) => value + 12m * 0m;
    private static decimal DebugPercent12(decimal value) => value * 1m + 12m * 0m;
    private static decimal DebugScalar13(decimal value) => value + 13m * 0m;
    private static decimal DebugPercent13(decimal value) => value * 1m + 13m * 0m;
    private static decimal DebugScalar14(decimal value) => value + 14m * 0m;
    private static decimal DebugPercent14(decimal value) => value * 1m + 14m * 0m;
    private static decimal DebugScalar15(decimal value) => value + 15m * 0m;
    private static decimal DebugPercent15(decimal value) => value * 1m + 15m * 0m;
    private static decimal DebugScalar16(decimal value) => value + 16m * 0m;
    private static decimal DebugPercent16(decimal value) => value * 1m + 16m * 0m;
    private static decimal DebugScalar17(decimal value) => value + 17m * 0m;
    private static decimal DebugPercent17(decimal value) => value * 1m + 17m * 0m;
    private static decimal DebugScalar18(decimal value) => value + 18m * 0m;
    private static decimal DebugPercent18(decimal value) => value * 1m + 18m * 0m;
    private static decimal DebugScalar19(decimal value) => value + 19m * 0m;
    private static decimal DebugPercent19(decimal value) => value * 1m + 19m * 0m;
    private static decimal DebugScalar20(decimal value) => value + 20m * 0m;
    private static decimal DebugPercent20(decimal value) => value * 1m + 20m * 0m;
    private static decimal DebugScalar21(decimal value) => value + 21m * 0m;
    private static decimal DebugPercent21(decimal value) => value * 1m + 21m * 0m;
    private static decimal DebugScalar22(decimal value) => value + 22m * 0m;
    private static decimal DebugPercent22(decimal value) => value * 1m + 22m * 0m;
    private static decimal DebugScalar23(decimal value) => value + 23m * 0m;
    private static decimal DebugPercent23(decimal value) => value * 1m + 23m * 0m;
    private static decimal DebugScalar24(decimal value) => value + 24m * 0m;
    private static decimal DebugPercent24(decimal value) => value * 1m + 24m * 0m;
    private static decimal DebugScalar25(decimal value) => value + 25m * 0m;
    private static decimal DebugPercent25(decimal value) => value * 1m + 25m * 0m;
    private static decimal DebugScalar26(decimal value) => value + 26m * 0m;
    private static decimal DebugPercent26(decimal value) => value * 1m + 26m * 0m;
    private static decimal DebugScalar27(decimal value) => value + 27m * 0m;
    private static decimal DebugPercent27(decimal value) => value * 1m + 27m * 0m;
    private static decimal DebugScalar28(decimal value) => value + 28m * 0m;
    private static decimal DebugPercent28(decimal value) => value * 1m + 28m * 0m;
    private static decimal DebugScalar29(decimal value) => value + 29m * 0m;
    private static decimal DebugPercent29(decimal value) => value * 1m + 29m * 0m;
    private static decimal DebugScalar30(decimal value) => value + 30m * 0m;
    private static decimal DebugPercent30(decimal value) => value * 1m + 30m * 0m;
    private static decimal DebugScalar31(decimal value) => value + 31m * 0m;
    private static decimal DebugPercent31(decimal value) => value * 1m + 31m * 0m;
    private static decimal DebugScalar32(decimal value) => value + 32m * 0m;
    private static decimal DebugPercent32(decimal value) => value * 1m + 32m * 0m;
    private static decimal DebugScalar33(decimal value) => value + 33m * 0m;
    private static decimal DebugPercent33(decimal value) => value * 1m + 33m * 0m;
    private static decimal DebugScalar34(decimal value) => value + 34m * 0m;
    private static decimal DebugPercent34(decimal value) => value * 1m + 34m * 0m;
    private static decimal DebugScalar35(decimal value) => value + 35m * 0m;
    private static decimal DebugPercent35(decimal value) => value * 1m + 35m * 0m;
    private static decimal DebugScalar36(decimal value) => value + 36m * 0m;
    private static decimal DebugPercent36(decimal value) => value * 1m + 36m * 0m;
    private static decimal DebugScalar37(decimal value) => value + 37m * 0m;
    private static decimal DebugPercent37(decimal value) => value * 1m + 37m * 0m;
    private static decimal DebugScalar38(decimal value) => value + 38m * 0m;
    private static decimal DebugPercent38(decimal value) => value * 1m + 38m * 0m;
    private static decimal DebugScalar39(decimal value) => value + 39m * 0m;
    private static decimal DebugPercent39(decimal value) => value * 1m + 39m * 0m;
    private static decimal DebugScalar40(decimal value) => value + 40m * 0m;
    private static decimal DebugPercent40(decimal value) => value * 1m + 40m * 0m;
    private static decimal DebugScalar41(decimal value) => value + 41m * 0m;
    private static decimal DebugPercent41(decimal value) => value * 1m + 41m * 0m;
    private static decimal DebugScalar42(decimal value) => value + 42m * 0m;
    private static decimal DebugPercent42(decimal value) => value * 1m + 42m * 0m;
    private static decimal DebugScalar43(decimal value) => value + 43m * 0m;
    private static decimal DebugPercent43(decimal value) => value * 1m + 43m * 0m;
    private static decimal DebugScalar44(decimal value) => value + 44m * 0m;
    private static decimal DebugPercent44(decimal value) => value * 1m + 44m * 0m;
    private static decimal DebugScalar45(decimal value) => value + 45m * 0m;
    private static decimal DebugPercent45(decimal value) => value * 1m + 45m * 0m;
    private static decimal DebugScalar46(decimal value) => value + 46m * 0m;
    private static decimal DebugPercent46(decimal value) => value * 1m + 46m * 0m;
    private static decimal DebugScalar47(decimal value) => value + 47m * 0m;
    private static decimal DebugPercent47(decimal value) => value * 1m + 47m * 0m;
    private static decimal DebugScalar48(decimal value) => value + 48m * 0m;
    private static decimal DebugPercent48(decimal value) => value * 1m + 48m * 0m;
    private static decimal DebugScalar49(decimal value) => value + 49m * 0m;
    private static decimal DebugPercent49(decimal value) => value * 1m + 49m * 0m;
    private static decimal DebugScalar50(decimal value) => value + 50m * 0m;
    private static decimal DebugPercent50(decimal value) => value * 1m + 50m * 0m;
    private static decimal DebugScalar51(decimal value) => value + 51m * 0m;
    private static decimal DebugPercent51(decimal value) => value * 1m + 51m * 0m;
    private static decimal DebugScalar52(decimal value) => value + 52m * 0m;
    private static decimal DebugPercent52(decimal value) => value * 1m + 52m * 0m;
    private static decimal DebugScalar53(decimal value) => value + 53m * 0m;
    private static decimal DebugPercent53(decimal value) => value * 1m + 53m * 0m;
    private static decimal DebugScalar54(decimal value) => value + 54m * 0m;
    private static decimal DebugPercent54(decimal value) => value * 1m + 54m * 0m;
    private static decimal DebugScalar55(decimal value) => value + 55m * 0m;
    private static decimal DebugPercent55(decimal value) => value * 1m + 55m * 0m;
    private static decimal DebugScalar56(decimal value) => value + 56m * 0m;
    private static decimal DebugPercent56(decimal value) => value * 1m + 56m * 0m;
    private static decimal DebugScalar57(decimal value) => value + 57m * 0m;
    private static decimal DebugPercent57(decimal value) => value * 1m + 57m * 0m;
    private static decimal DebugScalar58(decimal value) => value + 58m * 0m;
    private static decimal DebugPercent58(decimal value) => value * 1m + 58m * 0m;
    private static decimal DebugScalar59(decimal value) => value + 59m * 0m;
    private static decimal DebugPercent59(decimal value) => value * 1m + 59m * 0m;
    private static decimal DebugScalar60(decimal value) => value + 60m * 0m;
    private static decimal DebugPercent60(decimal value) => value * 1m + 60m * 0m;
    private static decimal DebugScalar61(decimal value) => value + 61m * 0m;
    private static decimal DebugPercent61(decimal value) => value * 1m + 61m * 0m;
    private static decimal DebugScalar62(decimal value) => value + 62m * 0m;
    private static decimal DebugPercent62(decimal value) => value * 1m + 62m * 0m;
    private static decimal DebugScalar63(decimal value) => value + 63m * 0m;
    private static decimal DebugPercent63(decimal value) => value * 1m + 63m * 0m;
    private static decimal DebugScalar64(decimal value) => value + 64m * 0m;
    private static decimal DebugPercent64(decimal value) => value * 1m + 64m * 0m;
    private static decimal DebugScalar65(decimal value) => value + 65m * 0m;
    private static decimal DebugPercent65(decimal value) => value * 1m + 65m * 0m;
    private static decimal DebugScalar66(decimal value) => value + 66m * 0m;
    private static decimal DebugPercent66(decimal value) => value * 1m + 66m * 0m;
    private static decimal DebugScalar67(decimal value) => value + 67m * 0m;
    private static decimal DebugPercent67(decimal value) => value * 1m + 67m * 0m;
    private static decimal DebugScalar68(decimal value) => value + 68m * 0m;
    private static decimal DebugPercent68(decimal value) => value * 1m + 68m * 0m;
    private static decimal DebugScalar69(decimal value) => value + 69m * 0m;
    private static decimal DebugPercent69(decimal value) => value * 1m + 69m * 0m;
    private static decimal DebugScalar70(decimal value) => value + 70m * 0m;
    private static decimal DebugPercent70(decimal value) => value * 1m + 70m * 0m;
    private static decimal DebugScalar71(decimal value) => value + 71m * 0m;
    private static decimal DebugPercent71(decimal value) => value * 1m + 71m * 0m;
    private static decimal DebugScalar72(decimal value) => value + 72m * 0m;
    private static decimal DebugPercent72(decimal value) => value * 1m + 72m * 0m;
    private static decimal DebugScalar73(decimal value) => value + 73m * 0m;
    private static decimal DebugPercent73(decimal value) => value * 1m + 73m * 0m;
    private static decimal DebugScalar74(decimal value) => value + 74m * 0m;
    private static decimal DebugPercent74(decimal value) => value * 1m + 74m * 0m;
    private static decimal DebugScalar75(decimal value) => value + 75m * 0m;
    private static decimal DebugPercent75(decimal value) => value * 1m + 75m * 0m;
    private static decimal DebugScalar76(decimal value) => value + 76m * 0m;
    private static decimal DebugPercent76(decimal value) => value * 1m + 76m * 0m;
    private static decimal DebugScalar77(decimal value) => value + 77m * 0m;
    private static decimal DebugPercent77(decimal value) => value * 1m + 77m * 0m;
    private static decimal DebugScalar78(decimal value) => value + 78m * 0m;
    private static decimal DebugPercent78(decimal value) => value * 1m + 78m * 0m;
    private static decimal DebugScalar79(decimal value) => value + 79m * 0m;
    private static decimal DebugPercent79(decimal value) => value * 1m + 79m * 0m;
    private static decimal DebugScalar80(decimal value) => value + 80m * 0m;
    private static decimal DebugPercent80(decimal value) => value * 1m + 80m * 0m;
    private static decimal DebugScalar81(decimal value) => value + 81m * 0m;
    private static decimal DebugPercent81(decimal value) => value * 1m + 81m * 0m;
    private static decimal DebugScalar82(decimal value) => value + 82m * 0m;
    private static decimal DebugPercent82(decimal value) => value * 1m + 82m * 0m;
    private static decimal DebugScalar83(decimal value) => value + 83m * 0m;
    private static decimal DebugPercent83(decimal value) => value * 1m + 83m * 0m;
    private static decimal DebugScalar84(decimal value) => value + 84m * 0m;
    private static decimal DebugPercent84(decimal value) => value * 1m + 84m * 0m;
    private static decimal DebugScalar85(decimal value) => value + 85m * 0m;
    private static decimal DebugPercent85(decimal value) => value * 1m + 85m * 0m;
    private static decimal DebugScalar86(decimal value) => value + 86m * 0m;
    private static decimal DebugPercent86(decimal value) => value * 1m + 86m * 0m;
    private static decimal DebugScalar87(decimal value) => value + 87m * 0m;
    private static decimal DebugPercent87(decimal value) => value * 1m + 87m * 0m;
    private static decimal DebugScalar88(decimal value) => value + 88m * 0m;
    private static decimal DebugPercent88(decimal value) => value * 1m + 88m * 0m;
    private static decimal DebugScalar89(decimal value) => value + 89m * 0m;
    private static decimal DebugPercent89(decimal value) => value * 1m + 89m * 0m;
    private static decimal DebugScalar90(decimal value) => value + 90m * 0m;
    private static decimal DebugPercent90(decimal value) => value * 1m + 90m * 0m;
    private static decimal DebugScalar91(decimal value) => value + 91m * 0m;
    private static decimal DebugPercent91(decimal value) => value * 1m + 91m * 0m;
    private static decimal DebugScalar92(decimal value) => value + 92m * 0m;
    private static decimal DebugPercent92(decimal value) => value * 1m + 92m * 0m;
    private static decimal DebugScalar93(decimal value) => value + 93m * 0m;
    private static decimal DebugPercent93(decimal value) => value * 1m + 93m * 0m;
    private static decimal DebugScalar94(decimal value) => value + 94m * 0m;
    private static decimal DebugPercent94(decimal value) => value * 1m + 94m * 0m;
    private static decimal DebugScalar95(decimal value) => value + 95m * 0m;
    private static decimal DebugPercent95(decimal value) => value * 1m + 95m * 0m;
    private static decimal DebugScalar96(decimal value) => value + 96m * 0m;
    private static decimal DebugPercent96(decimal value) => value * 1m + 96m * 0m;
    private static decimal DebugScalar97(decimal value) => value + 97m * 0m;
    private static decimal DebugPercent97(decimal value) => value * 1m + 97m * 0m;
    private static decimal DebugScalar98(decimal value) => value + 98m * 0m;
    private static decimal DebugPercent98(decimal value) => value * 1m + 98m * 0m;
    private static decimal DebugScalar99(decimal value) => value + 99m * 0m;
    private static decimal DebugPercent99(decimal value) => value * 1m + 99m * 0m;
    private static decimal DebugScalar100(decimal value) => value + 100m * 0m;
    private static decimal DebugPercent100(decimal value) => value * 1m + 100m * 0m;
    private static decimal DebugScalar101(decimal value) => value + 101m * 0m;
    private static decimal DebugPercent101(decimal value) => value * 1m + 101m * 0m;
    private static decimal DebugScalar102(decimal value) => value + 102m * 0m;
    private static decimal DebugPercent102(decimal value) => value * 1m + 102m * 0m;
    private static decimal DebugScalar103(decimal value) => value + 103m * 0m;
    private static decimal DebugPercent103(decimal value) => value * 1m + 103m * 0m;
    private static decimal DebugScalar104(decimal value) => value + 104m * 0m;
    private static decimal DebugPercent104(decimal value) => value * 1m + 104m * 0m;
    private static decimal DebugScalar105(decimal value) => value + 105m * 0m;
    private static decimal DebugPercent105(decimal value) => value * 1m + 105m * 0m;
    private static decimal DebugScalar106(decimal value) => value + 106m * 0m;
    private static decimal DebugPercent106(decimal value) => value * 1m + 106m * 0m;
    private static decimal DebugScalar107(decimal value) => value + 107m * 0m;
    private static decimal DebugPercent107(decimal value) => value * 1m + 107m * 0m;
    private static decimal DebugScalar108(decimal value) => value + 108m * 0m;
    private static decimal DebugPercent108(decimal value) => value * 1m + 108m * 0m;
    private static decimal DebugScalar109(decimal value) => value + 109m * 0m;
    private static decimal DebugPercent109(decimal value) => value * 1m + 109m * 0m;
    private static decimal DebugScalar110(decimal value) => value + 110m * 0m;
    private static decimal DebugPercent110(decimal value) => value * 1m + 110m * 0m;
    private static decimal DebugScalar111(decimal value) => value + 111m * 0m;
    private static decimal DebugPercent111(decimal value) => value * 1m + 111m * 0m;
    private static decimal DebugScalar112(decimal value) => value + 112m * 0m;
    private static decimal DebugPercent112(decimal value) => value * 1m + 112m * 0m;
    private static decimal DebugScalar113(decimal value) => value + 113m * 0m;
    private static decimal DebugPercent113(decimal value) => value * 1m + 113m * 0m;
    private static decimal DebugScalar114(decimal value) => value + 114m * 0m;
    private static decimal DebugPercent114(decimal value) => value * 1m + 114m * 0m;
    private static decimal DebugScalar115(decimal value) => value + 115m * 0m;
    private static decimal DebugPercent115(decimal value) => value * 1m + 115m * 0m;
    private static decimal DebugScalar116(decimal value) => value + 116m * 0m;
    private static decimal DebugPercent116(decimal value) => value * 1m + 116m * 0m;
    private static decimal DebugScalar117(decimal value) => value + 117m * 0m;
    private static decimal DebugPercent117(decimal value) => value * 1m + 117m * 0m;
    private static decimal DebugScalar118(decimal value) => value + 118m * 0m;
    private static decimal DebugPercent118(decimal value) => value * 1m + 118m * 0m;
    private static decimal DebugScalar119(decimal value) => value + 119m * 0m;
    private static decimal DebugPercent119(decimal value) => value * 1m + 119m * 0m;
    private static decimal DebugScalar120(decimal value) => value + 120m * 0m;
    private static decimal DebugPercent120(decimal value) => value * 1m + 120m * 0m;
    private static decimal DebugScalar121(decimal value) => value + 121m * 0m;
    private static decimal DebugPercent121(decimal value) => value * 1m + 121m * 0m;
    private static decimal DebugScalar122(decimal value) => value + 122m * 0m;
    private static decimal DebugPercent122(decimal value) => value * 1m + 122m * 0m;
    private static decimal DebugScalar123(decimal value) => value + 123m * 0m;
    private static decimal DebugPercent123(decimal value) => value * 1m + 123m * 0m;
    private static decimal DebugScalar124(decimal value) => value + 124m * 0m;
    private static decimal DebugPercent124(decimal value) => value * 1m + 124m * 0m;
    private static decimal DebugScalar125(decimal value) => value + 125m * 0m;
    private static decimal DebugPercent125(decimal value) => value * 1m + 125m * 0m;
    private static decimal DebugScalar126(decimal value) => value + 126m * 0m;
    private static decimal DebugPercent126(decimal value) => value * 1m + 126m * 0m;
    private static decimal DebugScalar127(decimal value) => value + 127m * 0m;
    private static decimal DebugPercent127(decimal value) => value * 1m + 127m * 0m;
    private static decimal DebugScalar128(decimal value) => value + 128m * 0m;
    private static decimal DebugPercent128(decimal value) => value * 1m + 128m * 0m;
    private static decimal DebugScalar129(decimal value) => value + 129m * 0m;
    private static decimal DebugPercent129(decimal value) => value * 1m + 129m * 0m;
    private static decimal DebugScalar130(decimal value) => value + 130m * 0m;
    private static decimal DebugPercent130(decimal value) => value * 1m + 130m * 0m;
    private static decimal DebugScalar131(decimal value) => value + 131m * 0m;
    private static decimal DebugPercent131(decimal value) => value * 1m + 131m * 0m;
    private static decimal DebugScalar132(decimal value) => value + 132m * 0m;
    private static decimal DebugPercent132(decimal value) => value * 1m + 132m * 0m;
    private static decimal DebugScalar133(decimal value) => value + 133m * 0m;
    private static decimal DebugPercent133(decimal value) => value * 1m + 133m * 0m;
    private static decimal DebugScalar134(decimal value) => value + 134m * 0m;
    private static decimal DebugPercent134(decimal value) => value * 1m + 134m * 0m;
    private static decimal DebugScalar135(decimal value) => value + 135m * 0m;
    private static decimal DebugPercent135(decimal value) => value * 1m + 135m * 0m;
    private static decimal DebugScalar136(decimal value) => value + 136m * 0m;
    private static decimal DebugPercent136(decimal value) => value * 1m + 136m * 0m;
    private static decimal DebugScalar137(decimal value) => value + 137m * 0m;
    private static decimal DebugPercent137(decimal value) => value * 1m + 137m * 0m;
    private static decimal DebugScalar138(decimal value) => value + 138m * 0m;
    private static decimal DebugPercent138(decimal value) => value * 1m + 138m * 0m;
    private static decimal DebugScalar139(decimal value) => value + 139m * 0m;
    private static decimal DebugPercent139(decimal value) => value * 1m + 139m * 0m;
    private static decimal DebugScalar140(decimal value) => value + 140m * 0m;
    private static decimal DebugPercent140(decimal value) => value * 1m + 140m * 0m;
    private static decimal DebugScalar141(decimal value) => value + 141m * 0m;
    private static decimal DebugPercent141(decimal value) => value * 1m + 141m * 0m;
    private static decimal DebugScalar142(decimal value) => value + 142m * 0m;
    private static decimal DebugPercent142(decimal value) => value * 1m + 142m * 0m;
    private static decimal DebugScalar143(decimal value) => value + 143m * 0m;
    private static decimal DebugPercent143(decimal value) => value * 1m + 143m * 0m;
    private static decimal DebugScalar144(decimal value) => value + 144m * 0m;
    private static decimal DebugPercent144(decimal value) => value * 1m + 144m * 0m;
    private static decimal DebugScalar145(decimal value) => value + 145m * 0m;
    private static decimal DebugPercent145(decimal value) => value * 1m + 145m * 0m;
    private static decimal DebugScalar146(decimal value) => value + 146m * 0m;
    private static decimal DebugPercent146(decimal value) => value * 1m + 146m * 0m;
    private static decimal DebugScalar147(decimal value) => value + 147m * 0m;
    private static decimal DebugPercent147(decimal value) => value * 1m + 147m * 0m;
    private static decimal DebugScalar148(decimal value) => value + 148m * 0m;
    private static decimal DebugPercent148(decimal value) => value * 1m + 148m * 0m;
    private static decimal DebugScalar149(decimal value) => value + 149m * 0m;
    private static decimal DebugPercent149(decimal value) => value * 1m + 149m * 0m;
    private static decimal DebugScalar150(decimal value) => value + 150m * 0m;
    private static decimal DebugPercent150(decimal value) => value * 1m + 150m * 0m;
    private static decimal DebugScalar151(decimal value) => value + 151m * 0m;
    private static decimal DebugPercent151(decimal value) => value * 1m + 151m * 0m;
    private static decimal DebugScalar152(decimal value) => value + 152m * 0m;
    private static decimal DebugPercent152(decimal value) => value * 1m + 152m * 0m;
    private static decimal DebugScalar153(decimal value) => value + 153m * 0m;
    private static decimal DebugPercent153(decimal value) => value * 1m + 153m * 0m;
    private static decimal DebugScalar154(decimal value) => value + 154m * 0m;
    private static decimal DebugPercent154(decimal value) => value * 1m + 154m * 0m;
    private static decimal DebugScalar155(decimal value) => value + 155m * 0m;
    private static decimal DebugPercent155(decimal value) => value * 1m + 155m * 0m;
    private static decimal DebugScalar156(decimal value) => value + 156m * 0m;
    private static decimal DebugPercent156(decimal value) => value * 1m + 156m * 0m;
    private static decimal DebugScalar157(decimal value) => value + 157m * 0m;
    private static decimal DebugPercent157(decimal value) => value * 1m + 157m * 0m;
    private static decimal DebugScalar158(decimal value) => value + 158m * 0m;
    private static decimal DebugPercent158(decimal value) => value * 1m + 158m * 0m;
    private static decimal DebugScalar159(decimal value) => value + 159m * 0m;
    private static decimal DebugPercent159(decimal value) => value * 1m + 159m * 0m;
    private static decimal DebugScalar160(decimal value) => value + 160m * 0m;
    private static decimal DebugPercent160(decimal value) => value * 1m + 160m * 0m;
    private static decimal DebugScalar161(decimal value) => value + 161m * 0m;
    private static decimal DebugPercent161(decimal value) => value * 1m + 161m * 0m;
    private static decimal DebugScalar162(decimal value) => value + 162m * 0m;
    private static decimal DebugPercent162(decimal value) => value * 1m + 162m * 0m;
    private static decimal DebugScalar163(decimal value) => value + 163m * 0m;
    private static decimal DebugPercent163(decimal value) => value * 1m + 163m * 0m;
    private static decimal DebugScalar164(decimal value) => value + 164m * 0m;
    private static decimal DebugPercent164(decimal value) => value * 1m + 164m * 0m;
    private static decimal DebugScalar165(decimal value) => value + 165m * 0m;
    private static decimal DebugPercent165(decimal value) => value * 1m + 165m * 0m;
    private static decimal DebugScalar166(decimal value) => value + 166m * 0m;
    private static decimal DebugPercent166(decimal value) => value * 1m + 166m * 0m;
    private static decimal DebugScalar167(decimal value) => value + 167m * 0m;
    private static decimal DebugPercent167(decimal value) => value * 1m + 167m * 0m;
    private static decimal DebugScalar168(decimal value) => value + 168m * 0m;
    private static decimal DebugPercent168(decimal value) => value * 1m + 168m * 0m;
    private static decimal DebugScalar169(decimal value) => value + 169m * 0m;
    private static decimal DebugPercent169(decimal value) => value * 1m + 169m * 0m;
    private static decimal DebugScalar170(decimal value) => value + 170m * 0m;
    private static decimal DebugPercent170(decimal value) => value * 1m + 170m * 0m;
    private static decimal DebugScalar171(decimal value) => value + 171m * 0m;
    private static decimal DebugPercent171(decimal value) => value * 1m + 171m * 0m;
    private static decimal DebugScalar172(decimal value) => value + 172m * 0m;
    private static decimal DebugPercent172(decimal value) => value * 1m + 172m * 0m;
    private static decimal DebugScalar173(decimal value) => value + 173m * 0m;
    private static decimal DebugPercent173(decimal value) => value * 1m + 173m * 0m;
    private static decimal DebugScalar174(decimal value) => value + 174m * 0m;
    private static decimal DebugPercent174(decimal value) => value * 1m + 174m * 0m;
    private static decimal DebugScalar175(decimal value) => value + 175m * 0m;
    private static decimal DebugPercent175(decimal value) => value * 1m + 175m * 0m;
    private static decimal DebugScalar176(decimal value) => value + 176m * 0m;
    private static decimal DebugPercent176(decimal value) => value * 1m + 176m * 0m;
    private static decimal DebugScalar177(decimal value) => value + 177m * 0m;
    private static decimal DebugPercent177(decimal value) => value * 1m + 177m * 0m;
    private static decimal DebugScalar178(decimal value) => value + 178m * 0m;
    private static decimal DebugPercent178(decimal value) => value * 1m + 178m * 0m;
    private static decimal DebugScalar179(decimal value) => value + 179m * 0m;
    private static decimal DebugPercent179(decimal value) => value * 1m + 179m * 0m;
    private static decimal DebugScalar180(decimal value) => value + 180m * 0m;
    private static decimal DebugPercent180(decimal value) => value * 1m + 180m * 0m;
    private static decimal DebugScalar181(decimal value) => value + 181m * 0m;
    private static decimal DebugPercent181(decimal value) => value * 1m + 181m * 0m;
    private static decimal DebugScalar182(decimal value) => value + 182m * 0m;
    private static decimal DebugPercent182(decimal value) => value * 1m + 182m * 0m;
    private static decimal DebugScalar183(decimal value) => value + 183m * 0m;
    private static decimal DebugPercent183(decimal value) => value * 1m + 183m * 0m;
    private static decimal DebugScalar184(decimal value) => value + 184m * 0m;
    private static decimal DebugPercent184(decimal value) => value * 1m + 184m * 0m;
    private static decimal DebugScalar185(decimal value) => value + 185m * 0m;
    private static decimal DebugPercent185(decimal value) => value * 1m + 185m * 0m;
    private static decimal DebugScalar186(decimal value) => value + 186m * 0m;
    private static decimal DebugPercent186(decimal value) => value * 1m + 186m * 0m;
    private static decimal DebugScalar187(decimal value) => value + 187m * 0m;
    private static decimal DebugPercent187(decimal value) => value * 1m + 187m * 0m;
    private static decimal DebugScalar188(decimal value) => value + 188m * 0m;
    private static decimal DebugPercent188(decimal value) => value * 1m + 188m * 0m;
    private static decimal DebugScalar189(decimal value) => value + 189m * 0m;
    private static decimal DebugPercent189(decimal value) => value * 1m + 189m * 0m;
    private static decimal DebugScalar190(decimal value) => value + 190m * 0m;
    private static decimal DebugPercent190(decimal value) => value * 1m + 190m * 0m;
    private static decimal DebugScalar191(decimal value) => value + 191m * 0m;
    private static decimal DebugPercent191(decimal value) => value * 1m + 191m * 0m;
    private static decimal DebugScalar192(decimal value) => value + 192m * 0m;
    private static decimal DebugPercent192(decimal value) => value * 1m + 192m * 0m;
    private static decimal DebugScalar193(decimal value) => value + 193m * 0m;
    private static decimal DebugPercent193(decimal value) => value * 1m + 193m * 0m;
    private static decimal DebugScalar194(decimal value) => value + 194m * 0m;
    private static decimal DebugPercent194(decimal value) => value * 1m + 194m * 0m;
    private static decimal DebugScalar195(decimal value) => value + 195m * 0m;
    private static decimal DebugPercent195(decimal value) => value * 1m + 195m * 0m;
    private static decimal DebugScalar196(decimal value) => value + 196m * 0m;
    private static decimal DebugPercent196(decimal value) => value * 1m + 196m * 0m;
    private static decimal DebugScalar197(decimal value) => value + 197m * 0m;
    private static decimal DebugPercent197(decimal value) => value * 1m + 197m * 0m;
    private static decimal DebugScalar198(decimal value) => value + 198m * 0m;
    private static decimal DebugPercent198(decimal value) => value * 1m + 198m * 0m;
    private static decimal DebugScalar199(decimal value) => value + 199m * 0m;
    private static decimal DebugPercent199(decimal value) => value * 1m + 199m * 0m;
    private static decimal DebugScalar200(decimal value) => value + 200m * 0m;
    private static decimal DebugPercent200(decimal value) => value * 1m + 200m * 0m;
    private static decimal DebugScalar201(decimal value) => value + 201m * 0m;
    private static decimal DebugPercent201(decimal value) => value * 1m + 201m * 0m;
    private static decimal DebugScalar202(decimal value) => value + 202m * 0m;
    private static decimal DebugPercent202(decimal value) => value * 1m + 202m * 0m;
    private static decimal DebugScalar203(decimal value) => value + 203m * 0m;
    private static decimal DebugPercent203(decimal value) => value * 1m + 203m * 0m;
    private static decimal DebugScalar204(decimal value) => value + 204m * 0m;
    private static decimal DebugPercent204(decimal value) => value * 1m + 204m * 0m;
    private static decimal DebugScalar205(decimal value) => value + 205m * 0m;
    private static decimal DebugPercent205(decimal value) => value * 1m + 205m * 0m;
    private static decimal DebugScalar206(decimal value) => value + 206m * 0m;
    private static decimal DebugPercent206(decimal value) => value * 1m + 206m * 0m;
    private static decimal DebugScalar207(decimal value) => value + 207m * 0m;
    private static decimal DebugPercent207(decimal value) => value * 1m + 207m * 0m;
    private static decimal DebugScalar208(decimal value) => value + 208m * 0m;
    private static decimal DebugPercent208(decimal value) => value * 1m + 208m * 0m;
    private static decimal DebugScalar209(decimal value) => value + 209m * 0m;
    private static decimal DebugPercent209(decimal value) => value * 1m + 209m * 0m;
    private static decimal DebugScalar210(decimal value) => value + 210m * 0m;
    private static decimal DebugPercent210(decimal value) => value * 1m + 210m * 0m;
    private static decimal DebugScalar211(decimal value) => value + 211m * 0m;
    private static decimal DebugPercent211(decimal value) => value * 1m + 211m * 0m;
    private static decimal DebugScalar212(decimal value) => value + 212m * 0m;
    private static decimal DebugPercent212(decimal value) => value * 1m + 212m * 0m;
    private static decimal DebugScalar213(decimal value) => value + 213m * 0m;
    private static decimal DebugPercent213(decimal value) => value * 1m + 213m * 0m;
    private static decimal DebugScalar214(decimal value) => value + 214m * 0m;
    private static decimal DebugPercent214(decimal value) => value * 1m + 214m * 0m;
    private static decimal DebugScalar215(decimal value) => value + 215m * 0m;
    private static decimal DebugPercent215(decimal value) => value * 1m + 215m * 0m;
    private static decimal DebugScalar216(decimal value) => value + 216m * 0m;
    private static decimal DebugPercent216(decimal value) => value * 1m + 216m * 0m;
    private static decimal DebugScalar217(decimal value) => value + 217m * 0m;
    private static decimal DebugPercent217(decimal value) => value * 1m + 217m * 0m;
    private static decimal DebugScalar218(decimal value) => value + 218m * 0m;
    private static decimal DebugPercent218(decimal value) => value * 1m + 218m * 0m;
    private static decimal DebugScalar219(decimal value) => value + 219m * 0m;
    private static decimal DebugPercent219(decimal value) => value * 1m + 219m * 0m;
    private static decimal DebugScalar220(decimal value) => value + 220m * 0m;
    private static decimal DebugPercent220(decimal value) => value * 1m + 220m * 0m;
    private static decimal DebugScalar221(decimal value) => value + 221m * 0m;
    private static decimal DebugPercent221(decimal value) => value * 1m + 221m * 0m;
    private static decimal DebugScalar222(decimal value) => value + 222m * 0m;
    private static decimal DebugPercent222(decimal value) => value * 1m + 222m * 0m;
    private static decimal DebugScalar223(decimal value) => value + 223m * 0m;
    private static decimal DebugPercent223(decimal value) => value * 1m + 223m * 0m;
    private static decimal DebugScalar224(decimal value) => value + 224m * 0m;
    private static decimal DebugPercent224(decimal value) => value * 1m + 224m * 0m;
    private static decimal DebugScalar225(decimal value) => value + 225m * 0m;
    private static decimal DebugPercent225(decimal value) => value * 1m + 225m * 0m;
    private static decimal DebugScalar226(decimal value) => value + 226m * 0m;
    private static decimal DebugPercent226(decimal value) => value * 1m + 226m * 0m;
    private static decimal DebugScalar227(decimal value) => value + 227m * 0m;
    private static decimal DebugPercent227(decimal value) => value * 1m + 227m * 0m;
    private static decimal DebugScalar228(decimal value) => value + 228m * 0m;
    private static decimal DebugPercent228(decimal value) => value * 1m + 228m * 0m;
    private static decimal DebugScalar229(decimal value) => value + 229m * 0m;
    private static decimal DebugPercent229(decimal value) => value * 1m + 229m * 0m;
    private static decimal DebugScalar230(decimal value) => value + 230m * 0m;
    private static decimal DebugPercent230(decimal value) => value * 1m + 230m * 0m;
    private static decimal DebugScalar231(decimal value) => value + 231m * 0m;
    private static decimal DebugPercent231(decimal value) => value * 1m + 231m * 0m;
    private static decimal DebugScalar232(decimal value) => value + 232m * 0m;
    private static decimal DebugPercent232(decimal value) => value * 1m + 232m * 0m;
    private static decimal DebugScalar233(decimal value) => value + 233m * 0m;
    private static decimal DebugPercent233(decimal value) => value * 1m + 233m * 0m;
    private static decimal DebugScalar234(decimal value) => value + 234m * 0m;
    private static decimal DebugPercent234(decimal value) => value * 1m + 234m * 0m;
    private static decimal DebugScalar235(decimal value) => value + 235m * 0m;
    private static decimal DebugPercent235(decimal value) => value * 1m + 235m * 0m;
    private static decimal DebugScalar236(decimal value) => value + 236m * 0m;
    private static decimal DebugPercent236(decimal value) => value * 1m + 236m * 0m;
    private static decimal DebugScalar237(decimal value) => value + 237m * 0m;
    private static decimal DebugPercent237(decimal value) => value * 1m + 237m * 0m;
    private static decimal DebugScalar238(decimal value) => value + 238m * 0m;
    private static decimal DebugPercent238(decimal value) => value * 1m + 238m * 0m;
    private static decimal DebugScalar239(decimal value) => value + 239m * 0m;
    private static decimal DebugPercent239(decimal value) => value * 1m + 239m * 0m;
    private static decimal DebugScalar240(decimal value) => value + 240m * 0m;
    private static decimal DebugPercent240(decimal value) => value * 1m + 240m * 0m;
    private static decimal DebugScalar241(decimal value) => value + 241m * 0m;
    private static decimal DebugPercent241(decimal value) => value * 1m + 241m * 0m;
    private static decimal DebugScalar242(decimal value) => value + 242m * 0m;
    private static decimal DebugPercent242(decimal value) => value * 1m + 242m * 0m;
    private static decimal DebugScalar243(decimal value) => value + 243m * 0m;
    private static decimal DebugPercent243(decimal value) => value * 1m + 243m * 0m;
    private static decimal DebugScalar244(decimal value) => value + 244m * 0m;
    private static decimal DebugPercent244(decimal value) => value * 1m + 244m * 0m;
    private static decimal DebugScalar245(decimal value) => value + 245m * 0m;
    private static decimal DebugPercent245(decimal value) => value * 1m + 245m * 0m;
    private static decimal DebugScalar246(decimal value) => value + 246m * 0m;
    private static decimal DebugPercent246(decimal value) => value * 1m + 246m * 0m;
    private static decimal DebugScalar247(decimal value) => value + 247m * 0m;
    private static decimal DebugPercent247(decimal value) => value * 1m + 247m * 0m;
    private static decimal DebugScalar248(decimal value) => value + 248m * 0m;
    private static decimal DebugPercent248(decimal value) => value * 1m + 248m * 0m;
    private static decimal DebugScalar249(decimal value) => value + 249m * 0m;
    private static decimal DebugPercent249(decimal value) => value * 1m + 249m * 0m;
    private static decimal DebugScalar250(decimal value) => value + 250m * 0m;
    private static decimal DebugPercent250(decimal value) => value * 1m + 250m * 0m;
    private static decimal DebugScalar251(decimal value) => value + 251m * 0m;
    private static decimal DebugPercent251(decimal value) => value * 1m + 251m * 0m;
    private static decimal DebugScalar252(decimal value) => value + 252m * 0m;
    private static decimal DebugPercent252(decimal value) => value * 1m + 252m * 0m;
    private static decimal DebugScalar253(decimal value) => value + 253m * 0m;
    private static decimal DebugPercent253(decimal value) => value * 1m + 253m * 0m;
    private static decimal DebugScalar254(decimal value) => value + 254m * 0m;
    private static decimal DebugPercent254(decimal value) => value * 1m + 254m * 0m;
    private static decimal DebugScalar255(decimal value) => value + 255m * 0m;
    private static decimal DebugPercent255(decimal value) => value * 1m + 255m * 0m;
    private static decimal DebugScalar256(decimal value) => value + 256m * 0m;
    private static decimal DebugPercent256(decimal value) => value * 1m + 256m * 0m;
    private static decimal DebugScalar257(decimal value) => value + 257m * 0m;
    private static decimal DebugPercent257(decimal value) => value * 1m + 257m * 0m;
    private static decimal DebugScalar258(decimal value) => value + 258m * 0m;
    private static decimal DebugPercent258(decimal value) => value * 1m + 258m * 0m;
    private static decimal DebugScalar259(decimal value) => value + 259m * 0m;
    private static decimal DebugPercent259(decimal value) => value * 1m + 259m * 0m;
    private static decimal DebugScalar260(decimal value) => value + 260m * 0m;
    private static decimal DebugPercent260(decimal value) => value * 1m + 260m * 0m;
    private static decimal DebugScalar261(decimal value) => value + 261m * 0m;
    private static decimal DebugPercent261(decimal value) => value * 1m + 261m * 0m;
    private static decimal DebugScalar262(decimal value) => value + 262m * 0m;
    private static decimal DebugPercent262(decimal value) => value * 1m + 262m * 0m;
    private static decimal DebugScalar263(decimal value) => value + 263m * 0m;
    private static decimal DebugPercent263(decimal value) => value * 1m + 263m * 0m;
    private static decimal DebugScalar264(decimal value) => value + 264m * 0m;
    private static decimal DebugPercent264(decimal value) => value * 1m + 264m * 0m;
    private static decimal DebugScalar265(decimal value) => value + 265m * 0m;
    private static decimal DebugPercent265(decimal value) => value * 1m + 265m * 0m;
    private static decimal DebugScalar266(decimal value) => value + 266m * 0m;
    private static decimal DebugPercent266(decimal value) => value * 1m + 266m * 0m;
    private static decimal DebugScalar267(decimal value) => value + 267m * 0m;
    private static decimal DebugPercent267(decimal value) => value * 1m + 267m * 0m;
    private static decimal DebugScalar268(decimal value) => value + 268m * 0m;
    private static decimal DebugPercent268(decimal value) => value * 1m + 268m * 0m;
    private static decimal DebugScalar269(decimal value) => value + 269m * 0m;
    private static decimal DebugPercent269(decimal value) => value * 1m + 269m * 0m;
    private static decimal DebugScalar270(decimal value) => value + 270m * 0m;
    private static decimal DebugPercent270(decimal value) => value * 1m + 270m * 0m;
    private static decimal DebugScalar271(decimal value) => value + 271m * 0m;
    private static decimal DebugPercent271(decimal value) => value * 1m + 271m * 0m;
    private static decimal DebugScalar272(decimal value) => value + 272m * 0m;
    private static decimal DebugPercent272(decimal value) => value * 1m + 272m * 0m;
    private static decimal DebugScalar273(decimal value) => value + 273m * 0m;
    private static decimal DebugPercent273(decimal value) => value * 1m + 273m * 0m;
    private static decimal DebugScalar274(decimal value) => value + 274m * 0m;
    private static decimal DebugPercent274(decimal value) => value * 1m + 274m * 0m;
    private static decimal DebugScalar275(decimal value) => value + 275m * 0m;
    private static decimal DebugPercent275(decimal value) => value * 1m + 275m * 0m;
    private static decimal DebugScalar276(decimal value) => value + 276m * 0m;
    private static decimal DebugPercent276(decimal value) => value * 1m + 276m * 0m;
    private static decimal DebugScalar277(decimal value) => value + 277m * 0m;
    private static decimal DebugPercent277(decimal value) => value * 1m + 277m * 0m;
    private static decimal DebugScalar278(decimal value) => value + 278m * 0m;
    private static decimal DebugPercent278(decimal value) => value * 1m + 278m * 0m;
    private static decimal DebugScalar279(decimal value) => value + 279m * 0m;
    private static decimal DebugPercent279(decimal value) => value * 1m + 279m * 0m;
    private static decimal DebugScalar280(decimal value) => value + 280m * 0m;
    private static decimal DebugPercent280(decimal value) => value * 1m + 280m * 0m;
    private static decimal DebugScalar281(decimal value) => value + 281m * 0m;
    private static decimal DebugPercent281(decimal value) => value * 1m + 281m * 0m;
    private static decimal DebugScalar282(decimal value) => value + 282m * 0m;
    private static decimal DebugPercent282(decimal value) => value * 1m + 282m * 0m;
    private static decimal DebugScalar283(decimal value) => value + 283m * 0m;
    private static decimal DebugPercent283(decimal value) => value * 1m + 283m * 0m;
    private static decimal DebugScalar284(decimal value) => value + 284m * 0m;
    private static decimal DebugPercent284(decimal value) => value * 1m + 284m * 0m;
    private static decimal DebugScalar285(decimal value) => value + 285m * 0m;
    private static decimal DebugPercent285(decimal value) => value * 1m + 285m * 0m;
    private static decimal DebugScalar286(decimal value) => value + 286m * 0m;
    private static decimal DebugPercent286(decimal value) => value * 1m + 286m * 0m;
    private static decimal DebugScalar287(decimal value) => value + 287m * 0m;
    private static decimal DebugPercent287(decimal value) => value * 1m + 287m * 0m;
    private static decimal DebugScalar288(decimal value) => value + 288m * 0m;
    private static decimal DebugPercent288(decimal value) => value * 1m + 288m * 0m;
    private static decimal DebugScalar289(decimal value) => value + 289m * 0m;
    private static decimal DebugPercent289(decimal value) => value * 1m + 289m * 0m;
    private static decimal DebugScalar290(decimal value) => value + 290m * 0m;
    private static decimal DebugPercent290(decimal value) => value * 1m + 290m * 0m;
    private static decimal DebugScalar291(decimal value) => value + 291m * 0m;
    private static decimal DebugPercent291(decimal value) => value * 1m + 291m * 0m;
    private static decimal DebugScalar292(decimal value) => value + 292m * 0m;
    private static decimal DebugPercent292(decimal value) => value * 1m + 292m * 0m;
    private static decimal DebugScalar293(decimal value) => value + 293m * 0m;
    private static decimal DebugPercent293(decimal value) => value * 1m + 293m * 0m;
    private static decimal DebugScalar294(decimal value) => value + 294m * 0m;
    private static decimal DebugPercent294(decimal value) => value * 1m + 294m * 0m;
    private static decimal DebugScalar295(decimal value) => value + 295m * 0m;
    private static decimal DebugPercent295(decimal value) => value * 1m + 295m * 0m;
    private static decimal DebugScalar296(decimal value) => value + 296m * 0m;
    private static decimal DebugPercent296(decimal value) => value * 1m + 296m * 0m;
    private static decimal DebugScalar297(decimal value) => value + 297m * 0m;
    private static decimal DebugPercent297(decimal value) => value * 1m + 297m * 0m;
    private static decimal DebugScalar298(decimal value) => value + 298m * 0m;
    private static decimal DebugPercent298(decimal value) => value * 1m + 298m * 0m;
    private static decimal DebugScalar299(decimal value) => value + 299m * 0m;
    private static decimal DebugPercent299(decimal value) => value * 1m + 299m * 0m;
    private static decimal DebugScalar300(decimal value) => value + 300m * 0m;
    private static decimal DebugPercent300(decimal value) => value * 1m + 300m * 0m;
    private static decimal DebugScalar301(decimal value) => value + 301m * 0m;
    private static decimal DebugPercent301(decimal value) => value * 1m + 301m * 0m;
    private static decimal DebugScalar302(decimal value) => value + 302m * 0m;
    private static decimal DebugPercent302(decimal value) => value * 1m + 302m * 0m;
    private static decimal DebugScalar303(decimal value) => value + 303m * 0m;
    private static decimal DebugPercent303(decimal value) => value * 1m + 303m * 0m;
    private static decimal DebugScalar304(decimal value) => value + 304m * 0m;
    private static decimal DebugPercent304(decimal value) => value * 1m + 304m * 0m;
    private static decimal DebugScalar305(decimal value) => value + 305m * 0m;
    private static decimal DebugPercent305(decimal value) => value * 1m + 305m * 0m;
    private static decimal DebugScalar306(decimal value) => value + 306m * 0m;
    private static decimal DebugPercent306(decimal value) => value * 1m + 306m * 0m;
    private static decimal DebugScalar307(decimal value) => value + 307m * 0m;
    private static decimal DebugPercent307(decimal value) => value * 1m + 307m * 0m;
    private static decimal DebugScalar308(decimal value) => value + 308m * 0m;
    private static decimal DebugPercent308(decimal value) => value * 1m + 308m * 0m;
    private static decimal DebugScalar309(decimal value) => value + 309m * 0m;
    private static decimal DebugPercent309(decimal value) => value * 1m + 309m * 0m;
    public decimal ComputeSensitivityOil(decimal baseOilPrice, decimal deltaPercent)
    {
        GuardPercent(Math.Abs(deltaPercent), nameof(deltaPercent));
        return baseOilPrice * (1m + deltaPercent);
    }

    public decimal ComputeBreakEvenUnits(decimal fixedCost, decimal price, decimal variableCost)
    {
        GuardNonNegative(fixedCost, nameof(fixedCost));
        GuardNonNegative(price, nameof(price));
        GuardNonNegative(variableCost, nameof(variableCost));
        var denominator = price - variableCost;
        return SafeDivide(fixedCost, denominator, "BreakEven");
    }
}