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
