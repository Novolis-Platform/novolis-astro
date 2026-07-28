using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Catalog;

/// <summary>A named system in a stellar catalog.</summary>
public sealed class StarSystem
{
    /// <summary>Creates a catalog system entry.</summary>
    public StarSystem(
        SystemId id,
        string name,
        StarCoords coords,
        SpectralClass spectralClass = SpectralClass.Unknown,
        IReadOnlyList<string>? tags = null)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Coords = coords;
        SpectralClass = spectralClass;
        Tags = tags ?? Array.Empty<string>();
    }

    /// <summary>Catalog id.</summary>
    public SystemId Id { get; }

    /// <summary>Display name.</summary>
    public string Name { get; }

    /// <summary>Position in light-years.</summary>
    public StarCoords Coords { get; }

    /// <summary>Optional spectral class.</summary>
    public SpectralClass SpectralClass { get; }

    /// <summary>Free-form tags.</summary>
    public IReadOnlyList<string> Tags { get; }
}
