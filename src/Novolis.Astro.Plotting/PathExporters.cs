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

/// <summary>Exports a path as a simple SVG polyline.</summary>
public static class PathSvgExporter
{
    /// <summary>Builds an SVG document for the given waypoint coordinates.</summary>
    public static string Export(IReadOnlyList<StarCoords> waypoints, int width = 800, int height = 600, int margin = 40)
    {
        ArgumentNullException.ThrowIfNull(waypoints);
        if (waypoints.Count == 0)
            return """<svg xmlns="http://www.w3.org/2000/svg"></svg>""";

        var pts = waypoints.Select(OrthographicProjector.Project).ToList();
        var minU = pts.Min(p => p.U);
        var maxU = pts.Max(p => p.U);
        var minV = pts.Min(p => p.V);
        var maxV = pts.Max(p => p.V);
        var spanU = Math.Max(maxU - minU, 1e-9);
        var spanV = Math.Max(maxV - minV, 1e-9);
        var innerW = width - 2.0 * margin;
        var innerH = height - 2.0 * margin;

        (double X, double Y) Map((double U, double V) p)
        {
            var x = margin + (p.U - minU) / spanU * innerW;
            var y = margin + (1.0 - (p.V - minV) / spanV) * innerH;
            return (x, y);
        }

        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\">");
        sb.Append("<rect width=\"100%\" height=\"100%\" fill=\"#0b1020\"/>");
        sb.Append("<polyline fill=\"none\" stroke=\"#6ec1ff\" stroke-width=\"2\" points=\"");
        foreach (var p in pts)
        {
            var m = Map(p);
            sb.Append(CultureInfo.InvariantCulture, $"{m.X:0.##},{m.Y:0.##} ");
        }

        sb.Append("\"/>");
        foreach (var p in pts)
        {
            var m = Map(p);
            sb.Append(CultureInfo.InvariantCulture, $"<circle cx=\"{m.X:0.##}\" cy=\"{m.Y:0.##}\" r=\"4\" fill=\"#ffd166\"/>");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }
}

/// <summary>Exports waypoint coordinates as TSV.</summary>
public static class PathTsvExporter
{
    /// <summary>Writes index, x_ly, y_ly, z_ly rows.</summary>
    public static string Export(IReadOnlyList<StarCoords> waypoints)
    {
        ArgumentNullException.ThrowIfNull(waypoints);
        var sb = new StringBuilder();
        sb.AppendLine("index\tx_ly\ty_ly\tz_ly");
        for (var i = 0; i < waypoints.Count; i++)
        {
            var c = waypoints[i];
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{i}\t{c.X}\t{c.Y}\t{c.Z}"));
        }

        return sb.ToString();
    }
}
