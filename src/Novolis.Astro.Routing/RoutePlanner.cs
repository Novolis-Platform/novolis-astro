using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Routing;

/// <summary>Dijkstra route planner over a <see cref="RouteGraph"/>.</summary>
public static class RoutePlanner
{
    /// <summary>Finds a minimum-cost route from <paramref name="fromId"/> to <paramref name="toId"/>.</summary>
    public static RouteResult Find(
        SystemId fromId,
        SystemId toId,
        RouteGraph graph,
        ITransitProfile? transitProfile = null)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var from = fromId.Value;
        var to = toId.Value;
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            return new RouteResult([from], true, new RouteAccumulation());

        if (!graph.Adjacency.ContainsKey(from) || !graph.Adjacency.ContainsKey(to))
            return new RouteResult([], false, new RouteAccumulation());

        var dist = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var prev = new Dictionary<string, RouteEdge?>(StringComparer.OrdinalIgnoreCase);
        var pq = new PriorityQueue<string, double>();

        foreach (var id in graph.Adjacency.Keys)
        {
            dist[id] = double.PositiveInfinity;
            prev[id] = null;
        }

        dist[from] = 0;
        pq.Enqueue(from, 0);

        while (pq.TryDequeue(out var u, out var priority))
        {
            if (priority > dist[u])
                continue;
            if (string.Equals(u, to, StringComparison.OrdinalIgnoreCase))
                break;

            if (!graph.Adjacency.TryGetValue(u, out var edges))
                continue;

            foreach (var edge in edges)
            {
                var v = edge.To.Value;
                var alt = dist[u] + edge.Cost;
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = edge;
                    pq.Enqueue(v, alt);
                }
            }
        }

        if (double.IsPositiveInfinity(dist[to]))
            return new RouteResult([], false, new RouteAccumulation());

        var edgesBack = new List<RouteEdge>();
        for (var cur = to; prev[cur] is { } edge; cur = edge.From.Value)
            edgesBack.Add(edge);
        edgesBack.Reverse();

        var waypoints = new List<string> { from };
        var accumulation = new RouteAccumulation();
        foreach (var edge in edgesBack)
        {
            waypoints.Add(edge.To.Value);
            var duration = 0.0;
            if (transitProfile is not null)
            {
                var te = transitProfile.Evaluate(edge.From, edge.To, edge.DistanceLy, edge.BandTag);
                duration = te.DurationSeconds;
            }

            accumulation.AddHop(edge.DistanceLy, edge.Cost, duration, edge.BandTag);
        }

        return new RouteResult(waypoints, true, accumulation);
    }
}
