using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class RoutingEdgeCaseTests
{
    [Test]
    public async Task RoutePlanner_Same_Node_Returns_Trivial_Route()
    {
        var catalog = new StarCatalog();
        catalog.Add(new StarSystem("a", "A", new StarCoords(0, 0, 0), SpectralClass.G));
        var graph = RouteGraph.Build(catalog.All, maxRangeLy: 12, RangeBandCostModel.CreatePrototypeCompatible());

        var route = RoutePlanner.Find("a", "a", graph);
        await Assert.That(route.Found).IsTrue();
        await Assert.That(route.WaypointIds).IsEquivalentTo(["a"]);
    }

    [Test]
    public async Task RoutePlanner_Unknown_Endpoints_Return_NotFound()
    {
        var catalog = new StarCatalog();
        catalog.Add(new StarSystem("a", "A", new StarCoords(0, 0, 0), SpectralClass.G));
        var graph = RouteGraph.Build(catalog.All, maxRangeLy: 12, RangeBandCostModel.CreatePrototypeCompatible());

        var route = RoutePlanner.Find("a", "missing", graph);
        await Assert.That(route.Found).IsFalse();
    }

    [Test]
    public async Task ConstantSpeedTransitProfile_Invalid_Speed_Throws()
    {
        await Assert.That(() => new ConstantSpeedTransitProfile(0))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task RangeBandCostModel_Empty_Bands_Throws()
    {
        await Assert.That(() => new RangeBandCostModel([]))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task HabitabilityAssessor_NullSystem_Throws()
    {
        await Assert.That(() => new HabitabilityAssessor().Assess(null!))
            .Throws<ArgumentNullException>();
    }
}
