namespace Novolis.Astro.Abstractions;

/// <summary>Result of evaluating a single hop under a cost model.</summary>
/// <param name="Feasible">Whether the hop is allowed.</param>
/// <param name="Cost">Pathfinding cost (not necessarily distance or time).</param>
/// <param name="DistanceLy">Geometric distance in light-years.</param>
/// <param name="BandTag">Optional band/class tag (e.g. short-range, long-range).</param>
public readonly record struct HopEvaluation(
    bool Feasible,
    double Cost,
    double DistanceLy,
    string? BandTag);

/// <summary>Result of evaluating transit timing/resources for a hop.</summary>
/// <param name="DurationSeconds">Travel duration in seconds.</param>
/// <param name="ResourceDelta">Signed resource change (fuel, stress, …); sign is consumer-defined.</param>
public readonly record struct TransitEvaluation(double DurationSeconds, double ResourceDelta);

/// <summary>Pluggable hop cost / feasibility model for graph construction and routing.</summary>
public interface IHopCostModel
{
    /// <summary>Evaluate a hop from <paramref name="from"/> to <paramref name="to"/>.</summary>
    HopEvaluation Evaluate(SystemId from, SystemId to, double distanceLy);
}

/// <summary>Pluggable transit duration / resource profile (separate from pathfinding cost).</summary>
public interface ITransitProfile
{
    /// <summary>Evaluate transit properties for a hop.</summary>
    TransitEvaluation Evaluate(SystemId from, SystemId to, double distanceLy, string? bandTag);
}
