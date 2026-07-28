using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Overlay;
using Novolis.Astro.Plotting;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class RoutingTests
{
    static StarCatalog CreateSyntheticCatalog()
    {
        var catalog = new StarCatalog();
        catalog.Add(new StarSystem("a", "A", new StarCoords(0, 0, 0), SpectralClass.G));
        catalog.Add(new StarSystem("b", "B", new StarCoords(9, 0, 0), SpectralClass.K));
        catalog.Add(new StarSystem("c", "C", new StarCoords(18, 0, 0), SpectralClass.M));
        catalog.Add(new StarSystem("d", "D", new StarCoords(11, 0, 0), SpectralClass.F));
        return catalog;
    }

    [Test]
    public async Task Find_Route_Across_Chain()
    {
        var catalog = CreateSyntheticCatalog();
        var cost = RangeBandCostModel.CreatePrototypeCompatible();
        var graph = RouteGraph.Build(catalog.All, maxRangeLy: 12, cost);
        var route = RoutePlanner.Find("a", "c", graph, new ConstantSpeedTransitProfile(1.0));

        await Assert.That(route.Found).IsTrue();
        await Assert.That(route.WaypointIds.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(route.Accumulation.TotalLy).IsGreaterThan(0);
        await Assert.That(route.Accumulation.TotalDurationSeconds).IsGreaterThan(0);
    }

    [Test]
    public async Task PrototypeCompatible_Bands_Match_Expected_Costs()
    {
        var model = RangeBandCostModel.CreatePrototypeCompatible();
        var shortHop = model.Evaluate("a", "b", 9.0);
        var longHop = model.Evaluate("a", "b", 11.0);
        var tooFar = model.Evaluate("a", "b", 13.0);

        await Assert.That(shortHop.Feasible).IsTrue();
        await Assert.That(shortHop.BandTag).IsEqualTo("short");
        await Assert.That(Math.Abs(shortHop.Cost - 9.0)).IsLessThan(1e-9);

        await Assert.That(longHop.Feasible).IsTrue();
        await Assert.That(longHop.BandTag).IsEqualTo("long");
        await Assert.That(Math.Abs(longHop.Cost - 33.0)).IsLessThan(1e-9);

        await Assert.That(tooFar.Feasible).IsFalse();
    }

    [Test]
    public async Task Accumulation_Counts_Bands()
    {
        var accumulation = new RouteAccumulation();
        accumulation.AddHop(9, 9, 100, "short");
        accumulation.AddHop(11, 33, 200, "long");

        await Assert.That(accumulation.TotalLy).IsEqualTo(20.0);
        await Assert.That(accumulation.TotalCost).IsEqualTo(42.0);
        await Assert.That(accumulation.CountsByBand["short"]).IsEqualTo(1);
        await Assert.That(accumulation.CountsByBand["long"]).IsEqualTo(1);
    }

    [Test]
    public async Task Overlay_Resolves_And_Validates()
    {
        var catalog = CreateSyntheticCatalog();
        var overlay = new CatalogOverlay();
        overlay.Bind(new OverlayEntry("Home", "a", new Dictionary<string, string> { ["role"] = "origin" }));
        await Assert.That(overlay.TryResolve("Home", out var id)).IsTrue();
        await Assert.That(id.Value).IsEqualTo("a");
        await Assert.That(overlay.Validate(catalog).Count).IsEqualTo(0);

        overlay.Bind(new OverlayEntry("Missing", "nope"));
        await Assert.That(overlay.Validate(catalog).Count).IsEqualTo(1);
    }

    [Test]
    public async Task Plotting_Produces_Svg()
    {
        var coords = new[]
        {
            new StarCoords(0, 0, 0),
            new StarCoords(5, 0, 2),
            new StarCoords(10, 0, -1)
        };
        var svg = PathSvgExporter.Export(coords);
        await Assert.That(svg.Contains("polyline", StringComparison.Ordinal)).IsTrue();
        await Assert.That(PathTsvExporter.Export(coords).Contains("x_ly")).IsTrue();
    }

    [Test]
    public async Task HabitabilityAssessor_Scores_G()
    {
        var system = new StarSystem("sol", "Sol", new StarCoords(0, 0, 0), SpectralClass.G);
        var score = new HabitabilityAssessor().Assess(system);
        await Assert.That(score.Score).IsEqualTo(90.0);
        await Assert.That(score.Tier).IsEqualTo("prime");
    }
}
