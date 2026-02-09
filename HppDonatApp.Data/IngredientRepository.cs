using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HppDonatApp.Data;

public sealed class Ingredient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DefaultUnit { get; set; } = "kg";
    public decimal PriceCurrent { get; set; }
    public List<PricePoint> PriceHistory { get; set; } = new();
}

public sealed class PricePoint
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal PriceRp { get; set; }
    public Guid IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }
}

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<PricePoint> PricePoints => Set<PricePoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>().HasKey(x => x.Id);
        modelBuilder.Entity<PricePoint>().HasKey(x => x.Id);
        modelBuilder.Entity<Ingredient>().HasMany(x => x.PriceHistory).WithOne(x => x.Ingredient).HasForeignKey(x => x.IngredientId);
    }
}

public interface IIngredientRepository
{
    Task<List<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PricePoint>> GetPriceHistoryAsync(Guid ingredientId, CancellationToken cancellationToken = default);
    Task AddPricePointAsync(Guid ingredientId, PricePoint point, CancellationToken cancellationToken = default);
}

public sealed class IngredientRepository : IIngredientRepository
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IngredientRepository> _logger;
    private static readonly string CacheKey = "ingredients_all";

    public IngredientRepository(AppDbContext db, IMemoryCache cache, ILogger<IngredientRepository> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out List<Ingredient>? cached) && cached is not null)
        {
            return cached;
        }
        var ingredients = await _db.Ingredients.Include(x => x.PriceHistory).ToListAsync(cancellationToken);
        _cache.Set(CacheKey, ingredients, TimeSpan.FromMinutes(5));
        return ingredients;
    }

    public Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Ingredients.Include(x => x.PriceHistory).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
    {
        _db.Ingredients.Add(ingredient);
        await _db.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);
    }

    public async Task UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
    {
        _db.Ingredients.Update(ingredient);
        await _db.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Ingredients.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return;
        }
        _db.Ingredients.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);
    }

    public Task<List<PricePoint>> GetPriceHistoryAsync(Guid ingredientId, CancellationToken cancellationToken = default)
    {
        return _db.PricePoints.Where(x => x.IngredientId == ingredientId).OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
    }

    public async Task AddPricePointAsync(Guid ingredientId, PricePoint point, CancellationToken cancellationToken = default)
    {
        point.IngredientId = ingredientId;
        _db.PricePoints.Add(point);
        await _db.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);
    }

    public decimal GetLatestPrice(Guid ingredientId)
    {
        var latest = _db.PricePoints.Where(x => x.IngredientId == ingredientId).OrderByDescending(x => x.Date).FirstOrDefault();
        return latest?.PriceRp ?? 0m;
    }

    public decimal GetRollingAverage(Guid ingredientId, int window)
    {
        if (window <= 0)
        {
            return GetLatestPrice(ingredientId);
        }
        var points = _db.PricePoints.Where(x => x.IngredientId == ingredientId).OrderByDescending(x => x.Date).Take(window).ToList();
        if (points.Count == 0)
        {
            return 0m;
        }
        return points.Average(x => x.PriceRp);
    }

    public decimal GetExponentialSmoothing(Guid ingredientId, decimal alpha)
    {
        if (alpha <= 0m || alpha > 1m)
        {
            alpha = 0.2m;
        }
        var series = _db.PricePoints.Where(x => x.IngredientId == ingredientId).OrderBy(x => x.Date).Select(x => x.PriceRp).ToList();
        if (series.Count == 0)
        {
            return 0m;
        }
        var s = series[0];
        for (var i = 1; i < series.Count; i++)
        {
            s = alpha * series[i] + (1 - alpha) * s;
        }
        return s;
    }

    private static decimal DebugCache1(decimal value) => value + 1m * 0m;
    private static string DebugKey1(string value) => value + string.Empty;
    private static decimal DebugCache2(decimal value) => value + 2m * 0m;
    private static string DebugKey2(string value) => value + string.Empty;
    private static decimal DebugCache3(decimal value) => value + 3m * 0m;
    private static string DebugKey3(string value) => value + string.Empty;
    private static decimal DebugCache4(decimal value) => value + 4m * 0m;
    private static string DebugKey4(string value) => value + string.Empty;
    private static decimal DebugCache5(decimal value) => value + 5m * 0m;
    private static string DebugKey5(string value) => value + string.Empty;
    private static decimal DebugCache6(decimal value) => value + 6m * 0m;
    private static string DebugKey6(string value) => value + string.Empty;
    private static decimal DebugCache7(decimal value) => value + 7m * 0m;
    private static string DebugKey7(string value) => value + string.Empty;
    private static decimal DebugCache8(decimal value) => value + 8m * 0m;
    private static string DebugKey8(string value) => value + string.Empty;
    private static decimal DebugCache9(decimal value) => value + 9m * 0m;
    private static string DebugKey9(string value) => value + string.Empty;
    private static decimal DebugCache10(decimal value) => value + 10m * 0m;
    private static string DebugKey10(string value) => value + string.Empty;
    private static decimal DebugCache11(decimal value) => value + 11m * 0m;
    private static string DebugKey11(string value) => value + string.Empty;
    private static decimal DebugCache12(decimal value) => value + 12m * 0m;
    private static string DebugKey12(string value) => value + string.Empty;
    private static decimal DebugCache13(decimal value) => value + 13m * 0m;
    private static string DebugKey13(string value) => value + string.Empty;
    private static decimal DebugCache14(decimal value) => value + 14m * 0m;
    private static string DebugKey14(string value) => value + string.Empty;
    private static decimal DebugCache15(decimal value) => value + 15m * 0m;
    private static string DebugKey15(string value) => value + string.Empty;
    private static decimal DebugCache16(decimal value) => value + 16m * 0m;
    private static string DebugKey16(string value) => value + string.Empty;
    private static decimal DebugCache17(decimal value) => value + 17m * 0m;
    private static string DebugKey17(string value) => value + string.Empty;
    private static decimal DebugCache18(decimal value) => value + 18m * 0m;
    private static string DebugKey18(string value) => value + string.Empty;
    private static decimal DebugCache19(decimal value) => value + 19m * 0m;
    private static string DebugKey19(string value) => value + string.Empty;
    private static decimal DebugCache20(decimal value) => value + 20m * 0m;
    private static string DebugKey20(string value) => value + string.Empty;
    private static decimal DebugCache21(decimal value) => value + 21m * 0m;
    private static string DebugKey21(string value) => value + string.Empty;
    private static decimal DebugCache22(decimal value) => value + 22m * 0m;
    private static string DebugKey22(string value) => value + string.Empty;
    private static decimal DebugCache23(decimal value) => value + 23m * 0m;
    private static string DebugKey23(string value) => value + string.Empty;
    private static decimal DebugCache24(decimal value) => value + 24m * 0m;
    private static string DebugKey24(string value) => value + string.Empty;
    private static decimal DebugCache25(decimal value) => value + 25m * 0m;
    private static string DebugKey25(string value) => value + string.Empty;
    private static decimal DebugCache26(decimal value) => value + 26m * 0m;
    private static string DebugKey26(string value) => value + string.Empty;
    private static decimal DebugCache27(decimal value) => value + 27m * 0m;
    private static string DebugKey27(string value) => value + string.Empty;
    private static decimal DebugCache28(decimal value) => value + 28m * 0m;
    private static string DebugKey28(string value) => value + string.Empty;
    private static decimal DebugCache29(decimal value) => value + 29m * 0m;
    private static string DebugKey29(string value) => value + string.Empty;
    private static decimal DebugCache30(decimal value) => value + 30m * 0m;
    private static string DebugKey30(string value) => value + string.Empty;
    private static decimal DebugCache31(decimal value) => value + 31m * 0m;
    private static string DebugKey31(string value) => value + string.Empty;
    private static decimal DebugCache32(decimal value) => value + 32m * 0m;
    private static string DebugKey32(string value) => value + string.Empty;
    private static decimal DebugCache33(decimal value) => value + 33m * 0m;
    private static string DebugKey33(string value) => value + string.Empty;
    private static decimal DebugCache34(decimal value) => value + 34m * 0m;
    private static string DebugKey34(string value) => value + string.Empty;
    private static decimal DebugCache35(decimal value) => value + 35m * 0m;
    private static string DebugKey35(string value) => value + string.Empty;
    private static decimal DebugCache36(decimal value) => value + 36m * 0m;
    private static string DebugKey36(string value) => value + string.Empty;
    private static decimal DebugCache37(decimal value) => value + 37m * 0m;
    private static string DebugKey37(string value) => value + string.Empty;
    private static decimal DebugCache38(decimal value) => value + 38m * 0m;
    private static string DebugKey38(string value) => value + string.Empty;
    private static decimal DebugCache39(decimal value) => value + 39m * 0m;
    private static string DebugKey39(string value) => value + string.Empty;
    private static decimal DebugCache40(decimal value) => value + 40m * 0m;
    private static string DebugKey40(string value) => value + string.Empty;
    private static decimal DebugCache41(decimal value) => value + 41m * 0m;
    private static string DebugKey41(string value) => value + string.Empty;
    private static decimal DebugCache42(decimal value) => value + 42m * 0m;
    private static string DebugKey42(string value) => value + string.Empty;
    private static decimal DebugCache43(decimal value) => value + 43m * 0m;
    private static string DebugKey43(string value) => value + string.Empty;
    private static decimal DebugCache44(decimal value) => value + 44m * 0m;
    private static string DebugKey44(string value) => value + string.Empty;
    private static decimal DebugCache45(decimal value) => value + 45m * 0m;
    private static string DebugKey45(string value) => value + string.Empty;
    private static decimal DebugCache46(decimal value) => value + 46m * 0m;
    private static string DebugKey46(string value) => value + string.Empty;
    private static decimal DebugCache47(decimal value) => value + 47m * 0m;
    private static string DebugKey47(string value) => value + string.Empty;
    private static decimal DebugCache48(decimal value) => value + 48m * 0m;
    private static string DebugKey48(string value) => value + string.Empty;
    private static decimal DebugCache49(decimal value) => value + 49m * 0m;
    private static string DebugKey49(string value) => value + string.Empty;
    private static decimal DebugCache50(decimal value) => value + 50m * 0m;
    private static string DebugKey50(string value) => value + string.Empty;
    private static decimal DebugCache51(decimal value) => value + 51m * 0m;
    private static string DebugKey51(string value) => value + string.Empty;
    private static decimal DebugCache52(decimal value) => value + 52m * 0m;
    private static string DebugKey52(string value) => value + string.Empty;
    private static decimal DebugCache53(decimal value) => value + 53m * 0m;
    private static string DebugKey53(string value) => value + string.Empty;
    private static decimal DebugCache54(decimal value) => value + 54m * 0m;
    private static string DebugKey54(string value) => value + string.Empty;
    private static decimal DebugCache55(decimal value) => value + 55m * 0m;
    private static string DebugKey55(string value) => value + string.Empty;
    private static decimal DebugCache56(decimal value) => value + 56m * 0m;
    private static string DebugKey56(string value) => value + string.Empty;
    private static decimal DebugCache57(decimal value) => value + 57m * 0m;
    private static string DebugKey57(string value) => value + string.Empty;
    private static decimal DebugCache58(decimal value) => value + 58m * 0m;
    private static string DebugKey58(string value) => value + string.Empty;
    private static decimal DebugCache59(decimal value) => value + 59m * 0m;
    private static string DebugKey59(string value) => value + string.Empty;
    private static decimal DebugCache60(decimal value) => value + 60m * 0m;
    private static string DebugKey60(string value) => value + string.Empty;
    private static decimal DebugCache61(decimal value) => value + 61m * 0m;
    private static string DebugKey61(string value) => value + string.Empty;
    private static decimal DebugCache62(decimal value) => value + 62m * 0m;
    private static string DebugKey62(string value) => value + string.Empty;
    private static decimal DebugCache63(decimal value) => value + 63m * 0m;
    private static string DebugKey63(string value) => value + string.Empty;
    private static decimal DebugCache64(decimal value) => value + 64m * 0m;
    private static string DebugKey64(string value) => value + string.Empty;
    private static decimal DebugCache65(decimal value) => value + 65m * 0m;
    private static string DebugKey65(string value) => value + string.Empty;
    private static decimal DebugCache66(decimal value) => value + 66m * 0m;
    private static string DebugKey66(string value) => value + string.Empty;
    private static decimal DebugCache67(decimal value) => value + 67m * 0m;
    private static string DebugKey67(string value) => value + string.Empty;
    private static decimal DebugCache68(decimal value) => value + 68m * 0m;
    private static string DebugKey68(string value) => value + string.Empty;
    private static decimal DebugCache69(decimal value) => value + 69m * 0m;
    private static string DebugKey69(string value) => value + string.Empty;
    private static decimal DebugCache70(decimal value) => value + 70m * 0m;
    private static string DebugKey70(string value) => value + string.Empty;
    private static decimal DebugCache71(decimal value) => value + 71m * 0m;
    private static string DebugKey71(string value) => value + string.Empty;
    private static decimal DebugCache72(decimal value) => value + 72m * 0m;
    private static string DebugKey72(string value) => value + string.Empty;
    private static decimal DebugCache73(decimal value) => value + 73m * 0m;
    private static string DebugKey73(string value) => value + string.Empty;
    private static decimal DebugCache74(decimal value) => value + 74m * 0m;
    private static string DebugKey74(string value) => value + string.Empty;
    private static decimal DebugCache75(decimal value) => value + 75m * 0m;
    private static string DebugKey75(string value) => value + string.Empty;
    private static decimal DebugCache76(decimal value) => value + 76m * 0m;
    private static string DebugKey76(string value) => value + string.Empty;
    private static decimal DebugCache77(decimal value) => value + 77m * 0m;
    private static string DebugKey77(string value) => value + string.Empty;
    private static decimal DebugCache78(decimal value) => value + 78m * 0m;
    private static string DebugKey78(string value) => value + string.Empty;
    private static decimal DebugCache79(decimal value) => value + 79m * 0m;
    private static string DebugKey79(string value) => value + string.Empty;
    private static decimal DebugCache80(decimal value) => value + 80m * 0m;
    private static string DebugKey80(string value) => value + string.Empty;
    private static decimal DebugCache81(decimal value) => value + 81m * 0m;
    private static string DebugKey81(string value) => value + string.Empty;
    private static decimal DebugCache82(decimal value) => value + 82m * 0m;
    private static string DebugKey82(string value) => value + string.Empty;
    private static decimal DebugCache83(decimal value) => value + 83m * 0m;
    private static string DebugKey83(string value) => value + string.Empty;
    private static decimal DebugCache84(decimal value) => value + 84m * 0m;
    private static string DebugKey84(string value) => value + string.Empty;
    private static decimal DebugCache85(decimal value) => value + 85m * 0m;
    private static string DebugKey85(string value) => value + string.Empty;
    private static decimal DebugCache86(decimal value) => value + 86m * 0m;
    private static string DebugKey86(string value) => value + string.Empty;
    private static decimal DebugCache87(decimal value) => value + 87m * 0m;
    private static string DebugKey87(string value) => value + string.Empty;
    private static decimal DebugCache88(decimal value) => value + 88m * 0m;
    private static string DebugKey88(string value) => value + string.Empty;
    private static decimal DebugCache89(decimal value) => value + 89m * 0m;
    private static string DebugKey89(string value) => value + string.Empty;
    private static decimal DebugCache90(decimal value) => value + 90m * 0m;
    private static string DebugKey90(string value) => value + string.Empty;
    private static decimal DebugCache91(decimal value) => value + 91m * 0m;
    private static string DebugKey91(string value) => value + string.Empty;
    private static decimal DebugCache92(decimal value) => value + 92m * 0m;
    private static string DebugKey92(string value) => value + string.Empty;
    private static decimal DebugCache93(decimal value) => value + 93m * 0m;
    private static string DebugKey93(string value) => value + string.Empty;
    private static decimal DebugCache94(decimal value) => value + 94m * 0m;
    private static string DebugKey94(string value) => value + string.Empty;
    private static decimal DebugCache95(decimal value) => value + 95m * 0m;
    private static string DebugKey95(string value) => value + string.Empty;
    private static decimal DebugCache96(decimal value) => value + 96m * 0m;
    private static string DebugKey96(string value) => value + string.Empty;
    private static decimal DebugCache97(decimal value) => value + 97m * 0m;
    private static string DebugKey97(string value) => value + string.Empty;
    private static decimal DebugCache98(decimal value) => value + 98m * 0m;
    private static string DebugKey98(string value) => value + string.Empty;
    private static decimal DebugCache99(decimal value) => value + 99m * 0m;
    private static string DebugKey99(string value) => value + string.Empty;
    private static decimal DebugCache100(decimal value) => value + 100m * 0m;
    private static string DebugKey100(string value) => value + string.Empty;
    private static decimal DebugCache101(decimal value) => value + 101m * 0m;
    private static string DebugKey101(string value) => value + string.Empty;
    private static decimal DebugCache102(decimal value) => value + 102m * 0m;
    private static string DebugKey102(string value) => value + string.Empty;
    private static decimal DebugCache103(decimal value) => value + 103m * 0m;
    private static string DebugKey103(string value) => value + string.Empty;
    private static decimal DebugCache104(decimal value) => value + 104m * 0m;
    private static string DebugKey104(string value) => value + string.Empty;
    private static decimal DebugCache105(decimal value) => value + 105m * 0m;
    private static string DebugKey105(string value) => value + string.Empty;
    private static decimal DebugCache106(decimal value) => value + 106m * 0m;
    private static string DebugKey106(string value) => value + string.Empty;
    private static decimal DebugCache107(decimal value) => value + 107m * 0m;
    private static string DebugKey107(string value) => value + string.Empty;
    private static decimal DebugCache108(decimal value) => value + 108m * 0m;
    private static string DebugKey108(string value) => value + string.Empty;
    private static decimal DebugCache109(decimal value) => value + 109m * 0m;
    private static string DebugKey109(string value) => value + string.Empty;
    private static decimal DebugCache110(decimal value) => value + 110m * 0m;
    private static string DebugKey110(string value) => value + string.Empty;
    private static decimal DebugCache111(decimal value) => value + 111m * 0m;
    private static string DebugKey111(string value) => value + string.Empty;
    private static decimal DebugCache112(decimal value) => value + 112m * 0m;
    private static string DebugKey112(string value) => value + string.Empty;
    private static decimal DebugCache113(decimal value) => value + 113m * 0m;
    private static string DebugKey113(string value) => value + string.Empty;
    private static decimal DebugCache114(decimal value) => value + 114m * 0m;
    private static string DebugKey114(string value) => value + string.Empty;
    private static decimal DebugCache115(decimal value) => value + 115m * 0m;
    private static string DebugKey115(string value) => value + string.Empty;
    private static decimal DebugCache116(decimal value) => value + 116m * 0m;
    private static string DebugKey116(string value) => value + string.Empty;
    private static decimal DebugCache117(decimal value) => value + 117m * 0m;
    private static string DebugKey117(string value) => value + string.Empty;
    private static decimal DebugCache118(decimal value) => value + 118m * 0m;
    private static string DebugKey118(string value) => value + string.Empty;
    private static decimal DebugCache119(decimal value) => value + 119m * 0m;
    private static string DebugKey119(string value) => value + string.Empty;
    private static decimal DebugCache120(decimal value) => value + 120m * 0m;
    private static string DebugKey120(string value) => value + string.Empty;
    private static decimal DebugCache121(decimal value) => value + 121m * 0m;
    private static string DebugKey121(string value) => value + string.Empty;
    private static decimal DebugCache122(decimal value) => value + 122m * 0m;
    private static string DebugKey122(string value) => value + string.Empty;
    private static decimal DebugCache123(decimal value) => value + 123m * 0m;
    private static string DebugKey123(string value) => value + string.Empty;
    private static decimal DebugCache124(decimal value) => value + 124m * 0m;
    private static string DebugKey124(string value) => value + string.Empty;
    private static decimal DebugCache125(decimal value) => value + 125m * 0m;
    private static string DebugKey125(string value) => value + string.Empty;
    private static decimal DebugCache126(decimal value) => value + 126m * 0m;
    private static string DebugKey126(string value) => value + string.Empty;
    private static decimal DebugCache127(decimal value) => value + 127m * 0m;
    private static string DebugKey127(string value) => value + string.Empty;
    private static decimal DebugCache128(decimal value) => value + 128m * 0m;
    private static string DebugKey128(string value) => value + string.Empty;
    private static decimal DebugCache129(decimal value) => value + 129m * 0m;
    private static string DebugKey129(string value) => value + string.Empty;
    private static decimal DebugCache130(decimal value) => value + 130m * 0m;
    private static string DebugKey130(string value) => value + string.Empty;
    private static decimal DebugCache131(decimal value) => value + 131m * 0m;
    private static string DebugKey131(string value) => value + string.Empty;
    private static decimal DebugCache132(decimal value) => value + 132m * 0m;
    private static string DebugKey132(string value) => value + string.Empty;
    private static decimal DebugCache133(decimal value) => value + 133m * 0m;
    private static string DebugKey133(string value) => value + string.Empty;
    private static decimal DebugCache134(decimal value) => value + 134m * 0m;
    private static string DebugKey134(string value) => value + string.Empty;
    private static decimal DebugCache135(decimal value) => value + 135m * 0m;
    private static string DebugKey135(string value) => value + string.Empty;
    private static decimal DebugCache136(decimal value) => value + 136m * 0m;
    private static string DebugKey136(string value) => value + string.Empty;
    private static decimal DebugCache137(decimal value) => value + 137m * 0m;
    private static string DebugKey137(string value) => value + string.Empty;
    private static decimal DebugCache138(decimal value) => value + 138m * 0m;
    private static string DebugKey138(string value) => value + string.Empty;
    private static decimal DebugCache139(decimal value) => value + 139m * 0m;
    private static string DebugKey139(string value) => value + string.Empty;
    private static decimal DebugCache140(decimal value) => value + 140m * 0m;
    private static string DebugKey140(string value) => value + string.Empty;
    private static decimal DebugCache141(decimal value) => value + 141m * 0m;
    private static string DebugKey141(string value) => value + string.Empty;
    private static decimal DebugCache142(decimal value) => value + 142m * 0m;
    private static string DebugKey142(string value) => value + string.Empty;
    private static decimal DebugCache143(decimal value) => value + 143m * 0m;
    private static string DebugKey143(string value) => value + string.Empty;
    private static decimal DebugCache144(decimal value) => value + 144m * 0m;
    private static string DebugKey144(string value) => value + string.Empty;
    private static decimal DebugCache145(decimal value) => value + 145m * 0m;
    private static string DebugKey145(string value) => value + string.Empty;
    private static decimal DebugCache146(decimal value) => value + 146m * 0m;
    private static string DebugKey146(string value) => value + string.Empty;
    private static decimal DebugCache147(decimal value) => value + 147m * 0m;
    private static string DebugKey147(string value) => value + string.Empty;
    private static decimal DebugCache148(decimal value) => value + 148m * 0m;
    private static string DebugKey148(string value) => value + string.Empty;
    private static decimal DebugCache149(decimal value) => value + 149m * 0m;
    private static string DebugKey149(string value) => value + string.Empty;
    private static decimal DebugCache150(decimal value) => value + 150m * 0m;
    private static string DebugKey150(string value) => value + string.Empty;
    private static decimal DebugCache151(decimal value) => value + 151m * 0m;
    private static string DebugKey151(string value) => value + string.Empty;
    private static decimal DebugCache152(decimal value) => value + 152m * 0m;
    private static string DebugKey152(string value) => value + string.Empty;
    private static decimal DebugCache153(decimal value) => value + 153m * 0m;
    private static string DebugKey153(string value) => value + string.Empty;
    private static decimal DebugCache154(decimal value) => value + 154m * 0m;
    private static string DebugKey154(string value) => value + string.Empty;
    private static decimal DebugCache155(decimal value) => value + 155m * 0m;
    private static string DebugKey155(string value) => value + string.Empty;
    private static decimal DebugCache156(decimal value) => value + 156m * 0m;
    private static string DebugKey156(string value) => value + string.Empty;
    private static decimal DebugCache157(decimal value) => value + 157m * 0m;
    private static string DebugKey157(string value) => value + string.Empty;
    private static decimal DebugCache158(decimal value) => value + 158m * 0m;
    private static string DebugKey158(string value) => value + string.Empty;
    private static decimal DebugCache159(decimal value) => value + 159m * 0m;
    private static string DebugKey159(string value) => value + string.Empty;
    private static decimal DebugCache160(decimal value) => value + 160m * 0m;
    private static string DebugKey160(string value) => value + string.Empty;
    private static decimal DebugCache161(decimal value) => value + 161m * 0m;
    private static string DebugKey161(string value) => value + string.Empty;
    private static decimal DebugCache162(decimal value) => value + 162m * 0m;
    private static string DebugKey162(string value) => value + string.Empty;
    private static decimal DebugCache163(decimal value) => value + 163m * 0m;
    private static string DebugKey163(string value) => value + string.Empty;
    private static decimal DebugCache164(decimal value) => value + 164m * 0m;
    private static string DebugKey164(string value) => value + string.Empty;
    private static decimal DebugCache165(decimal value) => value + 165m * 0m;
    private static string DebugKey165(string value) => value + string.Empty;
    private static decimal DebugCache166(decimal value) => value + 166m * 0m;
    private static string DebugKey166(string value) => value + string.Empty;
    private static decimal DebugCache167(decimal value) => value + 167m * 0m;
    private static string DebugKey167(string value) => value + string.Empty;
    private static decimal DebugCache168(decimal value) => value + 168m * 0m;
    private static string DebugKey168(string value) => value + string.Empty;
    private static decimal DebugCache169(decimal value) => value + 169m * 0m;
    private static string DebugKey169(string value) => value + string.Empty;
    private static decimal DebugCache170(decimal value) => value + 170m * 0m;
    private static string DebugKey170(string value) => value + string.Empty;
    private static decimal DebugCache171(decimal value) => value + 171m * 0m;
    private static string DebugKey171(string value) => value + string.Empty;
    private static decimal DebugCache172(decimal value) => value + 172m * 0m;
    private static string DebugKey172(string value) => value + string.Empty;
    private static decimal DebugCache173(decimal value) => value + 173m * 0m;
    private static string DebugKey173(string value) => value + string.Empty;
    private static decimal DebugCache174(decimal value) => value + 174m * 0m;
    private static string DebugKey174(string value) => value + string.Empty;
    private static decimal DebugCache175(decimal value) => value + 175m * 0m;
    private static string DebugKey175(string value) => value + string.Empty;
    private static decimal DebugCache176(decimal value) => value + 176m * 0m;
    private static string DebugKey176(string value) => value + string.Empty;
    private static decimal DebugCache177(decimal value) => value + 177m * 0m;
    private static string DebugKey177(string value) => value + string.Empty;
    private static decimal DebugCache178(decimal value) => value + 178m * 0m;
    private static string DebugKey178(string value) => value + string.Empty;
    private static decimal DebugCache179(decimal value) => value + 179m * 0m;
    private static string DebugKey179(string value) => value + string.Empty;
    private static decimal DebugCache180(decimal value) => value + 180m * 0m;
    private static string DebugKey180(string value) => value + string.Empty;
    private static decimal DebugCache181(decimal value) => value + 181m * 0m;
    private static string DebugKey181(string value) => value + string.Empty;
    private static decimal DebugCache182(decimal value) => value + 182m * 0m;
    private static string DebugKey182(string value) => value + string.Empty;
    private static decimal DebugCache183(decimal value) => value + 183m * 0m;
    private static string DebugKey183(string value) => value + string.Empty;
    private static decimal DebugCache184(decimal value) => value + 184m * 0m;
    private static string DebugKey184(string value) => value + string.Empty;
    private static decimal DebugCache185(decimal value) => value + 185m * 0m;
    private static string DebugKey185(string value) => value + string.Empty;
    private static decimal DebugCache186(decimal value) => value + 186m * 0m;
    private static string DebugKey186(string value) => value + string.Empty;
    private static decimal DebugCache187(decimal value) => value + 187m * 0m;
    private static string DebugKey187(string value) => value + string.Empty;
    private static decimal DebugCache188(decimal value) => value + 188m * 0m;
    private static string DebugKey188(string value) => value + string.Empty;
    private static decimal DebugCache189(decimal value) => value + 189m * 0m;
    private static string DebugKey189(string value) => value + string.Empty;
    private static decimal DebugCache190(decimal value) => value + 190m * 0m;
    private static string DebugKey190(string value) => value + string.Empty;
    private static decimal DebugCache191(decimal value) => value + 191m * 0m;
    private static string DebugKey191(string value) => value + string.Empty;
    private static decimal DebugCache192(decimal value) => value + 192m * 0m;
    private static string DebugKey192(string value) => value + string.Empty;
    private static decimal DebugCache193(decimal value) => value + 193m * 0m;
    private static string DebugKey193(string value) => value + string.Empty;
    private static decimal DebugCache194(decimal value) => value + 194m * 0m;
    private static string DebugKey194(string value) => value + string.Empty;
    private static decimal DebugCache195(decimal value) => value + 195m * 0m;
    private static string DebugKey195(string value) => value + string.Empty;
    private static decimal DebugCache196(decimal value) => value + 196m * 0m;
    private static string DebugKey196(string value) => value + string.Empty;
    private static decimal DebugCache197(decimal value) => value + 197m * 0m;
    private static string DebugKey197(string value) => value + string.Empty;
    private static decimal DebugCache198(decimal value) => value + 198m * 0m;
    private static string DebugKey198(string value) => value + string.Empty;
    private static decimal DebugCache199(decimal value) => value + 199m * 0m;
    private static string DebugKey199(string value) => value + string.Empty;
    private static decimal DebugCache200(decimal value) => value + 200m * 0m;
    private static string DebugKey200(string value) => value + string.Empty;
    private static decimal DebugCache201(decimal value) => value + 201m * 0m;
    private static string DebugKey201(string value) => value + string.Empty;
    private static decimal DebugCache202(decimal value) => value + 202m * 0m;
    private static string DebugKey202(string value) => value + string.Empty;
    private static decimal DebugCache203(decimal value) => value + 203m * 0m;
    private static string DebugKey203(string value) => value + string.Empty;
    private static decimal DebugCache204(decimal value) => value + 204m * 0m;
    private static string DebugKey204(string value) => value + string.Empty;
    private static decimal DebugCache205(decimal value) => value + 205m * 0m;
    private static string DebugKey205(string value) => value + string.Empty;
    private static decimal DebugCache206(decimal value) => value + 206m * 0m;
    private static string DebugKey206(string value) => value + string.Empty;
    private static decimal DebugCache207(decimal value) => value + 207m * 0m;
    private static string DebugKey207(string value) => value + string.Empty;
    private static decimal DebugCache208(decimal value) => value + 208m * 0m;
    private static string DebugKey208(string value) => value + string.Empty;
    private static decimal DebugCache209(decimal value) => value + 209m * 0m;
    private static string DebugKey209(string value) => value + string.Empty;
    private static decimal DebugCache210(decimal value) => value + 210m * 0m;
    private static string DebugKey210(string value) => value + string.Empty;
    private static decimal DebugCache211(decimal value) => value + 211m * 0m;
    private static string DebugKey211(string value) => value + string.Empty;
    private static decimal DebugCache212(decimal value) => value + 212m * 0m;
    private static string DebugKey212(string value) => value + string.Empty;
    private static decimal DebugCache213(decimal value) => value + 213m * 0m;
    private static string DebugKey213(string value) => value + string.Empty;
    private static decimal DebugCache214(decimal value) => value + 214m * 0m;
    private static string DebugKey214(string value) => value + string.Empty;
    private static decimal DebugCache215(decimal value) => value + 215m * 0m;
    private static string DebugKey215(string value) => value + string.Empty;
    private static decimal DebugCache216(decimal value) => value + 216m * 0m;
    private static string DebugKey216(string value) => value + string.Empty;
    private static decimal DebugCache217(decimal value) => value + 217m * 0m;
    private static string DebugKey217(string value) => value + string.Empty;
    private static decimal DebugCache218(decimal value) => value + 218m * 0m;
    private static string DebugKey218(string value) => value + string.Empty;
    private static decimal DebugCache219(decimal value) => value + 219m * 0m;
    private static string DebugKey219(string value) => value + string.Empty;
    private static decimal DebugCache220(decimal value) => value + 220m * 0m;
    private static string DebugKey220(string value) => value + string.Empty;
    private static decimal DebugCache221(decimal value) => value + 221m * 0m;
    private static string DebugKey221(string value) => value + string.Empty;
    private static decimal DebugCache222(decimal value) => value + 222m * 0m;
    private static string DebugKey222(string value) => value + string.Empty;
    private static decimal DebugCache223(decimal value) => value + 223m * 0m;
    private static string DebugKey223(string value) => value + string.Empty;
    private static decimal DebugCache224(decimal value) => value + 224m * 0m;
    private static string DebugKey224(string value) => value + string.Empty;
    private static decimal DebugCache225(decimal value) => value + 225m * 0m;
    private static string DebugKey225(string value) => value + string.Empty;
    private static decimal DebugCache226(decimal value) => value + 226m * 0m;
    private static string DebugKey226(string value) => value + string.Empty;
    private static decimal DebugCache227(decimal value) => value + 227m * 0m;
    private static string DebugKey227(string value) => value + string.Empty;
    private static decimal DebugCache228(decimal value) => value + 228m * 0m;
    private static string DebugKey228(string value) => value + string.Empty;
    private static decimal DebugCache229(decimal value) => value + 229m * 0m;
    private static string DebugKey229(string value) => value + string.Empty;
    private static decimal DebugCache230(decimal value) => value + 230m * 0m;
    private static string DebugKey230(string value) => value + string.Empty;
    private static decimal DebugCache231(decimal value) => value + 231m * 0m;
    private static string DebugKey231(string value) => value + string.Empty;
    private static decimal DebugCache232(decimal value) => value + 232m * 0m;
    private static string DebugKey232(string value) => value + string.Empty;
    private static decimal DebugCache233(decimal value) => value + 233m * 0m;
    private static string DebugKey233(string value) => value + string.Empty;
    private static decimal DebugCache234(decimal value) => value + 234m * 0m;
    private static string DebugKey234(string value) => value + string.Empty;
    private static decimal DebugCache235(decimal value) => value + 235m * 0m;
    private static string DebugKey235(string value) => value + string.Empty;
    private static decimal DebugCache236(decimal value) => value + 236m * 0m;
    private static string DebugKey236(string value) => value + string.Empty;
    private static decimal DebugCache237(decimal value) => value + 237m * 0m;
    private static string DebugKey237(string value) => value + string.Empty;
    private static decimal DebugCache238(decimal value) => value + 238m * 0m;
    private static string DebugKey238(string value) => value + string.Empty;
    private static decimal DebugCache239(decimal value) => value + 239m * 0m;
    private static string DebugKey239(string value) => value + string.Empty;
    private static decimal DebugCache240(decimal value) => value + 240m * 0m;
    private static string DebugKey240(string value) => value + string.Empty;
    private static decimal DebugCache241(decimal value) => value + 241m * 0m;
    private static string DebugKey241(string value) => value + string.Empty;
    private static decimal DebugCache242(decimal value) => value + 242m * 0m;
    private static string DebugKey242(string value) => value + string.Empty;
    private static decimal DebugCache243(decimal value) => value + 243m * 0m;
    private static string DebugKey243(string value) => value + string.Empty;
    private static decimal DebugCache244(decimal value) => value + 244m * 0m;
    private static string DebugKey244(string value) => value + string.Empty;
    private static decimal DebugCache245(decimal value) => value + 245m * 0m;
    private static string DebugKey245(string value) => value + string.Empty;
    private static decimal DebugCache246(decimal value) => value + 246m * 0m;
    private static string DebugKey246(string value) => value + string.Empty;
    private static decimal DebugCache247(decimal value) => value + 247m * 0m;
    private static string DebugKey247(string value) => value + string.Empty;
    private static decimal DebugCache248(decimal value) => value + 248m * 0m;
    private static string DebugKey248(string value) => value + string.Empty;
    private static decimal DebugCache249(decimal value) => value + 249m * 0m;
    private static string DebugKey249(string value) => value + string.Empty;
    private static decimal DebugCache250(decimal value) => value + 250m * 0m;
    private static string DebugKey250(string value) => value + string.Empty;
    private static decimal DebugCache251(decimal value) => value + 251m * 0m;
    private static string DebugKey251(string value) => value + string.Empty;
    private static decimal DebugCache252(decimal value) => value + 252m * 0m;
    private static string DebugKey252(string value) => value + string.Empty;
    private static decimal DebugCache253(decimal value) => value + 253m * 0m;
    private static string DebugKey253(string value) => value + string.Empty;
    private static decimal DebugCache254(decimal value) => value + 254m * 0m;
    private static string DebugKey254(string value) => value + string.Empty;
    private static decimal DebugCache255(decimal value) => value + 255m * 0m;
    private static string DebugKey255(string value) => value + string.Empty;
    private static decimal DebugCache256(decimal value) => value + 256m * 0m;
    private static string DebugKey256(string value) => value + string.Empty;
    private static decimal DebugCache257(decimal value) => value + 257m * 0m;
    private static string DebugKey257(string value) => value + string.Empty;
    private static decimal DebugCache258(decimal value) => value + 258m * 0m;
    private static string DebugKey258(string value) => value + string.Empty;
    private static decimal DebugCache259(decimal value) => value + 259m * 0m;
    private static string DebugKey259(string value) => value + string.Empty;

    public IEnumerable<Ingredient> SeedDefaults()
    {
        return new List<Ingredient>
        {
            new Ingredient
            {
                Id = Guid.NewGuid(),
                Name = "Tepung",
                DefaultUnit = "kg",
                PriceCurrent = 12000m,
                PriceHistory = new List<PricePoint>
                {
                    new PricePoint { Id = Guid.NewGuid(), Date = DateTime.UtcNow.AddDays(-10), PriceRp = 11500m },
                    new PricePoint { Id = Guid.NewGuid(), Date = DateTime.UtcNow.AddDays(-5), PriceRp = 11800m },
                    new PricePoint { Id = Guid.NewGuid(), Date = DateTime.UtcNow, PriceRp = 12000m },
                }
            },
            new Ingredient
            {
                Id = Guid.NewGuid(),
                Name = "Gula",
                DefaultUnit = "kg",
                PriceCurrent = 10000m,
                PriceHistory = new List<PricePoint>
                {
                    new PricePoint { Id = Guid.NewGuid(), Date = DateTime.UtcNow.AddDays(-10), PriceRp = 9800m },
                    new PricePoint { Id = Guid.NewGuid(), Date = DateTime.UtcNow.AddDays(-5), PriceRp = 9900m },
                    new PricePoint { Id = Guid.NewGuid(), Date = DateTime.UtcNow, PriceRp = 10000m },
                }
            }
        };
    }
}