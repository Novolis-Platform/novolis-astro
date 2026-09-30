namespace Novolis.Astro.Assessment;

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
