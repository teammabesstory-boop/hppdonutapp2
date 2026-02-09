# HppDonatApp

Aplikasi desktop WinUI 3 untuk kalkulasi HPP donat (Rp/IDR) dengan arsitektur MVVM, DI, EF Core (SQLite), logging, dan testing.

## Fitur utama
- CRUD Resep & Bahan, termasuk histori harga time-series.
- Kalkulasi HPP per batch dengan rounding rules dan PPN.
- CSV import/export, logging Serilog, dan seed data awal.
- Unit tests xUnit + CI GitHub Actions.

## Prasyarat
- .NET 10 SDK (atau versi preview yang mendukung `net10.0-windows`).
- Windows 10+ untuk WinUI 3.

## Scaffolding (perintah dari prompt)
```bash
dotnet new sln -n HppDonatApp
# If CLI supports WinUI template:
# dotnet new winui -n HppDonatApp --framework net10-windows
dotnet new classlib -n HppDonatApp.Core
dotnet new classlib -n HppDonatApp.Data
dotnet new classlib -n HppDonatApp.Services
dotnet new xunit -n HppDonatApp.Tests
dotnet sln add HppDonatApp/HppDonatApp.csproj
dotnet restore
dotnet build
dotnet test
```

### Fallback jika template WinUI tidak tersedia
Gunakan Visual Studio:
1. `File > New > Project > Blank App, Packaged (WinUI 3 in Desktop)`.
2. Set target `net10.0-windows` jika tersedia.
3. Tambahkan project references ke `HppDonatApp.Core`, `HppDonatApp.Data`, dan `HppDonatApp.Services`.

## Menjalankan aplikasi
```bash
dotnet build
```
Jalankan dari Visual Studio untuk pengalaman WinUI terbaik.

## Paket NuGet (utama)
- Microsoft.WindowsAppSDK, WinUIEx
- CommunityToolkit.Mvvm
- Microsoft.EntityFrameworkCore.Sqlite
- Serilog + Serilog.Sinks.File
- CsvHelper

Jika paket tertentu tidak kompatibel dengan `net10.0`, gunakan versi stabil terakhir untuk `net8.0` sebagai fallback.

## Catatan Rupiah
Semua perhitungan finansial menggunakan `decimal` dan mata uang Rupiah (Rp/IDR).

## Lisensi
MIT. Lihat `LICENSE`.
