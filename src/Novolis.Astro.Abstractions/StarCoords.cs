namespace Novolis.Astro.Abstractions;

/// <summary>3D stellar position in light-years from a chosen origin.</summary>
public readonly record struct StarCoords(double X, double Y, double Z)
{
    /// <summary>Euclidean distance from the origin in light-years.</summary>
    public double DistanceFromOrigin => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>Euclidean distance between two positions in light-years.</summary>
    public static double Distance(StarCoords a, StarCoords b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
