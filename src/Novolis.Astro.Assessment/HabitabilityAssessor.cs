using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

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
