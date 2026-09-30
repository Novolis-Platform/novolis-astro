namespace Novolis.Astro.Abstractions;

/// <summary>Pluggable hop cost / feasibility model for graph construction and routing.</summary>
public interface IHopCostModel
{
    /// <summary>Evaluate a hop from <paramref name="from"/> to <paramref name="to"/>.</summary>
    HopEvaluation Evaluate(SystemId from, SystemId to, double distanceLy);
}
