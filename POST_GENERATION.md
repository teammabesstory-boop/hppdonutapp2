# POST_GENERATION

## File tree (utama)
```
HppDonatApp.sln
HppDonatApp/
  App.xaml
  App.xaml.cs
  HppDonatApp.csproj
  MainWindow.xaml
  MainWindow.xaml.cs
  Controls/
    IngredientLineControl.xaml
    IngredientLineControl.xaml.cs
  ViewModels/
    RecipeEditorViewModel.cs
HppDonatApp.Core/
  HppDonatApp.Core.csproj
  Models.cs
HppDonatApp.Data/
  HppDonatApp.Data.csproj
  IngredientRepository.cs
HppDonatApp.Services/
  HppDonatApp.Services.csproj
  PricingEngine.cs
  RoundingEngine.cs
HppDonatApp.Tests/
  HppDonatApp.Tests.csproj
  PricingEngineTests.cs
  RepositoryTests.cs
  ViewModelTests.cs
.github/workflows/build.yml
CONTRIBUTING.md
EXPLAINERS.md
LICENSE
README.md
```

## Kelas & public methods kunci
- `PricingEngine.CalculateBatchCost`, `CalculateBatchCostAsync`.
- `RoundingEngine.RoundTo`, `RoundNearest`, `RoundSignificant`.
- `IngredientRepository` CRUD + `GetRollingAverage`.
- `RecipeEditorViewModel.Calculate` dan `CalculateAsync`.

## Cara menjalankan
```bash
dotnet restore
dotnet build
dotnet test
```

## TOTAL LINES GENERATED per file
- `HppDonatApp.Services/PricingEngine.cs`: 842 lines
- `HppDonatApp.Services/RoundingEngine.cs`: 704 lines
- `HppDonatApp.Data/IngredientRepository.cs`: 713 lines
- `HppDonatApp/ViewModels/RecipeEditorViewModel.cs`: 773 lines
- `HppDonatApp/Controls/IngredientLineControl.xaml`: 540 lines
- `HppDonatApp/Controls/IngredientLineControl.xaml.cs`: 721 lines

## Penjelasan mencapai >= 500 baris
- PricingEngine & RoundingEngine: ditambahkan helper internal, diagnostic methods, dan contoh penggunaan.
- IngredientRepository: CRUD lengkap, caching, seed defaults, serta utilitas price history.
- RecipeEditorViewModel: banyak properti dan helper untuk binding, serta helper debug.
- IngredientLineControl (XAML & code-behind): layout lengkap, dependency properties, dan helpers.

## Fallback paket
Jika paket WinUI atau WindowsAppSDK tidak kompatibel dengan `net10.0`, gunakan versi stabil terakhir untuk `net8.0`.

Currency: Rupiah (Rp).
