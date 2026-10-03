using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Catalog;

/// <summary>A catalog star after projection, with a deterministic display radius.</summary>
public sealed record CelestialProjectedStar(
    CelestialCatalogEntry Entry,
    CelestialProjectedCoordinate Coordinate,
    double RadiusPixels)
{
    /// <summary>Stable catalog id.</summary>
    public string Id => Entry.Id;

    /// <summary>Display name.</summary>
    public string Label => Entry.Name;

    /// <summary>Apparent magnitude retained for scene renderers.</summary>
    public double? ApparentMagnitude => Entry.ApparentMagnitude;
}
