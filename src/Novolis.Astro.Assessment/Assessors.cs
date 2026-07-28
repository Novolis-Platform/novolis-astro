using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

/// <summary>A scored assessment facet with optional tier label and reasons.</summary>
public sealed record AssessmentScore(double Score, string Tier, IReadOnlyList<string> Reasons);

/// <summary>Assesses a catalog system for a consumer-defined facet.</summary>
public interface ISystemAssessor
{
    /// <summary>Facet name (e.g. habitability, strategic-value).</summary>
    string Facet { get; }

    /// <summary>Scores the system.</summary>
    AssessmentScore Assess(StarSystem system);
}

/// <summary>Stock habitability scorer (Kopparapu HZ + weighted stellar rating).</summary>
public sealed class HabitabilityAssessor : ISystemAssessor
{
    /// <summary>Creates an assessor using the given HZ convention.</summary>
    public HabitabilityAssessor(HabitableZoneConvention convention = HabitableZoneConvention.Conservative) =>
        Convention = convention;

    /// <summary>HZ convention used when scoring.</summary>
    public HabitableZoneConvention Convention { get; }

    /// <inheritdoc />
    public string Facet => "habitability";

    /// <inheritdoc />
    public AssessmentScore Assess(StarSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        var rating = system.AssessHabitability(Convention);
        return new AssessmentScore(
            rating.Score,
            rating.Tier.ToString(),
            rating.Reasons);
    }
}

/// <summary>Stock strategic-value scorer from distance and spectral class.</summary>
public sealed class StrategicValueAssessor : ISystemAssessor
{
    /// <inheritdoc />
    public string Facet => "strategic-value";

    /// <inheritdoc />
    public AssessmentScore Assess(StarSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        var dist = system.Coords.DistanceFromOrigin;
        var proximity = Math.Clamp(100.0 - dist * 2.0, 0, 100);
        var spectralBonus = system.SpectralClass is SpectralClass.G or SpectralClass.K or SpectralClass.F ? 10.0 : 0.0;
        var score = Math.Clamp(proximity + spectralBonus, 0, 100);
        var tier = score >= 80 ? "hub" : score >= 55 ? "node" : "fringe";
        return new AssessmentScore(score, tier,
        [
            $"distance {dist:0.##} ly",
            $"spectral {system.SpectralClass}"
        ]);
    }
}
