namespace Novolis.Astro.Routing;

/// <summary>Accumulated totals along a planned route.</summary>
public sealed class RouteAccumulation
{
    readonly Dictionary<string, int> _counts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Total geometric distance in light-years.</summary>
    public double TotalLy { get; private set; }

    /// <summary>Total pathfinding cost.</summary>
    public double TotalCost { get; private set; }

    /// <summary>Total transit duration in seconds (0 if no transit profile).</summary>
    public double TotalDurationSeconds { get; private set; }

    /// <summary>Hop counts keyed by band tag (unknown tags use "unspecified").</summary>
    public IReadOnlyDictionary<string, int> CountsByBand => _counts;

    /// <summary>Adds one hop into the accumulation.</summary>
    public void AddHop(double distanceLy, double cost, double durationSeconds, string? bandTag)
    {
        TotalLy += distanceLy;
        TotalCost += cost;
        TotalDurationSeconds += durationSeconds;
        var key = string.IsNullOrWhiteSpace(bandTag) ? "unspecified" : bandTag;
        _counts[key] = _counts.TryGetValue(key, out var n) ? n + 1 : 1;
    }
}

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
