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

/// <summary>Stock habitability-like scorer from spectral class heuristics.</summary>
public sealed class HabitabilityAssessor : ISystemAssessor
{
    /// <inheritdoc />
    public string Facet => "habitability";

    /// <inheritdoc />
    public AssessmentScore Assess(StarSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        var (score, tier, reason) = system.SpectralClass switch
        {
            SpectralClass.G => (90.0, "prime", "G-class primary"),
            SpectralClass.K => (80.0, "open", "K-class primary"),
            SpectralClass.F => (70.0, "open", "F-class primary"),
            SpectralClass.M => (40.0, "marginal", "M-class primary"),
            SpectralClass.A or SpectralClass.B or SpectralClass.O => (30.0, "hostile", "hot primary"),
            _ => (25.0, "excluded", "unsupported spectral class")
        };
        return new AssessmentScore(score, tier, [reason]);
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
