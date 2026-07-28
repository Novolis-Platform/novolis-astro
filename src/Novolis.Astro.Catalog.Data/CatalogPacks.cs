using Novolis.Astro.Catalog;

namespace Novolis.Astro.Catalog.Data;

/// <summary>Pregenerated stellar catalog packs (committed <c>*.g.cs</c>).</summary>
public static partial class CatalogPacks
{
    /// <summary>Builds an indexed catalog from a named pack sequence.</summary>
    public static StarCatalog ToCatalog(IReadOnlyList<StarSystem> pack) => StarCatalog.From(pack);
}
