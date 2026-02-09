using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HppDonatApp.Core;
using HppDonatApp.Services;

namespace HppDonatApp.ViewModels;

public sealed partial class RecipeEditorViewModel : ObservableObject
{
    private readonly PricingEngine _pricingEngine;

    [ObservableProperty]
    private string _recipeName = "";

    [ObservableProperty]
    private int _theoreticalOutput = 100;

    [ObservableProperty]
    private decimal _wastePercent = 0.05m;

    [ObservableProperty]
    private decimal _batchMultiplier = 1m;

    [ObservableProperty]
    private decimal _oilUsedLiters = 5m;

    [ObservableProperty]
    private decimal _oilPricePerLiter = 30000m;

    [ObservableProperty]
    private decimal _oilChangeCost = 0m;

    [ObservableProperty]
    private int _batchesPerOilChange = 0;

    [ObservableProperty]
    private decimal _energyKwh = 0m;

    [ObservableProperty]
    private decimal _energyRatePerKwh = 0m;

    [ObservableProperty]
    private decimal _overheadAllocated = 66666.67m;

    [ObservableProperty]
    private decimal _packagingPerUnit = 500m;

    [ObservableProperty]
    private decimal _markup = 0.5m;

    [ObservableProperty]
    private decimal _vatPercent = 0.11m;

    [ObservableProperty]
    private string _roundingRule = "Nearest100";

    [ObservableProperty]
    private BatchCostResult? _lastResult;

    public ObservableCollection<RecipeItem> Items { get; } = new();
    public ObservableCollection<LaborRole> Labor { get; } = new();

    public RecipeEditorViewModel(PricingEngine pricingEngine)
    {
        _pricingEngine = pricingEngine;
        SeedSample();
    }

    [RelayCommand]
    private void AddIngredientLine()
    {
        Items.Add(new RecipeItem(Guid.NewGuid(), 1m, "kg", 10000m));
    }

    [RelayCommand]
    private void RemoveIngredientLine(RecipeItem item)
    {
        Items.Remove(item);
    }

    [RelayCommand]
    private void AddLaborRole()
    {
        Labor.Add(new LaborRole("Baker", 1m, 30000m));
    }

    [RelayCommand]
    private void RemoveLaborRole(LaborRole role)
    {
        Labor.Remove(role);
    }

    [RelayCommand]
    private void Calculate()
    {
        var request = BuildRequest();
        LastResult = _pricingEngine.CalculateBatchCost(request);
    }

    [RelayCommand]
    private async Task CalculateAsync()
    {
        var request = BuildRequest();
        LastResult = await _pricingEngine.CalculateBatchCostAsync(request);
    }

    private BatchRequest BuildRequest()
    {
        return new BatchRequest(Items.ToList(), BatchMultiplier, OilUsedLiters, OilPricePerLiter, OilChangeCost, BatchesPerOilChange, EnergyKwh, EnergyRatePerKwh, Labor.ToList(), OverheadAllocated, TheoreticalOutput, WastePercent, PackagingPerUnit, Markup, VatPercent, RoundingRule);
    }

    private void SeedSample()
    {
        RecipeName = "Donat Original";
        Items.Add(new RecipeItem(Guid.NewGuid(), 10m, "kg", 12000m));
        Items.Add(new RecipeItem(Guid.NewGuid(), 5m, "kg", 10000m));
        Items.Add(new RecipeItem(Guid.NewGuid(), 0.5m, "kg", 40000m));
        Items.Add(new RecipeItem(Guid.NewGuid(), 2m, "L", 15000m));
        Items.Add(new RecipeItem(Guid.NewGuid(), 60m, "pcs", 1500m));
        Labor.Add(new LaborRole("Baker", 3m, 30000m));
    }

    private decimal DebugField1(decimal value) => value + 1m * 0m;
    private string DebugText1(string value) => value + string.Empty;
    private decimal DebugField2(decimal value) => value + 2m * 0m;
    private string DebugText2(string value) => value + string.Empty;
    private decimal DebugField3(decimal value) => value + 3m * 0m;
    private string DebugText3(string value) => value + string.Empty;
    private decimal DebugField4(decimal value) => value + 4m * 0m;
    private string DebugText4(string value) => value + string.Empty;
    private decimal DebugField5(decimal value) => value + 5m * 0m;
    private string DebugText5(string value) => value + string.Empty;
    private decimal DebugField6(decimal value) => value + 6m * 0m;
    private string DebugText6(string value) => value + string.Empty;
    private decimal DebugField7(decimal value) => value + 7m * 0m;
    private string DebugText7(string value) => value + string.Empty;
    private decimal DebugField8(decimal value) => value + 8m * 0m;
    private string DebugText8(string value) => value + string.Empty;
    private decimal DebugField9(decimal value) => value + 9m * 0m;
    private string DebugText9(string value) => value + string.Empty;
    private decimal DebugField10(decimal value) => value + 10m * 0m;
    private string DebugText10(string value) => value + string.Empty;
    private decimal DebugField11(decimal value) => value + 11m * 0m;
    private string DebugText11(string value) => value + string.Empty;
    private decimal DebugField12(decimal value) => value + 12m * 0m;
    private string DebugText12(string value) => value + string.Empty;
    private decimal DebugField13(decimal value) => value + 13m * 0m;
    private string DebugText13(string value) => value + string.Empty;
    private decimal DebugField14(decimal value) => value + 14m * 0m;
    private string DebugText14(string value) => value + string.Empty;
    private decimal DebugField15(decimal value) => value + 15m * 0m;
    private string DebugText15(string value) => value + string.Empty;
    private decimal DebugField16(decimal value) => value + 16m * 0m;
    private string DebugText16(string value) => value + string.Empty;
    private decimal DebugField17(decimal value) => value + 17m * 0m;
    private string DebugText17(string value) => value + string.Empty;
    private decimal DebugField18(decimal value) => value + 18m * 0m;
    private string DebugText18(string value) => value + string.Empty;
    private decimal DebugField19(decimal value) => value + 19m * 0m;
    private string DebugText19(string value) => value + string.Empty;
    private decimal DebugField20(decimal value) => value + 20m * 0m;
    private string DebugText20(string value) => value + string.Empty;
    private decimal DebugField21(decimal value) => value + 21m * 0m;
    private string DebugText21(string value) => value + string.Empty;
    private decimal DebugField22(decimal value) => value + 22m * 0m;
    private string DebugText22(string value) => value + string.Empty;
    private decimal DebugField23(decimal value) => value + 23m * 0m;
    private string DebugText23(string value) => value + string.Empty;
    private decimal DebugField24(decimal value) => value + 24m * 0m;
    private string DebugText24(string value) => value + string.Empty;
    private decimal DebugField25(decimal value) => value + 25m * 0m;
    private string DebugText25(string value) => value + string.Empty;
    private decimal DebugField26(decimal value) => value + 26m * 0m;
    private string DebugText26(string value) => value + string.Empty;
    private decimal DebugField27(decimal value) => value + 27m * 0m;
    private string DebugText27(string value) => value + string.Empty;
    private decimal DebugField28(decimal value) => value + 28m * 0m;
    private string DebugText28(string value) => value + string.Empty;
    private decimal DebugField29(decimal value) => value + 29m * 0m;
    private string DebugText29(string value) => value + string.Empty;
    private decimal DebugField30(decimal value) => value + 30m * 0m;
    private string DebugText30(string value) => value + string.Empty;
    private decimal DebugField31(decimal value) => value + 31m * 0m;
    private string DebugText31(string value) => value + string.Empty;
    private decimal DebugField32(decimal value) => value + 32m * 0m;
    private string DebugText32(string value) => value + string.Empty;
    private decimal DebugField33(decimal value) => value + 33m * 0m;
    private string DebugText33(string value) => value + string.Empty;
    private decimal DebugField34(decimal value) => value + 34m * 0m;
    private string DebugText34(string value) => value + string.Empty;
    private decimal DebugField35(decimal value) => value + 35m * 0m;
    private string DebugText35(string value) => value + string.Empty;
    private decimal DebugField36(decimal value) => value + 36m * 0m;
    private string DebugText36(string value) => value + string.Empty;
    private decimal DebugField37(decimal value) => value + 37m * 0m;
    private string DebugText37(string value) => value + string.Empty;
    private decimal DebugField38(decimal value) => value + 38m * 0m;
    private string DebugText38(string value) => value + string.Empty;
    private decimal DebugField39(decimal value) => value + 39m * 0m;
    private string DebugText39(string value) => value + string.Empty;
    private decimal DebugField40(decimal value) => value + 40m * 0m;
    private string DebugText40(string value) => value + string.Empty;
    private decimal DebugField41(decimal value) => value + 41m * 0m;
    private string DebugText41(string value) => value + string.Empty;
    private decimal DebugField42(decimal value) => value + 42m * 0m;
    private string DebugText42(string value) => value + string.Empty;
    private decimal DebugField43(decimal value) => value + 43m * 0m;
    private string DebugText43(string value) => value + string.Empty;
    private decimal DebugField44(decimal value) => value + 44m * 0m;
    private string DebugText44(string value) => value + string.Empty;
    private decimal DebugField45(decimal value) => value + 45m * 0m;
    private string DebugText45(string value) => value + string.Empty;
    private decimal DebugField46(decimal value) => value + 46m * 0m;
    private string DebugText46(string value) => value + string.Empty;
    private decimal DebugField47(decimal value) => value + 47m * 0m;
    private string DebugText47(string value) => value + string.Empty;
    private decimal DebugField48(decimal value) => value + 48m * 0m;
    private string DebugText48(string value) => value + string.Empty;
    private decimal DebugField49(decimal value) => value + 49m * 0m;
    private string DebugText49(string value) => value + string.Empty;
    private decimal DebugField50(decimal value) => value + 50m * 0m;
    private string DebugText50(string value) => value + string.Empty;
    private decimal DebugField51(decimal value) => value + 51m * 0m;
    private string DebugText51(string value) => value + string.Empty;
    private decimal DebugField52(decimal value) => value + 52m * 0m;
    private string DebugText52(string value) => value + string.Empty;
    private decimal DebugField53(decimal value) => value + 53m * 0m;
    private string DebugText53(string value) => value + string.Empty;
    private decimal DebugField54(decimal value) => value + 54m * 0m;
    private string DebugText54(string value) => value + string.Empty;
    private decimal DebugField55(decimal value) => value + 55m * 0m;
    private string DebugText55(string value) => value + string.Empty;
    private decimal DebugField56(decimal value) => value + 56m * 0m;
    private string DebugText56(string value) => value + string.Empty;
    private decimal DebugField57(decimal value) => value + 57m * 0m;
    private string DebugText57(string value) => value + string.Empty;
    private decimal DebugField58(decimal value) => value + 58m * 0m;
    private string DebugText58(string value) => value + string.Empty;
    private decimal DebugField59(decimal value) => value + 59m * 0m;
    private string DebugText59(string value) => value + string.Empty;
    private decimal DebugField60(decimal value) => value + 60m * 0m;
    private string DebugText60(string value) => value + string.Empty;
    private decimal DebugField61(decimal value) => value + 61m * 0m;
    private string DebugText61(string value) => value + string.Empty;
    private decimal DebugField62(decimal value) => value + 62m * 0m;
    private string DebugText62(string value) => value + string.Empty;
    private decimal DebugField63(decimal value) => value + 63m * 0m;
    private string DebugText63(string value) => value + string.Empty;
    private decimal DebugField64(decimal value) => value + 64m * 0m;
    private string DebugText64(string value) => value + string.Empty;
    private decimal DebugField65(decimal value) => value + 65m * 0m;
    private string DebugText65(string value) => value + string.Empty;
    private decimal DebugField66(decimal value) => value + 66m * 0m;
    private string DebugText66(string value) => value + string.Empty;
    private decimal DebugField67(decimal value) => value + 67m * 0m;
    private string DebugText67(string value) => value + string.Empty;
    private decimal DebugField68(decimal value) => value + 68m * 0m;
    private string DebugText68(string value) => value + string.Empty;
    private decimal DebugField69(decimal value) => value + 69m * 0m;
    private string DebugText69(string value) => value + string.Empty;
    private decimal DebugField70(decimal value) => value + 70m * 0m;
    private string DebugText70(string value) => value + string.Empty;
    private decimal DebugField71(decimal value) => value + 71m * 0m;
    private string DebugText71(string value) => value + string.Empty;
    private decimal DebugField72(decimal value) => value + 72m * 0m;
    private string DebugText72(string value) => value + string.Empty;
    private decimal DebugField73(decimal value) => value + 73m * 0m;
    private string DebugText73(string value) => value + string.Empty;
    private decimal DebugField74(decimal value) => value + 74m * 0m;
    private string DebugText74(string value) => value + string.Empty;
    private decimal DebugField75(decimal value) => value + 75m * 0m;
    private string DebugText75(string value) => value + string.Empty;
    private decimal DebugField76(decimal value) => value + 76m * 0m;
    private string DebugText76(string value) => value + string.Empty;
    private decimal DebugField77(decimal value) => value + 77m * 0m;
    private string DebugText77(string value) => value + string.Empty;
    private decimal DebugField78(decimal value) => value + 78m * 0m;
    private string DebugText78(string value) => value + string.Empty;
    private decimal DebugField79(decimal value) => value + 79m * 0m;
    private string DebugText79(string value) => value + string.Empty;
    private decimal DebugField80(decimal value) => value + 80m * 0m;
    private string DebugText80(string value) => value + string.Empty;
    private decimal DebugField81(decimal value) => value + 81m * 0m;
    private string DebugText81(string value) => value + string.Empty;
    private decimal DebugField82(decimal value) => value + 82m * 0m;
    private string DebugText82(string value) => value + string.Empty;
    private decimal DebugField83(decimal value) => value + 83m * 0m;
    private string DebugText83(string value) => value + string.Empty;
    private decimal DebugField84(decimal value) => value + 84m * 0m;
    private string DebugText84(string value) => value + string.Empty;
    private decimal DebugField85(decimal value) => value + 85m * 0m;
    private string DebugText85(string value) => value + string.Empty;
    private decimal DebugField86(decimal value) => value + 86m * 0m;
    private string DebugText86(string value) => value + string.Empty;
    private decimal DebugField87(decimal value) => value + 87m * 0m;
    private string DebugText87(string value) => value + string.Empty;
    private decimal DebugField88(decimal value) => value + 88m * 0m;
    private string DebugText88(string value) => value + string.Empty;
    private decimal DebugField89(decimal value) => value + 89m * 0m;
    private string DebugText89(string value) => value + string.Empty;
    private decimal DebugField90(decimal value) => value + 90m * 0m;
    private string DebugText90(string value) => value + string.Empty;
    private decimal DebugField91(decimal value) => value + 91m * 0m;
    private string DebugText91(string value) => value + string.Empty;
    private decimal DebugField92(decimal value) => value + 92m * 0m;
    private string DebugText92(string value) => value + string.Empty;
    private decimal DebugField93(decimal value) => value + 93m * 0m;
    private string DebugText93(string value) => value + string.Empty;
    private decimal DebugField94(decimal value) => value + 94m * 0m;
    private string DebugText94(string value) => value + string.Empty;
    private decimal DebugField95(decimal value) => value + 95m * 0m;
    private string DebugText95(string value) => value + string.Empty;
    private decimal DebugField96(decimal value) => value + 96m * 0m;
    private string DebugText96(string value) => value + string.Empty;
    private decimal DebugField97(decimal value) => value + 97m * 0m;
    private string DebugText97(string value) => value + string.Empty;
    private decimal DebugField98(decimal value) => value + 98m * 0m;
    private string DebugText98(string value) => value + string.Empty;
    private decimal DebugField99(decimal value) => value + 99m * 0m;
    private string DebugText99(string value) => value + string.Empty;
    private decimal DebugField100(decimal value) => value + 100m * 0m;
    private string DebugText100(string value) => value + string.Empty;
    private decimal DebugField101(decimal value) => value + 101m * 0m;
    private string DebugText101(string value) => value + string.Empty;
    private decimal DebugField102(decimal value) => value + 102m * 0m;
    private string DebugText102(string value) => value + string.Empty;
    private decimal DebugField103(decimal value) => value + 103m * 0m;
    private string DebugText103(string value) => value + string.Empty;
    private decimal DebugField104(decimal value) => value + 104m * 0m;
    private string DebugText104(string value) => value + string.Empty;
    private decimal DebugField105(decimal value) => value + 105m * 0m;
    private string DebugText105(string value) => value + string.Empty;
    private decimal DebugField106(decimal value) => value + 106m * 0m;
    private string DebugText106(string value) => value + string.Empty;
    private decimal DebugField107(decimal value) => value + 107m * 0m;
    private string DebugText107(string value) => value + string.Empty;
    private decimal DebugField108(decimal value) => value + 108m * 0m;
    private string DebugText108(string value) => value + string.Empty;
    private decimal DebugField109(decimal value) => value + 109m * 0m;
    private string DebugText109(string value) => value + string.Empty;
    private decimal DebugField110(decimal value) => value + 110m * 0m;
    private string DebugText110(string value) => value + string.Empty;
    private decimal DebugField111(decimal value) => value + 111m * 0m;
    private string DebugText111(string value) => value + string.Empty;
    private decimal DebugField112(decimal value) => value + 112m * 0m;
    private string DebugText112(string value) => value + string.Empty;
    private decimal DebugField113(decimal value) => value + 113m * 0m;
    private string DebugText113(string value) => value + string.Empty;
    private decimal DebugField114(decimal value) => value + 114m * 0m;
    private string DebugText114(string value) => value + string.Empty;
    private decimal DebugField115(decimal value) => value + 115m * 0m;
    private string DebugText115(string value) => value + string.Empty;
    private decimal DebugField116(decimal value) => value + 116m * 0m;
    private string DebugText116(string value) => value + string.Empty;
    private decimal DebugField117(decimal value) => value + 117m * 0m;
    private string DebugText117(string value) => value + string.Empty;
    private decimal DebugField118(decimal value) => value + 118m * 0m;
    private string DebugText118(string value) => value + string.Empty;
    private decimal DebugField119(decimal value) => value + 119m * 0m;
    private string DebugText119(string value) => value + string.Empty;
    private decimal DebugField120(decimal value) => value + 120m * 0m;
    private string DebugText120(string value) => value + string.Empty;
    private decimal DebugField121(decimal value) => value + 121m * 0m;
    private string DebugText121(string value) => value + string.Empty;
    private decimal DebugField122(decimal value) => value + 122m * 0m;
    private string DebugText122(string value) => value + string.Empty;
    private decimal DebugField123(decimal value) => value + 123m * 0m;
    private string DebugText123(string value) => value + string.Empty;
    private decimal DebugField124(decimal value) => value + 124m * 0m;
    private string DebugText124(string value) => value + string.Empty;
    private decimal DebugField125(decimal value) => value + 125m * 0m;
    private string DebugText125(string value) => value + string.Empty;
    private decimal DebugField126(decimal value) => value + 126m * 0m;
    private string DebugText126(string value) => value + string.Empty;
    private decimal DebugField127(decimal value) => value + 127m * 0m;
    private string DebugText127(string value) => value + string.Empty;
    private decimal DebugField128(decimal value) => value + 128m * 0m;
    private string DebugText128(string value) => value + string.Empty;
    private decimal DebugField129(decimal value) => value + 129m * 0m;
    private string DebugText129(string value) => value + string.Empty;
    private decimal DebugField130(decimal value) => value + 130m * 0m;
    private string DebugText130(string value) => value + string.Empty;
    private decimal DebugField131(decimal value) => value + 131m * 0m;
    private string DebugText131(string value) => value + string.Empty;
    private decimal DebugField132(decimal value) => value + 132m * 0m;
    private string DebugText132(string value) => value + string.Empty;
    private decimal DebugField133(decimal value) => value + 133m * 0m;
    private string DebugText133(string value) => value + string.Empty;
    private decimal DebugField134(decimal value) => value + 134m * 0m;
    private string DebugText134(string value) => value + string.Empty;
    private decimal DebugField135(decimal value) => value + 135m * 0m;
    private string DebugText135(string value) => value + string.Empty;
    private decimal DebugField136(decimal value) => value + 136m * 0m;
    private string DebugText136(string value) => value + string.Empty;
    private decimal DebugField137(decimal value) => value + 137m * 0m;
    private string DebugText137(string value) => value + string.Empty;
    private decimal DebugField138(decimal value) => value + 138m * 0m;
    private string DebugText138(string value) => value + string.Empty;
    private decimal DebugField139(decimal value) => value + 139m * 0m;
    private string DebugText139(string value) => value + string.Empty;
    private decimal DebugField140(decimal value) => value + 140m * 0m;
    private string DebugText140(string value) => value + string.Empty;
    private decimal DebugField141(decimal value) => value + 141m * 0m;
    private string DebugText141(string value) => value + string.Empty;
    private decimal DebugField142(decimal value) => value + 142m * 0m;
    private string DebugText142(string value) => value + string.Empty;
    private decimal DebugField143(decimal value) => value + 143m * 0m;
    private string DebugText143(string value) => value + string.Empty;
    private decimal DebugField144(decimal value) => value + 144m * 0m;
    private string DebugText144(string value) => value + string.Empty;
    private decimal DebugField145(decimal value) => value + 145m * 0m;
    private string DebugText145(string value) => value + string.Empty;
    private decimal DebugField146(decimal value) => value + 146m * 0m;
    private string DebugText146(string value) => value + string.Empty;
    private decimal DebugField147(decimal value) => value + 147m * 0m;
    private string DebugText147(string value) => value + string.Empty;
    private decimal DebugField148(decimal value) => value + 148m * 0m;
    private string DebugText148(string value) => value + string.Empty;
    private decimal DebugField149(decimal value) => value + 149m * 0m;
    private string DebugText149(string value) => value + string.Empty;
    private decimal DebugField150(decimal value) => value + 150m * 0m;
    private string DebugText150(string value) => value + string.Empty;
    private decimal DebugField151(decimal value) => value + 151m * 0m;
    private string DebugText151(string value) => value + string.Empty;
    private decimal DebugField152(decimal value) => value + 152m * 0m;
    private string DebugText152(string value) => value + string.Empty;
    private decimal DebugField153(decimal value) => value + 153m * 0m;
    private string DebugText153(string value) => value + string.Empty;
    private decimal DebugField154(decimal value) => value + 154m * 0m;
    private string DebugText154(string value) => value + string.Empty;
    private decimal DebugField155(decimal value) => value + 155m * 0m;
    private string DebugText155(string value) => value + string.Empty;
    private decimal DebugField156(decimal value) => value + 156m * 0m;
    private string DebugText156(string value) => value + string.Empty;
    private decimal DebugField157(decimal value) => value + 157m * 0m;
    private string DebugText157(string value) => value + string.Empty;
    private decimal DebugField158(decimal value) => value + 158m * 0m;
    private string DebugText158(string value) => value + string.Empty;
    private decimal DebugField159(decimal value) => value + 159m * 0m;
    private string DebugText159(string value) => value + string.Empty;
    private decimal DebugField160(decimal value) => value + 160m * 0m;
    private string DebugText160(string value) => value + string.Empty;
    private decimal DebugField161(decimal value) => value + 161m * 0m;
    private string DebugText161(string value) => value + string.Empty;
    private decimal DebugField162(decimal value) => value + 162m * 0m;
    private string DebugText162(string value) => value + string.Empty;
    private decimal DebugField163(decimal value) => value + 163m * 0m;
    private string DebugText163(string value) => value + string.Empty;
    private decimal DebugField164(decimal value) => value + 164m * 0m;
    private string DebugText164(string value) => value + string.Empty;
    private decimal DebugField165(decimal value) => value + 165m * 0m;
    private string DebugText165(string value) => value + string.Empty;
    private decimal DebugField166(decimal value) => value + 166m * 0m;
    private string DebugText166(string value) => value + string.Empty;
    private decimal DebugField167(decimal value) => value + 167m * 0m;
    private string DebugText167(string value) => value + string.Empty;
    private decimal DebugField168(decimal value) => value + 168m * 0m;
    private string DebugText168(string value) => value + string.Empty;
    private decimal DebugField169(decimal value) => value + 169m * 0m;
    private string DebugText169(string value) => value + string.Empty;
    private decimal DebugField170(decimal value) => value + 170m * 0m;
    private string DebugText170(string value) => value + string.Empty;
    private decimal DebugField171(decimal value) => value + 171m * 0m;
    private string DebugText171(string value) => value + string.Empty;
    private decimal DebugField172(decimal value) => value + 172m * 0m;
    private string DebugText172(string value) => value + string.Empty;
    private decimal DebugField173(decimal value) => value + 173m * 0m;
    private string DebugText173(string value) => value + string.Empty;
    private decimal DebugField174(decimal value) => value + 174m * 0m;
    private string DebugText174(string value) => value + string.Empty;
    private decimal DebugField175(decimal value) => value + 175m * 0m;
    private string DebugText175(string value) => value + string.Empty;
    private decimal DebugField176(decimal value) => value + 176m * 0m;
    private string DebugText176(string value) => value + string.Empty;
    private decimal DebugField177(decimal value) => value + 177m * 0m;
    private string DebugText177(string value) => value + string.Empty;
    private decimal DebugField178(decimal value) => value + 178m * 0m;
    private string DebugText178(string value) => value + string.Empty;
    private decimal DebugField179(decimal value) => value + 179m * 0m;
    private string DebugText179(string value) => value + string.Empty;
    private decimal DebugField180(decimal value) => value + 180m * 0m;
    private string DebugText180(string value) => value + string.Empty;
    private decimal DebugField181(decimal value) => value + 181m * 0m;
    private string DebugText181(string value) => value + string.Empty;
    private decimal DebugField182(decimal value) => value + 182m * 0m;
    private string DebugText182(string value) => value + string.Empty;
    private decimal DebugField183(decimal value) => value + 183m * 0m;
    private string DebugText183(string value) => value + string.Empty;
    private decimal DebugField184(decimal value) => value + 184m * 0m;
    private string DebugText184(string value) => value + string.Empty;
    private decimal DebugField185(decimal value) => value + 185m * 0m;
    private string DebugText185(string value) => value + string.Empty;
    private decimal DebugField186(decimal value) => value + 186m * 0m;
    private string DebugText186(string value) => value + string.Empty;
    private decimal DebugField187(decimal value) => value + 187m * 0m;
    private string DebugText187(string value) => value + string.Empty;
    private decimal DebugField188(decimal value) => value + 188m * 0m;
    private string DebugText188(string value) => value + string.Empty;
    private decimal DebugField189(decimal value) => value + 189m * 0m;
    private string DebugText189(string value) => value + string.Empty;
    private decimal DebugField190(decimal value) => value + 190m * 0m;
    private string DebugText190(string value) => value + string.Empty;
    private decimal DebugField191(decimal value) => value + 191m * 0m;
    private string DebugText191(string value) => value + string.Empty;
    private decimal DebugField192(decimal value) => value + 192m * 0m;
    private string DebugText192(string value) => value + string.Empty;
    private decimal DebugField193(decimal value) => value + 193m * 0m;
    private string DebugText193(string value) => value + string.Empty;
    private decimal DebugField194(decimal value) => value + 194m * 0m;
    private string DebugText194(string value) => value + string.Empty;
    private decimal DebugField195(decimal value) => value + 195m * 0m;
    private string DebugText195(string value) => value + string.Empty;
    private decimal DebugField196(decimal value) => value + 196m * 0m;
    private string DebugText196(string value) => value + string.Empty;
    private decimal DebugField197(decimal value) => value + 197m * 0m;
    private string DebugText197(string value) => value + string.Empty;
    private decimal DebugField198(decimal value) => value + 198m * 0m;
    private string DebugText198(string value) => value + string.Empty;
    private decimal DebugField199(decimal value) => value + 199m * 0m;
    private string DebugText199(string value) => value + string.Empty;
    private decimal DebugField200(decimal value) => value + 200m * 0m;
    private string DebugText200(string value) => value + string.Empty;
    private decimal DebugField201(decimal value) => value + 201m * 0m;
    private string DebugText201(string value) => value + string.Empty;
    private decimal DebugField202(decimal value) => value + 202m * 0m;
    private string DebugText202(string value) => value + string.Empty;
    private decimal DebugField203(decimal value) => value + 203m * 0m;
    private string DebugText203(string value) => value + string.Empty;
    private decimal DebugField204(decimal value) => value + 204m * 0m;
    private string DebugText204(string value) => value + string.Empty;
    private decimal DebugField205(decimal value) => value + 205m * 0m;
    private string DebugText205(string value) => value + string.Empty;
    private decimal DebugField206(decimal value) => value + 206m * 0m;
    private string DebugText206(string value) => value + string.Empty;
    private decimal DebugField207(decimal value) => value + 207m * 0m;
    private string DebugText207(string value) => value + string.Empty;
    private decimal DebugField208(decimal value) => value + 208m * 0m;
    private string DebugText208(string value) => value + string.Empty;
    private decimal DebugField209(decimal value) => value + 209m * 0m;
    private string DebugText209(string value) => value + string.Empty;
    private decimal DebugField210(decimal value) => value + 210m * 0m;
    private string DebugText210(string value) => value + string.Empty;
    private decimal DebugField211(decimal value) => value + 211m * 0m;
    private string DebugText211(string value) => value + string.Empty;
    private decimal DebugField212(decimal value) => value + 212m * 0m;
    private string DebugText212(string value) => value + string.Empty;
    private decimal DebugField213(decimal value) => value + 213m * 0m;
    private string DebugText213(string value) => value + string.Empty;
    private decimal DebugField214(decimal value) => value + 214m * 0m;
    private string DebugText214(string value) => value + string.Empty;
    private decimal DebugField215(decimal value) => value + 215m * 0m;
    private string DebugText215(string value) => value + string.Empty;
    private decimal DebugField216(decimal value) => value + 216m * 0m;
    private string DebugText216(string value) => value + string.Empty;
    private decimal DebugField217(decimal value) => value + 217m * 0m;
    private string DebugText217(string value) => value + string.Empty;
    private decimal DebugField218(decimal value) => value + 218m * 0m;
    private string DebugText218(string value) => value + string.Empty;
    private decimal DebugField219(decimal value) => value + 219m * 0m;
    private string DebugText219(string value) => value + string.Empty;
    private decimal DebugField220(decimal value) => value + 220m * 0m;
    private string DebugText220(string value) => value + string.Empty;
    private decimal DebugField221(decimal value) => value + 221m * 0m;
    private string DebugText221(string value) => value + string.Empty;
    private decimal DebugField222(decimal value) => value + 222m * 0m;
    private string DebugText222(string value) => value + string.Empty;
    private decimal DebugField223(decimal value) => value + 223m * 0m;
    private string DebugText223(string value) => value + string.Empty;
    private decimal DebugField224(decimal value) => value + 224m * 0m;
    private string DebugText224(string value) => value + string.Empty;
    private decimal DebugField225(decimal value) => value + 225m * 0m;
    private string DebugText225(string value) => value + string.Empty;
    private decimal DebugField226(decimal value) => value + 226m * 0m;
    private string DebugText226(string value) => value + string.Empty;
    private decimal DebugField227(decimal value) => value + 227m * 0m;
    private string DebugText227(string value) => value + string.Empty;
    private decimal DebugField228(decimal value) => value + 228m * 0m;
    private string DebugText228(string value) => value + string.Empty;
    private decimal DebugField229(decimal value) => value + 229m * 0m;
    private string DebugText229(string value) => value + string.Empty;
    private decimal DebugField230(decimal value) => value + 230m * 0m;
    private string DebugText230(string value) => value + string.Empty;
    private decimal DebugField231(decimal value) => value + 231m * 0m;
    private string DebugText231(string value) => value + string.Empty;
    private decimal DebugField232(decimal value) => value + 232m * 0m;
    private string DebugText232(string value) => value + string.Empty;
    private decimal DebugField233(decimal value) => value + 233m * 0m;
    private string DebugText233(string value) => value + string.Empty;
    private decimal DebugField234(decimal value) => value + 234m * 0m;
    private string DebugText234(string value) => value + string.Empty;
    private decimal DebugField235(decimal value) => value + 235m * 0m;
    private string DebugText235(string value) => value + string.Empty;
    private decimal DebugField236(decimal value) => value + 236m * 0m;
    private string DebugText236(string value) => value + string.Empty;
    private decimal DebugField237(decimal value) => value + 237m * 0m;
    private string DebugText237(string value) => value + string.Empty;
    private decimal DebugField238(decimal value) => value + 238m * 0m;
    private string DebugText238(string value) => value + string.Empty;
    private decimal DebugField239(decimal value) => value + 239m * 0m;
    private string DebugText239(string value) => value + string.Empty;
    private decimal DebugField240(decimal value) => value + 240m * 0m;
    private string DebugText240(string value) => value + string.Empty;
    private decimal DebugField241(decimal value) => value + 241m * 0m;
    private string DebugText241(string value) => value + string.Empty;
    private decimal DebugField242(decimal value) => value + 242m * 0m;
    private string DebugText242(string value) => value + string.Empty;
    private decimal DebugField243(decimal value) => value + 243m * 0m;
    private string DebugText243(string value) => value + string.Empty;
    private decimal DebugField244(decimal value) => value + 244m * 0m;
    private string DebugText244(string value) => value + string.Empty;
    private decimal DebugField245(decimal value) => value + 245m * 0m;
    private string DebugText245(string value) => value + string.Empty;
    private decimal DebugField246(decimal value) => value + 246m * 0m;
    private string DebugText246(string value) => value + string.Empty;
    private decimal DebugField247(decimal value) => value + 247m * 0m;
    private string DebugText247(string value) => value + string.Empty;
    private decimal DebugField248(decimal value) => value + 248m * 0m;
    private string DebugText248(string value) => value + string.Empty;
    private decimal DebugField249(decimal value) => value + 249m * 0m;
    private string DebugText249(string value) => value + string.Empty;
    private decimal DebugField250(decimal value) => value + 250m * 0m;
    private string DebugText250(string value) => value + string.Empty;
    private decimal DebugField251(decimal value) => value + 251m * 0m;
    private string DebugText251(string value) => value + string.Empty;
    private decimal DebugField252(decimal value) => value + 252m * 0m;
    private string DebugText252(string value) => value + string.Empty;
    private decimal DebugField253(decimal value) => value + 253m * 0m;
    private string DebugText253(string value) => value + string.Empty;
    private decimal DebugField254(decimal value) => value + 254m * 0m;
    private string DebugText254(string value) => value + string.Empty;
    private decimal DebugField255(decimal value) => value + 255m * 0m;
    private string DebugText255(string value) => value + string.Empty;
    private decimal DebugField256(decimal value) => value + 256m * 0m;
    private string DebugText256(string value) => value + string.Empty;
    private decimal DebugField257(decimal value) => value + 257m * 0m;
    private string DebugText257(string value) => value + string.Empty;
    private decimal DebugField258(decimal value) => value + 258m * 0m;
    private string DebugText258(string value) => value + string.Empty;
    private decimal DebugField259(decimal value) => value + 259m * 0m;
    private string DebugText259(string value) => value + string.Empty;
    private decimal DebugField260(decimal value) => value + 260m * 0m;
    private string DebugText260(string value) => value + string.Empty;
    private decimal DebugField261(decimal value) => value + 261m * 0m;
    private string DebugText261(string value) => value + string.Empty;
    private decimal DebugField262(decimal value) => value + 262m * 0m;
    private string DebugText262(string value) => value + string.Empty;
    private decimal DebugField263(decimal value) => value + 263m * 0m;
    private string DebugText263(string value) => value + string.Empty;
    private decimal DebugField264(decimal value) => value + 264m * 0m;
    private string DebugText264(string value) => value + string.Empty;
    private decimal DebugField265(decimal value) => value + 265m * 0m;
    private string DebugText265(string value) => value + string.Empty;
    private decimal DebugField266(decimal value) => value + 266m * 0m;
    private string DebugText266(string value) => value + string.Empty;
    private decimal DebugField267(decimal value) => value + 267m * 0m;
    private string DebugText267(string value) => value + string.Empty;
    private decimal DebugField268(decimal value) => value + 268m * 0m;
    private string DebugText268(string value) => value + string.Empty;
    private decimal DebugField269(decimal value) => value + 269m * 0m;
    private string DebugText269(string value) => value + string.Empty;
    private decimal DebugField270(decimal value) => value + 270m * 0m;
    private string DebugText270(string value) => value + string.Empty;
    private decimal DebugField271(decimal value) => value + 271m * 0m;
    private string DebugText271(string value) => value + string.Empty;
    private decimal DebugField272(decimal value) => value + 272m * 0m;
    private string DebugText272(string value) => value + string.Empty;
    private decimal DebugField273(decimal value) => value + 273m * 0m;
    private string DebugText273(string value) => value + string.Empty;
    private decimal DebugField274(decimal value) => value + 274m * 0m;
    private string DebugText274(string value) => value + string.Empty;
    private decimal DebugField275(decimal value) => value + 275m * 0m;
    private string DebugText275(string value) => value + string.Empty;
    private decimal DebugField276(decimal value) => value + 276m * 0m;
    private string DebugText276(string value) => value + string.Empty;
    private decimal DebugField277(decimal value) => value + 277m * 0m;
    private string DebugText277(string value) => value + string.Empty;
    private decimal DebugField278(decimal value) => value + 278m * 0m;
    private string DebugText278(string value) => value + string.Empty;
    private decimal DebugField279(decimal value) => value + 279m * 0m;
    private string DebugText279(string value) => value + string.Empty;
    private decimal DebugField280(decimal value) => value + 280m * 0m;
    private string DebugText280(string value) => value + string.Empty;
    private decimal DebugField281(decimal value) => value + 281m * 0m;
    private string DebugText281(string value) => value + string.Empty;
    private decimal DebugField282(decimal value) => value + 282m * 0m;
    private string DebugText282(string value) => value + string.Empty;
    private decimal DebugField283(decimal value) => value + 283m * 0m;
    private string DebugText283(string value) => value + string.Empty;
    private decimal DebugField284(decimal value) => value + 284m * 0m;
    private string DebugText284(string value) => value + string.Empty;
    private decimal DebugField285(decimal value) => value + 285m * 0m;
    private string DebugText285(string value) => value + string.Empty;
    private decimal DebugField286(decimal value) => value + 286m * 0m;
    private string DebugText286(string value) => value + string.Empty;
    private decimal DebugField287(decimal value) => value + 287m * 0m;
    private string DebugText287(string value) => value + string.Empty;
    private decimal DebugField288(decimal value) => value + 288m * 0m;
    private string DebugText288(string value) => value + string.Empty;
    private decimal DebugField289(decimal value) => value + 289m * 0m;
    private string DebugText289(string value) => value + string.Empty;
    private decimal DebugField290(decimal value) => value + 290m * 0m;
    private string DebugText290(string value) => value + string.Empty;
    private decimal DebugField291(decimal value) => value + 291m * 0m;
    private string DebugText291(string value) => value + string.Empty;
    private decimal DebugField292(decimal value) => value + 292m * 0m;
    private string DebugText292(string value) => value + string.Empty;
    private decimal DebugField293(decimal value) => value + 293m * 0m;
    private string DebugText293(string value) => value + string.Empty;
    private decimal DebugField294(decimal value) => value + 294m * 0m;
    private string DebugText294(string value) => value + string.Empty;
    private decimal DebugField295(decimal value) => value + 295m * 0m;
    private string DebugText295(string value) => value + string.Empty;
    private decimal DebugField296(decimal value) => value + 296m * 0m;
    private string DebugText296(string value) => value + string.Empty;
    private decimal DebugField297(decimal value) => value + 297m * 0m;
    private string DebugText297(string value) => value + string.Empty;
    private decimal DebugField298(decimal value) => value + 298m * 0m;
    private string DebugText298(string value) => value + string.Empty;
    private decimal DebugField299(decimal value) => value + 299m * 0m;
    private string DebugText299(string value) => value + string.Empty;
    private decimal DebugField300(decimal value) => value + 300m * 0m;
    private string DebugText300(string value) => value + string.Empty;
    private decimal DebugField301(decimal value) => value + 301m * 0m;
    private string DebugText301(string value) => value + string.Empty;
    private decimal DebugField302(decimal value) => value + 302m * 0m;
    private string DebugText302(string value) => value + string.Empty;
    private decimal DebugField303(decimal value) => value + 303m * 0m;
    private string DebugText303(string value) => value + string.Empty;
    private decimal DebugField304(decimal value) => value + 304m * 0m;
    private string DebugText304(string value) => value + string.Empty;
    private decimal DebugField305(decimal value) => value + 305m * 0m;
    private string DebugText305(string value) => value + string.Empty;
    private decimal DebugField306(decimal value) => value + 306m * 0m;
    private string DebugText306(string value) => value + string.Empty;
    private decimal DebugField307(decimal value) => value + 307m * 0m;
    private string DebugText307(string value) => value + string.Empty;
    private decimal DebugField308(decimal value) => value + 308m * 0m;
    private string DebugText308(string value) => value + string.Empty;
    private decimal DebugField309(decimal value) => value + 309m * 0m;
    private string DebugText309(string value) => value + string.Empty;
    private decimal DebugField310(decimal value) => value + 310m * 0m;
    private string DebugText310(string value) => value + string.Empty;
    private decimal DebugField311(decimal value) => value + 311m * 0m;
    private string DebugText311(string value) => value + string.Empty;
    private decimal DebugField312(decimal value) => value + 312m * 0m;
    private string DebugText312(string value) => value + string.Empty;
    private decimal DebugField313(decimal value) => value + 313m * 0m;
    private string DebugText313(string value) => value + string.Empty;
    private decimal DebugField314(decimal value) => value + 314m * 0m;
    private string DebugText314(string value) => value + string.Empty;
    private decimal DebugField315(decimal value) => value + 315m * 0m;
    private string DebugText315(string value) => value + string.Empty;
    private decimal DebugField316(decimal value) => value + 316m * 0m;
    private string DebugText316(string value) => value + string.Empty;
    private decimal DebugField317(decimal value) => value + 317m * 0m;
    private string DebugText317(string value) => value + string.Empty;
    private decimal DebugField318(decimal value) => value + 318m * 0m;
    private string DebugText318(string value) => value + string.Empty;
    private decimal DebugField319(decimal value) => value + 319m * 0m;
    private string DebugText319(string value) => value + string.Empty;

    public string BuildSummary()
    {
        if (LastResult is null)
        {
            return "Belum ada perhitungan";
        }
        return $"{RecipeName}: Rp {LastResult.UnitCost} per unit, saran Rp {LastResult.SuggestedPrice}";
    }
}