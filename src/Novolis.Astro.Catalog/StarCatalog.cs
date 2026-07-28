using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Catalog;

/// <summary>In-memory star system catalog with spatial queries.</summary>
public sealed class StarCatalog
{
    readonly Dictionary<string, StarSystem> _byId = new(StringComparer.OrdinalIgnoreCase);
    readonly List<StarSystem> _ordered = [];

    /// <summary>Creates an empty catalog.</summary>
    public StarCatalog()
    {
    }

    /// <summary>Creates a catalog from a sequence, preserving order. Duplicate ids throw.</summary>
    public static StarCatalog From(IEnumerable<StarSystem> systems)
    {
        ArgumentNullException.ThrowIfNull(systems);
        var catalog = new StarCatalog();
        foreach (var system in systems)
        {
            ArgumentNullException.ThrowIfNull(system);
            if (catalog._byId.ContainsKey(system.Id.Value))
                throw new ArgumentException($"Duplicate system id '{system.Id.Value}'.", nameof(systems));
            catalog._byId[system.Id.Value] = system;
            catalog._ordered.Add(system);
        }

        return catalog;
    }

    /// <summary>All systems in insertion / pack order.</summary>
    public IReadOnlyList<StarSystem> All => _ordered;

    /// <summary>Number of systems.</summary>
    public int Count => _ordered.Count;

    /// <summary>Adds or replaces a system. Replacement keeps the existing slot in <see cref="All"/>.</summary>
    public void Add(StarSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        if (_byId.TryGetValue(system.Id.Value, out var existing))
        {
            var index = _ordered.IndexOf(existing);
            _byId[system.Id.Value] = system;
            if (index >= 0)
                _ordered[index] = system;
            else
                _ordered.Add(system);
            return;
        }

        _byId[system.Id.Value] = system;
        _ordered.Add(system);
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
        foreach (var system in _ordered)
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
