using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Catalog;

/// <summary>In-memory star system catalog with spatial queries.</summary>
public sealed class StarCatalog
{
    readonly Dictionary<string, StarSystem> _byId = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>All systems currently in the catalog.</summary>
    public IReadOnlyCollection<StarSystem> All => _byId.Values;

    /// <summary>Number of systems.</summary>
    public int Count => _byId.Count;

    /// <summary>Adds or replaces a system.</summary>
    public void Add(StarSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        _byId[system.Id.Value] = system;
    }

    /// <summary>Tries to get a system by id.</summary>
    public bool TryGet(SystemId id, out StarSystem? system) =>
        _byId.TryGetValue(id.Value, out system);

    /// <summary>Gets a system or throws.</summary>
    public StarSystem GetRequired(SystemId id) =>
        TryGet(id, out var system) && system is not null
            ? system
            : throw new KeyNotFoundException($"Unknown system id '{id.Value}'.");

    /// <summary>Systems within <paramref name="radiusLy"/> of <paramref name="origin"/> (inclusive), excluding the origin id when present.</summary>
    public IReadOnlyList<(StarSystem System, double DistanceLy)> NeighborsWithin(
        StarCoords origin,
        double radiusLy,
        SystemId? excludeId = null)
    {
        var list = new List<(StarSystem, double)>();
        foreach (var system in _byId.Values)
        {
            if (excludeId is { } ex && string.Equals(system.Id.Value, ex.Value, StringComparison.OrdinalIgnoreCase))
                continue;
            var d = StarCoords.Distance(origin, system.Coords);
            if (d <= radiusLy)
                list.Add((system, d));
        }

        list.Sort(static (a, b) => a.Item2.CompareTo(b.Item2));
        return list;
    }
}
