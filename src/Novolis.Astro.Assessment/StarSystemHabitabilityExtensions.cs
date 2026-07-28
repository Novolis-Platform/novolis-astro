using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

/// <summary>Habitable-zone and habitability extensions for catalog systems.</summary>
public static class StarSystemHabitabilityExtensions
{
    /// <summary>Estimates the Kopparapu habitable zone for this primary, or null when inapplicable.</summary>
    public static HabitableZone? EstimateHabitableZone(
        this StarSystem system,
        HabitableZoneConvention convention = HabitableZoneConvention.Conservative)
    {
        ArgumentNullException.ThrowIfNull(system);
        var p = StellarParameterResolver.Resolve(system);
        if (p.LuminositySolar is not > 0 || p.TeffK is not { } teff)
            return null;
        return HabitableZoneCalculator.FromStellar(p.LuminositySolar.Value, teff, convention);
    }

    /// <summary>Scores Earth-analog host prospects with a deterministic weighted model.</summary>
    public static HabitabilityRating AssessHabitability(
        this StarSystem system,
        HabitableZoneConvention convention = HabitableZoneConvention.Conservative)
    {
        ArgumentNullException.ThrowIfNull(system);
        var p = StellarParameterResolver.Resolve(system);
        var reasons = new List<string>();
        var excluded = IsExcluded(p, reasons);

        HabitableZone? zone = null;
        if (p.LuminositySolar is > 0 && p.TeffK is { } teff)
            zone = HabitableZoneCalculator.FromStellar(p.LuminositySolar.Value, teff, convention);

        if (zone is null && !excluded)
            reasons.Add("Habitable zone unavailable (Teff/L outside Kopparapu domain or missing)");

        var stellar = ScoreStellarClass(p, reasons);
        var geometry = ScoreHzGeometry(zone, p, reasons);
        var evolution = ScoreEvolutionaryState(p);
        var activity = ScoreActivityProxy(p, zone, reasons);
        var confidence = ScoreDataConfidence(system, p);

        var score =
            stellar * 0.30 +
            geometry * 0.30 +
            evolution * 0.15 +
            activity * 0.15 +
            confidence * 0.10;

        if (excluded)
            score = Math.Min(score, 25.0);

        score = Math.Clamp(score, 0, 100);
        var tier = DetermineTier(score, excluded);
        return new HabitabilityRating(score, tier, excluded, zone, reasons);
    }

    static bool IsExcluded(StellarParameterResolver.ResolvedStellarParameters p, List<string> reasons)
    {
        if (p.IsWhiteDwarf || p.SpectralClass is SpectralClass.WD or SpectralClass.NS or SpectralClass.BH)
        {
            reasons.Add("Degenerate or compact remnant — excluded");
            return true;
        }

        if (p.SpectralClass is SpectralClass.O or SpectralClass.B)
        {
            reasons.Add("Hot O/B primary — excluded from Earth-analog rating");
            return true;
        }

        if (p.LuminosityClass is StellarParameterResolver.LuminosityClass.Giant
            or StellarParameterResolver.LuminosityClass.BrightGiant
            or StellarParameterResolver.LuminosityClass.Supergiant)
        {
            reasons.Add("Evolved giant/supergiant — excluded");
            return true;
        }

        if (p.LuminositySolar is null or <= 0)
        {
            reasons.Add("Missing positive luminosity");
            return true;
        }

        if (p.TeffK is null)
        {
            reasons.Add("Missing effective temperature");
            return true;
        }

        if (p.TeffK < HabitableZoneCalculator.MinTeffK || p.TeffK > HabitableZoneCalculator.MaxTeffK)
        {
            reasons.Add($"Teff {p.TeffK:0} K outside Kopparapu 2600–7200 K domain");
            return true;
        }

        return false;
    }

    static double ScoreStellarClass(StellarParameterResolver.ResolvedStellarParameters p, List<string> reasons)
    {
        var baseScore = p.SpectralClass switch
        {
            SpectralClass.K => 95.0,
            SpectralClass.G => 92.0,
            SpectralClass.F => 72.0,
            SpectralClass.M => 42.0,
            SpectralClass.A => 28.0,
            SpectralClass.B => 5.0,
            SpectralClass.O => 0.0,
            SpectralClass.L or SpectralClass.T or SpectralClass.Y => 10.0,
            _ => 40.0
        };

        if (p.Subtype is int subtype)
        {
            baseScore += p.SpectralClass switch
            {
                SpectralClass.K => Math.Clamp(subtype - 4, -4, 4),
                SpectralClass.G => Math.Clamp(3 - subtype, -3, 3),
                SpectralClass.F => Math.Clamp(5 - subtype, -5, 2),
                SpectralClass.M => -Math.Clamp(subtype, 0, 8),
                _ => 0
            };
        }

        if (p.SpectralClass is SpectralClass.G)
            reasons.Add("Stable G-class primary");
        else if (p.SpectralClass is SpectralClass.K)
            reasons.Add("Stable K-class primary");
        else if (p.SpectralClass is SpectralClass.M)
            reasons.Add("M-dwarf primary (lifetime favorable; HZ/activity challenges)");

        return Math.Clamp(baseScore, 0, 100);
    }

    static double ScoreHzGeometry(
        HabitableZone? zone,
        StellarParameterResolver.ResolvedStellarParameters p,
        List<string> reasons)
    {
        if (zone is null)
            return 20.0;

        var score = 50.0;
        var width = zone.WidthAu;
        var mid = zone.MidAu;

        if (width >= 0.5) score += 25;
        else if (width >= 0.2) score += 10;
        else score -= 10;

        // Earth-like orbital scales favored (conservative Sol mid ≈ 1.3 AU).
        if (mid is >= 0.7 and <= 1.5) score += 20;
        else if (mid is >= 0.5 and <= 2.0) score += 10;
        else if (mid < 0.1) score -= 25;
        else if (mid < 0.25) score -= 15;
        else if (mid > 5.0) score -= 10;

        if (p.SpectralClass == SpectralClass.M && mid < 0.2)
        {
            score -= 15;
            reasons.Add("Close-in M-dwarf HZ (tidal-locking / atmospheric-loss risk)");
        }

        reasons.Add($"HZ {zone.InnerAu:0.###}–{zone.OuterAu:0.###} AU ({zone.Convention})");
        return Math.Clamp(score, 0, 100);
    }

    static double ScoreEvolutionaryState(StellarParameterResolver.ResolvedStellarParameters p) =>
        p.LuminosityClass switch
        {
            StellarParameterResolver.LuminosityClass.MainSequence => 100.0,
            StellarParameterResolver.LuminosityClass.Subgiant => 60.0,
            StellarParameterResolver.LuminosityClass.Giant => 10.0,
            StellarParameterResolver.LuminosityClass.BrightGiant => 0.0,
            StellarParameterResolver.LuminosityClass.Supergiant => 0.0,
            _ => 65.0
        };

    static double ScoreActivityProxy(
        StellarParameterResolver.ResolvedStellarParameters p,
        HabitableZone? zone,
        List<string> reasons)
    {
        var score = p.SpectralClass switch
        {
            SpectralClass.K => 92.0,
            SpectralClass.G => 88.0,
            SpectralClass.F => 68.0,
            SpectralClass.M => 35.0,
            SpectralClass.A => 20.0,
            SpectralClass.B => 5.0,
            SpectralClass.O => 0.0,
            _ => 50.0
        };

        if (p.SpectralClass == SpectralClass.M && p.Subtype is >= 5)
        {
            score -= 15;
            reasons.Add("Mid/late M flare and XUV activity penalty");
        }

        if (zone is { MidAu: < 0.15 } && p.SpectralClass == SpectralClass.M)
            score -= 10;

        return Math.Clamp(score, 0, 100);
    }

    static double ScoreDataConfidence(StarSystem system, StellarParameterResolver.ResolvedStellarParameters p)
    {
        var present = 0;
        const int total = 5;
        if (p.HasExplicitLuminosity || system.AbsoluteMagnitude is not null) present++;
        if (p.HasExplicitTeff || p.HasSpectralDesignation || system.SpectralClass != SpectralClass.Unknown) present++;
        present++; // StarCoords always present
        if (!string.IsNullOrWhiteSpace(system.Name)) present++;
        if (p.LuminosityClass != StellarParameterResolver.LuminosityClass.Unknown || p.HasSpectralDesignation) present++;
        return 100.0 * present / total;
    }

    static HabitabilityTier DetermineTier(double score, bool excluded)
    {
        if (excluded) return HabitabilityTier.Excluded;
        if (score < 25) return HabitabilityTier.Hostile;
        if (score < 45) return HabitabilityTier.Marginal;
        if (score < 65) return HabitabilityTier.Candidate;
        if (score < 85) return HabitabilityTier.Favorable;
        return HabitabilityTier.Prime;
    }
}
