using System;
using TrendplusProdavnica.Domain.Catalog;
using Xunit;

namespace TrendplusProdavnica.Tests;

public class UnitTest1
{
    [Fact]
    public void Domain_SimpleCategory_CanConstruct()
    {
        var now = DateTimeOffset.UtcNow;
        var cat = new Category
        {
            Name = "Test",
            Slug = "test",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        Assert.Equal("Test", cat.Name);
        Assert.Equal("test", cat.Slug);
        Assert.True(cat.IsActive);
    }
}
