using System.Globalization;
using System.Text;
using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Plotting;

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
