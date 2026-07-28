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

        var count = 0;
        using var reader = new StringReader(csvText);
        foreach (var system in Enumerate(reader))
        {
            catalog.Add(system);
            count++;
        }

        return count;
    }

    /// <summary>Streams systems from a HYG-like CSV reader (header required).</summary>
    public static IEnumerable<StarSystem> Enumerate(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var headerLine = reader.ReadLine();
        if (headerLine is null)
            yield break;

        var headers = SplitCsv(headerLine);
        var idIdx = IndexOf(headers, "id", "starid", "hip");
        var nameIdx = IndexOf(headers, "name", "proper", "gl");
        var xIdx = IndexOf(headers, "x", "xly", "x_pc");
        var yIdx = IndexOf(headers, "y", "yly", "y_pc");
        var zIdx = IndexOf(headers, "z", "zly", "z_pc");
        var spectIdx = IndexOf(headers, "spect", "spectral", "sp_type");
        var lumIdx = IndexOf(headers, "lum", "luminosity", "lum_solar");
        var absmagIdx = IndexOf(headers, "absmag", "abs_mag", "absolute_magnitude");
        var teffIdx = IndexOf(headers, "teff", "teff_k", "effective_temperature");
        if (idIdx < 0 || xIdx < 0 || yIdx < 0 || zIdx < 0)
            throw new InvalidOperationException("CSV must include id and x,y,z columns.");

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

            string? spect = null;
            if (spectIdx >= 0 && spectIdx < cols.Count && !string.IsNullOrWhiteSpace(cols[spectIdx]))
                spect = cols[spectIdx].Trim();

            double? lum = null;
            if (lumIdx >= 0 && lumIdx < cols.Count && TryParse(cols[lumIdx], out var lumVal) && lumVal > 0)
                lum = lumVal;

            double? absMag = null;
            if (absmagIdx >= 0 && absmagIdx < cols.Count && TryParse(cols[absmagIdx], out var absVal))
                absMag = absVal;

            double? teff = null;
            if (teffIdx >= 0 && teffIdx < cols.Count && TryParse(cols[teffIdx], out var teffVal) && teffVal > 0)
                teff = teffVal;

            var spectralClass = ParseSpectralClass(spect);
            yield return new StarSystem(
                id,
                name,
                new StarCoords(x, y, z),
                spectralClass,
                luminositySolar: lum,
                effectiveTemperatureK: teff,
                spectralDesignation: spect,
                absoluteMagnitude: absMag);
        }
    }

    static SpectralClass ParseSpectralClass(string? spect)
    {
        if (string.IsNullOrWhiteSpace(spect))
            return SpectralClass.Unknown;

        var text = spect.Trim().ToUpperInvariant();
        if (text.Contains("WD", StringComparison.Ordinal) || text.StartsWith('D'))
            return SpectralClass.WD;

        return text[0] switch
        {
            'O' => SpectralClass.O,
            'B' => SpectralClass.B,
            'A' => SpectralClass.A,
            'F' => SpectralClass.F,
            'G' => SpectralClass.G,
            'K' => SpectralClass.K,
            'M' => SpectralClass.M,
            'L' => SpectralClass.L,
            'T' => SpectralClass.T,
            'Y' => SpectralClass.Y,
            _ => SpectralClass.Unknown
        };
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
