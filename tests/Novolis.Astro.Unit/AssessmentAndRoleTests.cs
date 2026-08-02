using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class StrategicValueAssessorTests
{
    static StarSystem Sol() => new(
        "sol",
        "Sol",
        new StarCoords(0, 0, 0),
        SpectralClass.G);

    static StarSystem RemoteM() => new(
        "remote",
        "Remote",
        new StarCoords(50, 0, 0),
        SpectralClass.M);

    [Test]
    public async Task Sol_Rates_As_Hub()
    {
        var score = new StrategicValueAssessor().Assess(Sol());
        await Assert.That(score.Tier).IsEqualTo("hub");
        await Assert.That(score.Score).IsGreaterThanOrEqualTo(80.0);
    }

    [Test]
    public async Task RemoteM_Rates_As_Fringe()
    {
        var score = new StrategicValueAssessor().Assess(RemoteM());
        await Assert.That(score.Tier).IsEqualTo("fringe");
    }

    [Test]
    public async Task Assess_NullSystem_Throws()
    {
        await Assert.That(() => new StrategicValueAssessor().Assess(null!))
            .Throws<ArgumentNullException>();
    }
}

public sealed class StellarParameterResolverTests
{
    [Test]
    public async Task ParseDesignation_Recognizes_Luminosity_Classes()
    {
        var (g2vSpec, g2vSub, g2vLum, _) = StellarParameterResolver.ParseDesignation("G2V");
        await Assert.That(g2vSpec).IsEqualTo(SpectralClass.G);
        await Assert.That(g2vSub).IsEqualTo(2);
        await Assert.That(g2vLum).IsEqualTo(StellarParameterResolver.LuminosityClass.MainSequence);

        var (_, _, brightGiant, _) = StellarParameterResolver.ParseDesignation("K5III");
        await Assert.That(brightGiant).IsEqualTo(StellarParameterResolver.LuminosityClass.BrightGiant);

        var (_, _, subgiant, _) = StellarParameterResolver.ParseDesignation("G2IV");
        await Assert.That(subgiant).IsEqualTo(StellarParameterResolver.LuminosityClass.Subgiant);

        var (_, _, supergiant, isWd) = StellarParameterResolver.ParseDesignation("K2Ia");
        await Assert.That(supergiant).IsEqualTo(StellarParameterResolver.LuminosityClass.Supergiant);
        await Assert.That(isWd).IsFalse();
    }

    [Test]
    public async Task Resolve_Uses_AbsMag_When_Luminosity_Missing()
    {
        var system = new StarSystem(
            "a",
            "A",
            new StarCoords(1, 0, 0),
            SpectralClass.G,
            spectralDesignation: "G2V",
            absoluteMagnitude: 4.83);

        var resolved = StellarParameterResolver.Resolve(system);
        await Assert.That(resolved.LuminositySolar).IsNotNull();
        await Assert.That(resolved.LuminositySolar!.Value).IsGreaterThan(0);
        await Assert.That(resolved.HasSpectralDesignation).IsTrue();
    }

    [Test]
    public async Task EstimateTeffK_Covers_Brown_Dwarfs()
    {
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.L, null)).IsEqualTo(2000);
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.T, null)).IsEqualTo(1200);
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.Y, null)).IsEqualTo(600);
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.Unknown, null)).IsNull();
    }

    [Test]
    public async Task EstimateLuminositySolar_Scales_For_Giants()
    {
        var ms = StellarParameterResolver.EstimateLuminositySolar(
            SpectralClass.G, 2, StellarParameterResolver.LuminosityClass.MainSequence);
        var giant = StellarParameterResolver.EstimateLuminositySolar(
            SpectralClass.G, 2, StellarParameterResolver.LuminosityClass.Giant);

        await Assert.That(ms).IsNotNull();
        await Assert.That(giant).IsNotNull();
        await Assert.That(giant!.Value).IsGreaterThan(ms!.Value);
    }
}

public sealed class RoleAssignerTests
{
    private const ulong CampaignSeed = 1001;

    [Test]
    public async Task Assign_NearSol100_Produces_Expected_Census()
    {
        var catalog = CatalogPacks.ToCatalog(CatalogPacks.NearSol100);
        var graph = RouteGraph.Build(catalog.All, maxRangeLy: 12, RangeBandCostModel.CreatePrototypeCompatible());
        var gen = new SystemProfileGenerator();
        var profiles = catalog.All.ToDictionary(
            s => s.Id.Value,
            s => gen.Generate(s, CampaignSeed));

        var roles = RoleAssigner.Assign(catalog, graph, profiles);

        await Assert.That(roles["sol"]).IsEqualTo(SystemRole.Capital);
        await Assert.That(roles.Values.Count(r => r == SystemRole.Inhabited)).IsGreaterThan(0);

        var summary = RoleAssigner.Summarize(roles);
        await Assert.That(summary.StartsWith("C1", StringComparison.Ordinal)).IsTrue();

        var hubs = roles.Select(kvp =>
        {
            profiles.TryGetValue(kvp.Key, out var profile);
            return (kvp.Value, profile!.Potential);
        });
        var potentialSummary = RoleAssigner.SummarizePotentials(hubs);
        await Assert.That(potentialSummary).Contains("miningHubs=");
    }
}

public sealed class SystemRoleInvariantsTests
{
    [Test]
    public async Task CollectFailures_Flags_Mining_And_Settlement_Violations()
    {
        var failures = SystemRoleInvariants.CollectFailures([
            ("mine-low", SystemRole.Mining, new SystemEconomicPotential(0.1, 0, 0, 0)),
            ("settle-zero", SystemRole.Inhabited, new SystemEconomicPotential(0, 0, 0, 0.5)),
            ("ok-mine", SystemRole.Mining, new SystemEconomicPotential(0.5, 0, 0, 0)),
        ]);

        await Assert.That(failures.Count).IsEqualTo(2);
        await Assert.That(failures[0]).Contains("mine-low");
        await Assert.That(failures[1]).Contains("settle-zero");
    }
}

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
