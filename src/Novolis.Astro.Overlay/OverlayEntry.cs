using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Overlay;

/// <summary>Alias binding from a campaign/world label to a catalog system.</summary>
public sealed record OverlayEntry(
    string Alias,
    SystemId CatalogSystemId,
    IReadOnlyDictionary<string, string>? Labels = null);
