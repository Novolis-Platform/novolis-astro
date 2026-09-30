using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

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
