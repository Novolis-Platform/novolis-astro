using System.Globalization;
using System.Text;
using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Plotting;

/// <summary>Orthographic projection onto the XZ plane (drops Y).</summary>
public static class OrthographicProjector
{
    /// <summary>Projects stellar coords to 2D map units.</summary>
    public static (double U, double V) Project(StarCoords coords) => (coords.X, coords.Z);
}
