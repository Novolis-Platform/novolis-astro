using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Catalog.Data;

/// <summary>Source and coordinate-frame facts for a generated catalog pack.</summary>
public sealed record CatalogPackProvenance(
    string Name,
    string SourceFile,
    string SourceDescription,
    CelestialCartesianFrame CartesianFrame,
    string SolSourceId,
    int RecordCount,
    bool IsSolNormalized);
