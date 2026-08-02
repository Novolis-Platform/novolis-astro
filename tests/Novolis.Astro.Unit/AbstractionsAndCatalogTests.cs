using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Overlay;

namespace Novolis.Astro.Unit;

public sealed class AbstractionsTests
{
    [Test]
    public async Task SystemId_ToString_And_Implicit_Conversions()
    {
        SystemId id = "alpha-centauri";
        await Assert.That(id.ToString()).IsEqualTo("alpha-centauri");

        string asString = id;
        await Assert.That(asString).IsEqualTo("alpha-centauri");

        SystemId roundTrip = asString;
        await Assert.That(roundTrip.Value).IsEqualTo("alpha-centauri");
    }

    [Test]
    public async Task StarCoords_DistanceFromOrigin_And_Distance()
    {
        var origin = new StarCoords(0, 0, 0);
        var point = new StarCoords(3, 4, 0);

        await Assert.That(origin.DistanceFromOrigin).IsEqualTo(0.0);
        await Assert.That(Math.Abs(point.DistanceFromOrigin - 5.0)).IsLessThan(1e-9);
        await Assert.That(Math.Abs(StarCoords.Distance(origin, point) - 5.0)).IsLessThan(1e-9);
        await Assert.That(Math.Abs(StarCoords.Distance(point, point))).IsLessThan(1e-9);
    }
}

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

public sealed class HygCsvImporterTests
{
    const string SampleCsv =
        """
        id,name,x,y,z,spect,lum,absmag,teff
        sol,Sol,0,0,0,G2V,1,4.83,5780
        wd,WD,1,0,0,DA,,,
        skip,,0,0,0,,,,
        bad,Bad,not-a-number,0,0,,,,
        """;

    [Test]
    public async Task Import_Parses_Valid_Rows()
    {
        var catalog = new StarCatalog();
        var count = HygCsvImporter.Import(SampleCsv, catalog);

        await Assert.That(count).IsEqualTo(3);
        await Assert.That(catalog.TryGet("sol", out var sol)).IsTrue();
        await Assert.That(sol!.SpectralClass).IsEqualTo(SpectralClass.G);
        await Assert.That(sol.LuminositySolar).IsEqualTo(1.0);
        await Assert.That(catalog.TryGet("wd", out var wd)).IsTrue();
        await Assert.That(wd!.SpectralClass).IsEqualTo(SpectralClass.WD);
    }

    [Test]
    public async Task Enumerate_Missing_Columns_Throws()
    {
        var reader = new StringReader("name,x,y\na,1,2,3");
        var act = () => HygCsvImporter.Enumerate(reader).ToList();
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Import_NullArguments_Throw()
    {
        var catalog = new StarCatalog();
        await Assert.That(() => HygCsvImporter.Import(null!, catalog)).Throws<ArgumentNullException>();
        await Assert.That(() => HygCsvImporter.Import("id,x,y,z\n", null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Enumerate_Empty_Reader_Yields_Nothing()
    {
        var reader = new StringReader("");
        await Assert.That(HygCsvImporter.Enumerate(reader).ToList()).IsEmpty();
    }
}

public sealed class CatalogOverlayExtendedTests
{
    [Test]
    public async Task Bind_Empty_Alias_Throws()
    {
        var overlay = new CatalogOverlay();
        var act = () => overlay.Bind(new OverlayEntry("  ", "sol"));
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Entries_Reflects_Bindings()
    {
        var overlay = new CatalogOverlay();
        overlay.Bind(new OverlayEntry("Home", "a"));
        overlay.Bind(new OverlayEntry("Away", "b"));

        await Assert.That(overlay.Entries.Count).IsEqualTo(2);
        await Assert.That(overlay.TryResolve("Away", out var id)).IsTrue();
        await Assert.That(id.Value).IsEqualTo("b");
    }

    [Test]
    public async Task Validate_NullCatalog_Throws()
    {
        var overlay = new CatalogOverlay();
        await Assert.That(() => overlay.Validate(null!)).Throws<ArgumentNullException>();
    }
}
