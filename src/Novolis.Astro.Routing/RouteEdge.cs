using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Routing;

/// <summary>Directed edge in a route graph.</summary>
public sealed record RouteEdge(
    SystemId From,
    SystemId To,
    double DistanceLy,
    double Cost,
    string? BandTag);
