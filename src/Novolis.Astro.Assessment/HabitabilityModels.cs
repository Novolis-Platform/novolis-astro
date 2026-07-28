namespace Novolis.Astro.Assessment;

/// <summary>Kopparapu et al. (2013) habitable-zone flux limits.</summary>
public enum HabitableZoneLimit
{
    /// <summary>Optimistic inner edge (empirical).</summary>
    RecentVenus = 0,
    /// <summary>Conservative inner edge (1D climate model).</summary>
    RunawayGreenhouse,
    /// <summary>Moist greenhouse (water-loss) inner edge.</summary>
    MoistGreenhouse,
    /// <summary>Conservative outer edge.</summary>
    MaximumGreenhouse,
    /// <summary>Optimistic outer edge (empirical).</summary>
    EarlyMars
}

/// <summary>Which pair of Kopparapu limits defines the reported zone.</summary>
public enum HabitableZoneConvention
{
    /// <summary>Runaway greenhouse → maximum greenhouse.</summary>
    Conservative = 0,
    /// <summary>Recent Venus → early Mars.</summary>
    Optimistic
}

/// <summary>Science-oriented habitability tier for a stellar primary.</summary>
public enum HabitabilityTier
{
    /// <summary>Not assessable as an Earth-analog host (degenerate, giant, too hot, …).</summary>
    Excluded = 0,
    /// <summary>Very poor host prospects.</summary>
    Hostile,
    /// <summary>Severe challenges (e.g. late M, narrow/close-in HZ).</summary>
    Marginal,
    /// <summary>Plausible but constrained.</summary>
    Candidate,
    /// <summary>Good main-sequence host with a usable HZ.</summary>
    Favorable,
    /// <summary>Best G/K-like hosts with Earth-like HZ geometry.</summary>
    Prime
}

/// <summary>Estimated circumstellar habitable zone in astronomical units.</summary>
public sealed record HabitableZone(
    double InnerAu,
    double OuterAu,
    double TeffK,
    double LuminositySolar,
    HabitableZoneConvention Convention,
    HabitableZoneLimit InnerLimit,
    HabitableZoneLimit OuterLimit)
{
    /// <summary>Outer − inner (AU).</summary>
    public double WidthAu => OuterAu - InnerAu;

    /// <summary>Midpoint of the zone (AU).</summary>
    public double MidAu => (InnerAu + OuterAu) * 0.5;
}

/// <summary>Deterministic stellar habitability rating with optional HZ.</summary>
public sealed record HabitabilityRating(
    double Score,
    HabitabilityTier Tier,
    bool Excluded,
    HabitableZone? Zone,
    IReadOnlyList<string> Reasons);
