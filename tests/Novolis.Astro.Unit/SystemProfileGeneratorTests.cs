using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;

namespace Novolis.Astro.Unit;

public sealed class SystemProfileGeneratorTests
{
    private const ulong CampaignSeed = 1001;

    static StarSystem Sol() => new(
        "sol",
        "Sol",
        new StarCoords(0, 0, 0),
        SpectralClass.G,
        spectralDesignation: "G2V",
        luminositySolar: 1.0,
        effectiveTemperatureK: 5780.0,
        absoluteMagnitude: 4.83);

    static StarSystem WhiteDwarf() => new(
        "wd",
        "WD",
        new StarCoords(1, 0, 0),
        SpectralClass.WD,
        spectralDesignation: "DA");

    static StarSystem OStar() => new(
        "o",
        "O",
        new StarCoords(2, 0, 0),
        SpectralClass.O,
        spectralDesignation: "O5V",
        luminositySolar: 1e5,
        effectiveTemperatureK: 40000);

    [Test]
    public async Task Generate_Is_Deterministic_For_Same_Seed()
    {
        var gen = new SystemProfileGenerator();
        var a = gen.Generate(Sol(), CampaignSeed);
        var b = gen.Generate(Sol(), CampaignSeed);

        await Assert.That(a.EffectiveSeed).IsEqualTo(b.EffectiveSeed);
        await Assert.That(a.Potential).IsEqualTo(b.Potential);
        await Assert.That(a.Elements.Count).IsEqualTo(b.Elements.Count);
        for (var i = 0; i < a.Elements.Count; i++)
        {
            await Assert.That(a.Elements[i]).IsEqualTo(b.Elements[i]);
        }

        await Assert.That(a.Habitability.Tier).IsEqualTo(b.Habitability.Tier);
        await Assert.That(a.Habitability.Score).IsEqualTo(b.Habitability.Score);
    }

    [Test]
    public async Task Generate_Differs_Across_Campaign_Seeds()
    {
        var gen = new SystemProfileGenerator();
        var a = gen.Generate(Sol(), CampaignSeed);
        var b = gen.Generate(Sol(), CampaignSeed + 1);

        await Assert.That(a.EffectiveSeed).IsNotEqualTo(b.EffectiveSeed);
        var sameElements = a.Elements.Count == b.Elements.Count
            && a.Elements.Zip(b.Elements).All(p => p.First.Equals(p.Second));
        var samePotential = a.Potential.Equals(b.Potential);
        await Assert.That(sameElements && samePotential).IsFalse();
    }

    [Test]
    public async Task Excluded_And_Hostile_Have_Zero_Agriculture()
    {
        var gen = new SystemProfileGenerator();
        var wd = gen.Generate(WhiteDwarf(), CampaignSeed);
        var o = gen.Generate(OStar(), CampaignSeed);

        await Assert.That(wd.Habitability.Tier).IsEqualTo(HabitabilityTier.Excluded);
        await Assert.That(wd.Potential.Agriculture).IsEqualTo(0.0);
        await Assert.That(o.Habitability.Tier).IsEqualTo(HabitabilityTier.Excluded);
        await Assert.That(o.Potential.Agriculture).IsEqualTo(0.0);
    }

    [Test]
    public async Task Sol_Has_NonZero_Agriculture()
    {
        var profile = new SystemProfileGenerator().Generate(Sol(), CampaignSeed);
        await Assert.That(profile.Habitability.Tier).IsEqualTo(HabitabilityTier.Prime);
        await Assert.That(profile.Potential.Agriculture).IsGreaterThan(0.0);
        await Assert.That(profile.Elements.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task NearSol100_Smoke_Counts_Are_Stable()
    {
        var gen = new SystemProfileGenerator();
        var catalog = CatalogPacks.ToCatalog(CatalogPacks.NearSol100);
        var profiles = catalog.All
            .Select(s => gen.Generate(s, CampaignSeed))
            .ToList();

        await Assert.That(profiles.Count).IsEqualTo(100);

        var agriCapable = profiles.Count(p => p.Potential.Agriculture > 0.3);
        var miningCapable = profiles.Count(p => p.Potential.Mining > 0.3);
        var zeroAgri = profiles.Count(p => p.Potential.Agriculture == 0.0);

        // Snapshot: regeneration with CampaignSeed 1001 must not drift silently.
        await Assert.That(agriCapable).IsEqualTo(19);
        await Assert.That(miningCapable).IsEqualTo(55);
        await Assert.That(zeroAgri).IsEqualTo(52);

        foreach (var p in profiles.Where(p =>
                     p.Habitability.Tier is HabitabilityTier.Excluded or HabitabilityTier.Hostile))
        {
            await Assert.That(p.Potential.Agriculture).IsEqualTo(0.0);
        }
    }
}
