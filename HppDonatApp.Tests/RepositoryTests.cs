using System;
using System.Threading.Tasks;
using HppDonatApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HppDonatApp.Tests;

public sealed class RepositoryTests
{
    [Fact]
    public async Task Can_Add_And_Read_Ingredient()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        await using var context = new AppDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        var repo = new IngredientRepository(context, new MemoryCache(new MemoryCacheOptions()), new NullLogger<IngredientRepository>());

        var ingredient = new Ingredient { Id = Guid.NewGuid(), Name = "Tepung", DefaultUnit = "kg", PriceCurrent = 12000m };
        await repo.AddAsync(ingredient);

        var loaded = await repo.GetByIdAsync(ingredient.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Tepung", loaded!.Name);
    }
}
