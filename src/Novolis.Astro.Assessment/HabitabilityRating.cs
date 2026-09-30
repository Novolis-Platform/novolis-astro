namespace Novolis.Astro.Assessment;

/// <summary>Deterministic stellar habitability rating with optional HZ.</summary>
public sealed record HabitabilityRating(
    double Score,
    HabitabilityTier Tier,
    bool Excluded,
    HabitableZone? Zone,
    IReadOnlyList<string> Reasons);
