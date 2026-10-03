using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Catalog.Data;

/// <summary>Pregenerated stellar catalog packs (committed <c>*.g.cs</c>).</summary>
public static partial class CatalogPacks
{
    /// <summary>Builds an indexed catalog from a named pack sequence.</summary>
    public static StarCatalog ToCatalog(IReadOnlyList<StarSystem> pack) => StarCatalog.From(pack);

    /// <summary>Builds an indexed catalog from enriched, Sol-normalized pack entries.</summary>
    public static StarCatalog ToCatalog(IReadOnlyList<CelestialCatalogEntry> pack) =>
        StarCatalog.From(pack.Select(ToStarSystem));

    /// <summary>Gets enriched entries from the named generated pack.</summary>
    public static IReadOnlyList<CelestialCatalogEntry> GetEntries(CatalogPackId pack) =>
        pack switch
        {
            CatalogPackId.NearSol100 => NearSol100Entries,
            CatalogPackId.HygLocal1901 => HygLocal1901Entries,
            _ => throw new ArgumentOutOfRangeException(nameof(pack), pack, "Unknown catalog pack."),
        };

    /// <summary>Gets source and normalization provenance from the named generated pack.</summary>
    public static CatalogPackProvenance GetProvenance(CatalogPackId pack) =>
        pack switch
        {
            CatalogPackId.NearSol100 => NearSol100Provenance,
            CatalogPackId.HygLocal1901 => HygLocal1901Provenance,
            _ => throw new ArgumentOutOfRangeException(nameof(pack), pack, "Unknown catalog pack."),
        };

    /// <summary>Opens the packageable NDJSON snapshot for the named generated pack.</summary>
    public static Stream OpenNdjson(CatalogPackId pack)
    {
        var resourceName = pack switch
        {
            CatalogPackId.NearSol100 =>
                "Novolis.Astro.Catalog.Data.Generated.NearSol100.ndjson",
            CatalogPackId.HygLocal1901 =>
                "Novolis.Astro.Catalog.Data.Generated.HygLocal1901.ndjson",
            _ => throw new ArgumentOutOfRangeException(nameof(pack), pack, "Unknown catalog pack."),
        };
        return typeof(CatalogPacks).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"The generated catalog snapshot '{resourceName}' was not embedded.");
    }

    static StarSystem ToStarSystem(CelestialCatalogEntry entry) =>
        new(
            entry.Id,
            entry.Name,
            entry.SolRelativeCartesian,
            entry.SpectralClass,
            entry.Tags,
            entry.LuminositySolar,
            effectiveTemperatureK: null,
            entry.SpectralDesignation,
            entry.AbsoluteMagnitude);
}
