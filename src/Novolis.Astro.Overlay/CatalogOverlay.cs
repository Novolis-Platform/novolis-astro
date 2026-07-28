using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Overlay;

/// <summary>Alias binding from a campaign/world label to a catalog system.</summary>
public sealed record OverlayEntry(
    string Alias,
    SystemId CatalogSystemId,
    IReadOnlyDictionary<string, string>? Labels = null);

/// <summary>Worldbuilding overlay of aliases and labels onto a catalog.</summary>
public sealed class CatalogOverlay
{
    readonly Dictionary<string, OverlayEntry> _byAlias = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>All overlay entries.</summary>
    public IReadOnlyCollection<OverlayEntry> Entries => _byAlias.Values;

    /// <summary>Adds or replaces an alias binding.</summary>
    public void Bind(OverlayEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (string.IsNullOrWhiteSpace(entry.Alias))
            throw new ArgumentException("Alias is required.", nameof(entry));
        _byAlias[entry.Alias] = entry;
    }

    /// <summary>Resolves an alias to a catalog system id.</summary>
    public bool TryResolve(string alias, out SystemId systemId)
    {
        if (_byAlias.TryGetValue(alias, out var entry))
        {
            systemId = entry.CatalogSystemId;
            return true;
        }

        systemId = default;
        return false;
    }

    /// <summary>Validates that every alias points at a system present in <paramref name="catalog"/>.</summary>
    public IReadOnlyList<string> Validate(StarCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var errors = new List<string>();
        foreach (var entry in _byAlias.Values)
        {
            if (!catalog.TryGet(entry.CatalogSystemId, out _))
                errors.Add($"Alias '{entry.Alias}' references missing system '{entry.CatalogSystemId.Value}'.");
        }

        return errors;
    }
}
