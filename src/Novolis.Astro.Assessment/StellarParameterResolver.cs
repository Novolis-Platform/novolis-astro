using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

/// <summary>Parses spectral designations and estimates Teff / luminosity fallbacks.</summary>
public static class StellarParameterResolver
{
    /// <summary>Resolved stellar parameters used for HZ / habitability.</summary>
    public sealed record ResolvedStellarParameters(
        double? LuminositySolar,
        double? TeffK,
        SpectralClass SpectralClass,
        int? Subtype,
        LuminosityClass LuminosityClass,
        bool IsWhiteDwarf,
        bool HasExplicitLuminosity,
        bool HasExplicitTeff,
        bool HasSpectralDesignation);

    /// <summary>Morgan–Keenan luminosity class bucket.</summary>
    public enum LuminosityClass
    {
        /// <summary>Unknown / unspecified.</summary>
        Unknown = 0,
        /// <summary>Main sequence (V).</summary>
        MainSequence,
        /// <summary>Subgiant (IV).</summary>
        Subgiant,
        /// <summary>Giant (III).</summary>
        Giant,
        /// <summary>Bright giant (II / Ib).</summary>
        BrightGiant,
        /// <summary>Supergiant (I / Ia).</summary>
        Supergiant
    }

    /// <summary>Resolves luminosity and Teff from catalog fields with documented fallbacks.</summary>
    public static ResolvedStellarParameters Resolve(StarSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);

        var parsed = ParseDesignation(system.SpectralDesignation);
        var spectral = system.SpectralClass != SpectralClass.Unknown
            ? system.SpectralClass
            : parsed.SpectralClass;
        var subtype = parsed.Subtype;
        var lumClass = parsed.LuminosityClass;
        var isWd = spectral == SpectralClass.WD || parsed.IsWhiteDwarf;

        double? lum = system.LuminositySolar is > 0 ? system.LuminositySolar : null;
        var hasExplicitLum = lum is not null;
        if (lum is null && system.AbsoluteMagnitude is { } absMag)
            lum = HabitableZoneCalculator.LuminosityFromAbsoluteMagnitude(absMag);
        if (lum is null)
            lum = EstimateLuminositySolar(spectral, subtype, lumClass);

        double? teff = system.EffectiveTemperatureK is > 0 ? system.EffectiveTemperatureK : null;
        var hasExplicitTeff = teff is not null;
        if (teff is null)
            teff = EstimateTeffK(spectral, subtype);

        return new ResolvedStellarParameters(
            lum,
            teff,
            spectral,
            subtype,
            lumClass,
            isWd,
            hasExplicitLum,
            hasExplicitTeff,
            !string.IsNullOrWhiteSpace(system.SpectralDesignation));
    }

    /// <summary>Parses a spectral designation such as G2V or M5.5Ve.</summary>
    public static (SpectralClass SpectralClass, int? Subtype, LuminosityClass LuminosityClass, bool IsWhiteDwarf)
        ParseDesignation(string? spect)
    {
        if (string.IsNullOrWhiteSpace(spect))
            return (SpectralClass.Unknown, null, LuminosityClass.Unknown, false);

        var text = spect.Trim().ToUpperInvariant();
        var isWd = text.Contains("WD", StringComparison.Ordinal) || text.StartsWith('D');

        var spectral = text[0] switch
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
            _ => isWd ? SpectralClass.WD : SpectralClass.Unknown
        };

        int? subtype = null;
        foreach (var ch in text)
        {
            if (!char.IsDigit(ch))
                continue;
            subtype = ch - '0';
            break;
        }

        var luminosity = text.Contains("IA", StringComparison.Ordinal) || text.Contains("IAB", StringComparison.Ordinal)
            ? LuminosityClass.Supergiant
            : text.Contains("IB", StringComparison.Ordinal) || text.Contains("II", StringComparison.Ordinal)
                ? LuminosityClass.BrightGiant
                : text.Contains("III", StringComparison.Ordinal)
                    ? LuminosityClass.Giant
                    : text.Contains("IV", StringComparison.Ordinal)
                        ? LuminosityClass.Subgiant
                        : text.Contains('V')
                            ? LuminosityClass.MainSequence
                            : LuminosityClass.Unknown;

        return (spectral, subtype, luminosity, isWd);
    }

    /// <summary>
    /// Main-sequence Teff (K) from spectral type (compact Pecaut &amp; Mamajek–style table).
    /// </summary>
    public static double? EstimateTeffK(SpectralClass spectral, int? subtype)
    {
        var s = Math.Clamp(subtype ?? DefaultSubtype(spectral), 0, 9);
        return spectral switch
        {
            SpectralClass.O => Lerp(54000, 38000, s / 9.0),
            SpectralClass.B => Lerp(29200, 10700, s / 9.0),
            SpectralClass.A => Lerp(9700, 7300, s / 9.0),
            SpectralClass.F => Lerp(7200, 6000, s / 9.0),
            SpectralClass.G => Lerp(5900, 5200, s / 9.0),
            SpectralClass.K => Lerp(5250, 3900, s / 9.0),
            SpectralClass.M => Lerp(3850, 2500, s / 9.0),
            SpectralClass.L => 2000,
            SpectralClass.T => 1200,
            SpectralClass.Y => 600,
            _ => null
        };
    }

    /// <summary>Rough main-sequence luminosity (L☉) when catalog Lum is absent.</summary>
    public static double? EstimateLuminositySolar(
        SpectralClass spectral,
        int? subtype,
        LuminosityClass luminosityClass)
    {
        if (spectral is SpectralClass.WD or SpectralClass.NS or SpectralClass.BH or SpectralClass.Unknown)
            return null;

        var s = Math.Clamp(subtype ?? DefaultSubtype(spectral), 0, 9);
        var ms = spectral switch
        {
            SpectralClass.O => Lerp(1e5, 3e4, s / 9.0),
            SpectralClass.B => Lerp(2e4, 50, s / 9.0),
            SpectralClass.A => Lerp(40, 8, s / 9.0),
            SpectralClass.F => Lerp(6.5, 1.5, s / 9.0),
            SpectralClass.G => Lerp(1.5, 0.6, s / 9.0),
            SpectralClass.K => Lerp(0.55, 0.08, s / 9.0),
            SpectralClass.M => Lerp(0.08, 0.0015, s / 9.0),
            SpectralClass.L => 1e-4,
            SpectralClass.T => 1e-5,
            SpectralClass.Y => 1e-6,
            _ => (double?)null
        };
        if (ms is null)
            return null;

        return luminosityClass switch
        {
            LuminosityClass.Giant => ms.Value * 40,
            LuminosityClass.BrightGiant => ms.Value * 200,
            LuminosityClass.Supergiant => ms.Value * 1000,
            LuminosityClass.Subgiant => ms.Value * 3,
            _ => ms.Value
        };
    }

    static int DefaultSubtype(SpectralClass spectral) => spectral switch
    {
        SpectralClass.G => 2,
        SpectralClass.K => 5,
        SpectralClass.M => 3,
        SpectralClass.F => 5,
        _ => 5
    };

    static double Lerp(double a, double b, double t) => a + (b - a) * t;
}
