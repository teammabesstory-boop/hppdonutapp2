using HppDonatApp.Services;
using HppDonatApp.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HppDonatApp.Tests;

public sealed class ViewModelTests
{
    [Fact]
    public void Can_Calculate_From_ViewModel()
    {
        var engine = new PricingEngine(new NullLogger<PricingEngine>());
        var vm = new RecipeEditorViewModel(engine);
        vm.CalculateCommand.Execute(null);
        Assert.NotNull(vm.LastResult);
        Assert.True(vm.LastResult!.UnitCost > 0m);
    }
}
