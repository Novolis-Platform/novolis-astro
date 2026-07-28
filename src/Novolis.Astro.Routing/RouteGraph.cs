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

/// <summary>Adjacency graph of feasible hops.</summary>
public sealed class RouteGraph
{
    /// <summary>Creates a graph from precomputed adjacency.</summary>
    public RouteGraph(IReadOnlyDictionary<string, IReadOnlyList<RouteEdge>> adjacency) =>
        Adjacency = adjacency;

    /// <summary>Outgoing edges keyed by system id string.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<RouteEdge>> Adjacency { get; }

    /// <summary>Builds a dense graph of hops up to <paramref name="maxRangeLy"/> under <paramref name="costModel"/>.</summary>
    public static RouteGraph Build(
        IEnumerable<StarSystem> systems,
        double maxRangeLy,
        IHopCostModel costModel)
    {
        ArgumentNullException.ThrowIfNull(systems);
        ArgumentNullException.ThrowIfNull(costModel);

        var list = systems.ToList();
        var adjacency = list.ToDictionary(
            s => s.Id.Value,
            _ => new List<RouteEdge>(),
            StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < list.Count; i++)
        {
            for (var j = i + 1; j < list.Count; j++)
            {
                var a = list[i];
                var b = list[j];
                var d = StarCoords.Distance(a.Coords, b.Coords);
                if (d <= 0 || d > maxRangeLy)
                    continue;

                var ab = costModel.Evaluate(a.Id, b.Id, d);
                if (ab.Feasible)
                {
                    adjacency[a.Id.Value].Add(new RouteEdge(a.Id, b.Id, d, ab.Cost, ab.BandTag));
                    adjacency[b.Id.Value].Add(new RouteEdge(b.Id, a.Id, d, ab.Cost, ab.BandTag));
                }
            }
        }

        return new RouteGraph(adjacency.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyList<RouteEdge>)kv.Value,
            StringComparer.OrdinalIgnoreCase));
    }
}
