namespace Novolis.Astro.Abstractions;

/// <summary>
/// A provenance-preserving stellar catalog entry with coordinates normalized
/// to a Sol-centered Cartesian origin.
/// </summary>
public sealed record CelestialCatalogEntry
{
    /// <summary>Creates a normalized catalog entry.</summary>
    public CelestialCatalogEntry(
        string id,
        string sourceId,
        string name,
        CelestialCartesianFrame cartesianFrame,
        StarCoords solRelativeCartesian,
        EquatorialCoordinates equatorial,
        double? apparentMagnitude,
        double? absoluteMagnitude,
        SpectralClass spectralClass,
        string? spectralDesignation = null,
        double? luminositySolar = null,
        IReadOnlyList<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A catalog id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("A source id is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A catalog name is required.", nameof(name));
        if (!Enum.IsDefined(cartesianFrame))
            throw new ArgumentOutOfRangeException(nameof(cartesianFrame));
        if (apparentMagnitude is { } apparent && !double.IsFinite(apparent))
            throw new ArgumentOutOfRangeException(nameof(apparentMagnitude));
        if (absoluteMagnitude is { } absolute && !double.IsFinite(absolute))
            throw new ArgumentOutOfRangeException(nameof(absoluteMagnitude));
        if (luminositySolar is { } luminosity
            && (!double.IsFinite(luminosity) || luminosity < 0))
        {
            throw new ArgumentOutOfRangeException(nameof(luminositySolar));
        }

        Id = id.Trim();
        SourceId = sourceId.Trim();
        Name = name.Trim();
        CartesianFrame = cartesianFrame;
        SolRelativeCartesian = solRelativeCartesian;
        Equatorial = equatorial;
        ApparentMagnitude = apparentMagnitude;
        AbsoluteMagnitude = absoluteMagnitude;
        SpectralClass = spectralClass;
        SpectralDesignation = spectralDesignation;
        LuminositySolar = luminositySolar;
        Tags = tags?.ToArray() ?? [];
    }

    /// <summary>Stable id within the generated catalog pack.</summary>
    public string Id { get; }

    /// <summary>Stable id from the original source data.</summary>
    public string SourceId { get; }

    /// <summary>Display name.</summary>
    public string Name { get; }

    /// <summary>Frame of <see cref="SolRelativeCartesian"/>.</summary>
    public CelestialCartesianFrame CartesianFrame { get; }

    /// <summary>Cartesian position in light-years with Sol exactly at the origin.</summary>
    public StarCoords SolRelativeCartesian { get; }

    /// <summary>Equatorial J2000 position derived or preserved from the source.</summary>
    public EquatorialCoordinates Equatorial { get; }

    /// <summary>Apparent visual magnitude when the source provides it.</summary>
    public double? ApparentMagnitude { get; }

    /// <summary>Absolute visual magnitude when the source provides it.</summary>
    public double? AbsoluteMagnitude { get; }

    /// <summary>Broad spectral class.</summary>
    public SpectralClass SpectralClass { get; }

    /// <summary>Full spectral designation when the source provides it.</summary>
    public string? SpectralDesignation { get; }

    /// <summary>Bolometric luminosity in solar units when the source provides it.</summary>
    public double? LuminositySolar { get; }

    /// <summary>Source tags retained by the generated pack.</summary>
    public IReadOnlyList<string> Tags { get; }
}
