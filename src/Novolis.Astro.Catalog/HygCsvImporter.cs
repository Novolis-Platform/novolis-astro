using System.Globalization;
using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Catalog;

/// <summary>Minimal HYG-like CSV importer (id, name, x, y, z columns).</summary>
public static class HygCsvImporter
{
    /// <summary>Imports systems from CSV text into <paramref name="catalog"/>.</summary>
    /// <returns>Number of rows imported.</returns>
    public static int Import(string csvText, StarCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(csvText);
        ArgumentNullException.ThrowIfNull(catalog);

        using var reader = new StringReader(csvText);
        var headerLine = reader.ReadLine();
        if (headerLine is null)
            return 0;

        var headers = SplitCsv(headerLine);
        var idIdx = IndexOf(headers, "id", "starid", "hip");
        var nameIdx = IndexOf(headers, "name", "proper", "gl");
        var xIdx = IndexOf(headers, "x", "xly", "x_pc");
        var yIdx = IndexOf(headers, "y", "yly", "y_pc");
        var zIdx = IndexOf(headers, "z", "zly", "z_pc");
        if (idIdx < 0 || xIdx < 0 || yIdx < 0 || zIdx < 0)
            throw new InvalidOperationException("CSV must include id and x,y,z columns.");

        var count = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var cols = SplitCsv(line);
            if (cols.Count <= Math.Max(Math.Max(idIdx, xIdx), Math.Max(yIdx, zIdx)))
                continue;

            var id = cols[idIdx].Trim();
            if (string.IsNullOrEmpty(id))
                continue;
            if (!TryParse(cols[xIdx], out var x) || !TryParse(cols[yIdx], out var y) || !TryParse(cols[zIdx], out var z))
                continue;

            var name = nameIdx >= 0 && nameIdx < cols.Count && !string.IsNullOrWhiteSpace(cols[nameIdx])
                ? cols[nameIdx].Trim()
                : id;
            catalog.Add(new StarSystem(id, name, new StarCoords(x, y, z)));
            count++;
        }

        return count;
    }

    static bool TryParse(string s, out double value) =>
        double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    static int IndexOf(IReadOnlyList<string> headers, params string[] names)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            var h = headers[i].Trim().Trim('"');
            foreach (var n in names)
            {
                if (string.Equals(h, n, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
        }

        return -1;
    }

    static List<string> SplitCsv(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        result.Add(current.ToString());
        return result;
    }
}
