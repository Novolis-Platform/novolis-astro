namespace Novolis.Astro.Routing;

/// <summary>Result of a route search.</summary>
public sealed class RouteResult
{
    /// <summary>Creates a route result.</summary>
    public RouteResult(IReadOnlyList<string> waypointIds, bool found, RouteAccumulation accumulation)
    {
        WaypointIds = waypointIds;
        Found = found;
        Accumulation = accumulation;
    }

    /// <summary>Ordered system ids from origin to destination.</summary>
    public IReadOnlyList<string> WaypointIds { get; }

    /// <summary>Whether a path was found.</summary>
    public bool Found { get; }

    /// <summary>Totals along the path.</summary>
    public RouteAccumulation Accumulation { get; }
}
