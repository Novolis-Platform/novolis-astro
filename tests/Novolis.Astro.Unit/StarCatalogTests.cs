using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Overlay;

namespace Novolis.Astro.Unit;

public sealed class StarCatalogTests
{
    static StarSystem Sys(string id, double x) =>
        new(id, id.ToUpperInvariant(), new StarCoords(x, 0, 0), SpectralClass.G);

    [Test]
    public async Task Add_Replace_Keeps_Slot()
    {
        var catalog = new StarCatalog();
        catalog.Add(Sys("a", 0));
        catalog.Add(Sys("b", 5));
        catalog.Add(Sys("a", 1));

        await Assert.That(catalog.Count).IsEqualTo(2);
        await Assert.That(catalog.All[0].Coords.X).IsEqualTo(1.0);
        await Assert.That(catalog.GetRequired("a").Coords.X).IsEqualTo(1.0);
    }

    [Test]
    public async Task GetRequired_Throws_For_Unknown()
    {
        var catalog = StarCatalog.From([Sys("a", 0)]);
        var act = () => catalog.GetRequired("missing");
        await Assert.That(act).Throws<KeyNotFoundException>();
    }

    [Test]
    public async Task NeighborsWithin_Sorts_By_Distance_And_Excludes()
    {
        var catalog = StarCatalog.From([
            Sys("origin", 0),
            Sys("near", 2),
            Sys("far", 20)
        ]);

        var neighbors = catalog.NeighborsWithin(new StarCoords(0, 0, 0), 10, excludeId: "origin");
        await Assert.That(neighbors.Count).IsEqualTo(1);
        await Assert.That(neighbors[0].System.Id.Value).IsEqualTo("near");
        await Assert.That(neighbors[0].DistanceLy).IsEqualTo(2.0);
    }

    [Test]
    public async Task From_NullSystems_Throws()
    {
        await Assert.That(() => StarCatalog.From(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Add_NullSystem_Throws()
    {
        var catalog = new StarCatalog();
        await Assert.That(() => catalog.Add(null!)).Throws<ArgumentNullException>();
    }
}
