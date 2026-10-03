namespace Novolis.Astro.Catalog;

/// <summary>A projected 2D coordinate plus retained depth for inverse transforms.</summary>
public readonly record struct CelestialProjectedCoordinate
{
    /// <summary>Creates a finite projected coordinate.</summary>
    public CelestialProjectedCoordinate(double x, double y, double depth)
    {
        if (!double.IsFinite(x))
            throw new ArgumentOutOfRangeException(nameof(x));
        if (!double.IsFinite(y))
            throw new ArgumentOutOfRangeException(nameof(y));
        if (!double.IsFinite(depth))
            throw new ArgumentOutOfRangeException(nameof(depth));

        X = x;
        Y = y;
        Depth = depth;
    }

    /// <summary>Horizontal projected coordinate.</summary>
    public double X { get; }

    /// <summary>Vertical projected coordinate.</summary>
    public double Y { get; }

    /// <summary>Retained radial distance or source-frame depth.</summary>
    public double Depth { get; }
}
