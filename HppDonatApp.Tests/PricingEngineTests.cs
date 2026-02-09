using System;
using System.Collections.Generic;
using HppDonatApp.Core;
using HppDonatApp.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HppDonatApp.Tests;

public sealed class PricingEngineTests
{
    [Fact]
    public void Calculates_Deterministic_Scenario()
    {
        var engine = new PricingEngine(new NullLogger<PricingEngine>());
        var items = new List<RecipeItem>
        {
            new(Guid.NewGuid(), 10m, "kg", 12000m),
            new(Guid.NewGuid(), 5m, "kg", 10000m),
            new(Guid.NewGuid(), 0.5m, "kg", 40000m),
            new(Guid.NewGuid(), 2m, "L", 15000m),
            new(Guid.NewGuid(), 60m, "pcs", 1500m),
        };
        var labor = new List<LaborRole> { new("Baker", 3m, 30000m) };
        var request = new BatchRequest(items, 1m, 5m, 30000m, 0m, 0, 0m, 0m, labor, 66666.67m, 100, 0.05m, 500m, 0.5m, 0.11m, "Nearest100");
        var result = engine.CalculateBatchCost(request);
        Assert.Equal(330000m, result.IngredientCost, 2);
        Assert.Equal(684166.67m, result.TotalBatchCost, 2);
        Assert.Equal(7201.75m, result.UnitCost, 2);
    }

    [Fact]
    public void Scales_With_Batch_Multiplier()
    {
        var engine = new PricingEngine(new NullLogger<PricingEngine>());
        var items = new List<RecipeItem>
        {
            new(Guid.NewGuid(), 10m, "kg", 12000m),
        };
        var request = new BatchRequest(items, 2m, 0m, 0m, 0m, 0, 0m, 0m, Array.Empty<LaborRole>(), 0m, 10, 0m, 0m, 0m, 0m, "Nearest100");
        var result = engine.CalculateBatchCost(request);
        Assert.Equal(240000m, result.IngredientCost, 2);
    }
}
