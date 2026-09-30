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
