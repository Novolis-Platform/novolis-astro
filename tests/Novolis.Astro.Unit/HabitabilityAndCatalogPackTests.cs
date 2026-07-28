using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class HabitabilityTests
{
    static StarSystem Sol() => new(
        "sol",
        "Sol",
        new StarCoords(0, 0, 0),
        SpectralClass.G,
        spectralDesignation: "G2V",
        luminositySolar: 1.0,
        effectiveTemperatureK: 5780.0,
        absoluteMagnitude: 4.83);

    [Test]
    public async Task Sol_Conservative_Hz_Matches_Kopparapu()
    {
        var zone = Sol().EstimateHabitableZone(HabitableZoneConvention.Conservative);
        await Assert.That(zone).IsNotNull();
        await Assert.That(Math.Abs(zone!.InnerAu - 0.97)).IsLessThan(0.05);
        await Assert.That(Math.Abs(zone.OuterAu - 1.67)).IsLessThan(0.05);
        await Assert.That(zone.InnerLimit).IsEqualTo(HabitableZoneLimit.RunawayGreenhouse);
        await Assert.That(zone.OuterLimit).IsEqualTo(HabitableZoneLimit.MaximumGreenhouse);
    }

    [Test]
    public async Task Sol_Optimistic_Hz_Is_Wider()
    {
        var zone = Sol().EstimateHabitableZone(HabitableZoneConvention.Optimistic);
        await Assert.That(zone).IsNotNull();
        await Assert.That(Math.Abs(zone!.InnerAu - 0.75)).IsLessThan(0.05);
        await Assert.That(Math.Abs(zone.OuterAu - 1.77)).IsLessThan(0.05);
        await Assert.That(zone.WidthAu).IsGreaterThan(
            Sol().EstimateHabitableZone(HabitableZoneConvention.Conservative)!.WidthAu);
    }

    [Test]
    public async Task Habitability_Is_Deterministic()
    {
        var a = Sol().AssessHabitability();
        var b = Sol().AssessHabitability();
        await Assert.That(a.Score).IsEqualTo(b.Score);
        await Assert.That(a.Tier).IsEqualTo(b.Tier);
        await Assert.That(a.Zone!.InnerAu).IsEqualTo(b.Zone!.InnerAu);
    }

    [Test]
    public async Task Sol_Rates_As_Prime()
    {
        var rating = Sol().AssessHabitability();
        await Assert.That(rating.Excluded).IsFalse();
        await Assert.That(rating.Tier).IsEqualTo(HabitabilityTier.Prime);
        await Assert.That(rating.Score).IsGreaterThanOrEqualTo(85.0);
    }

    [Test]
    public async Task WhiteDwarf_Is_Excluded()
    {
        var system = new StarSystem("wd", "WD", new StarCoords(1, 0, 0), SpectralClass.WD, spectralDesignation: "DA");
        var rating = system.AssessHabitability();
        await Assert.That(rating.Excluded).IsTrue();
        await Assert.That(rating.Tier).IsEqualTo(HabitabilityTier.Excluded);
    }

    [Test]
    public async Task OStar_Is_Excluded()
    {
        var system = new StarSystem(
            "o",
            "O",
            new StarCoords(1, 0, 0),
            SpectralClass.O,
            spectralDesignation: "O5V",
            luminositySolar: 1e5,
            effectiveTemperatureK: 40000);
        var rating = system.AssessHabitability();
        await Assert.That(rating.Excluded).IsTrue();
        await Assert.That(rating.Tier).IsEqualTo(HabitabilityTier.Excluded);
    }

    [Test]
    public async Task Giant_Is_Excluded()
    {
        var system = new StarSystem(
            "giant",
            "Giant",
            new StarCoords(1, 0, 0),
            SpectralClass.K,
            spectralDesignation: "K2III",
            luminositySolar: 50,
            effectiveTemperatureK: 4500);
        var rating = system.AssessHabitability();
        await Assert.That(rating.Excluded).IsTrue();
    }

    [Test]
    public async Task HabitabilityAssessor_Maps_Extension()
    {
        var score = new HabitabilityAssessor().Assess(Sol());
        await Assert.That(score.Tier).IsEqualTo(nameof(HabitabilityTier.Prime));
        await Assert.That(score.Score).IsGreaterThanOrEqualTo(85.0);
    }
}

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
