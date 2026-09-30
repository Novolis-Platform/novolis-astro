using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Routing;

/// <summary>A distance band with per-ly cost multiplier and tag.</summary>
/// <param name="MaxLy">Inclusive maximum hop distance for this band.</param>
/// <param name="CostPerLy">Pathfinding cost per light-year within the band.</param>
/// <param name="Tag">Band tag recorded on edges and accumulation.</param>
public sealed record RangeBand(double MaxLy, double CostPerLy, string Tag);
