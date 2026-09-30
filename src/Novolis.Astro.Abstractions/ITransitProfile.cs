namespace Novolis.Astro.Abstractions;

/// <summary>Pluggable transit duration / resource profile (separate from pathfinding cost).</summary>
public interface ITransitProfile
{
    /// <summary>Evaluate transit properties for a hop.</summary>
    TransitEvaluation Evaluate(SystemId from, SystemId to, double distanceLy, string? bandTag);
}
