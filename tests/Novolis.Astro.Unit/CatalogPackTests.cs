using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class CatalogPackTests
{
    [Test]
    public async Task NearSol100_Has_Sol_And_Count()
    {
        await Assert.That(CatalogPacks.NearSol100.Count).IsEqualTo(100);
        var catalog = CatalogPacks.ToCatalog(CatalogPacks.NearSol100);
        await Assert.That(catalog.TryGet("sol", out _)).IsTrue();
        await Assert.That(catalog.All[0].Id.Value).IsEqualTo("sol");
    }

    [Test]
    public async Task HygLocal1901_Count_And_Order_Stable()
    {
        await Assert.That(CatalogPacks.HygLocal1901.Count).IsEqualTo(1901);
        var catalog = StarCatalog.From(CatalogPacks.HygLocal1901);
        await Assert.That(catalog.Count).IsEqualTo(1901);
        await Assert.That(catalog.All[0].Id.Value).IsEqualTo("0");
        await Assert.That(catalog.All[0].Name).IsEqualTo("Sol");

        var again = StarCatalog.From(CatalogPacks.HygLocal1901);
        for (var i = 0; i < 10; i++)
            await Assert.That(again.All[i].Id.Value).IsEqualTo(catalog.All[i].Id.Value);
    }

    [Test]
    public async Task NearSol_Pack_Builds_RouteGraph()
    {
        var cost = RangeBandCostModel.CreatePrototypeCompatible();
        var graph = RouteGraph.Build(CatalogPacks.NearSol100, maxRangeLy: 12, cost);
        await Assert.That(graph.Adjacency.ContainsKey("sol")).IsTrue();
        await Assert.That(graph.Adjacency["sol"].Count).IsGreaterThan(0);
    }

    [Test]
    public async Task From_Rejects_Duplicate_Ids()
    {
        var a = new StarSystem("a", "A", new StarCoords(0, 0, 0));
        var dup = new StarSystem("a", "A2", new StarCoords(1, 0, 0));
        var act = () => StarCatalog.From([a, dup]);
        await Assert.That(act).Throws<ArgumentException>();
    }
}
