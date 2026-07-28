namespace Novolis.Astro.Assessment;

/// <summary>
/// Kopparapu et al. 2013 habitable-zone calculator (erratum Table 3 coefficients).
/// Valid for 2600 K ≤ Teff ≤ 7200 K.
/// </summary>
public static class HabitableZoneCalculator
{
    /// <summary>Model lower Teff bound (K).</summary>
    public const double MinTeffK = 2600.0;

    /// <summary>Model upper Teff bound (K).</summary>
    public const double MaxTeffK = 7200.0;

    /// <summary>Solar absolute visual magnitude used for L from M_V.</summary>
    public const double SolarAbsoluteMagnitude = 4.83;

    /// <summary>
    /// Estimates the HZ for stellar luminosity (L☉) and effective temperature (K).
    /// Returns null when Teff is outside the Kopparapu range or luminosity is non-positive.
    /// </summary>
    public static HabitableZone? FromStellar(
        double luminositySolar,
        double teffK,
        HabitableZoneConvention convention = HabitableZoneConvention.Conservative)
    {
        if (luminositySolar <= 0 || teffK < MinTeffK || teffK > MaxTeffK)
            return null;

        var (innerLimit, outerLimit) = convention switch
        {
            HabitableZoneConvention.Optimistic => (HabitableZoneLimit.RecentVenus, HabitableZoneLimit.EarlyMars),
            _ => (HabitableZoneLimit.RunawayGreenhouse, HabitableZoneLimit.MaximumGreenhouse)
        };

        var inner = DistanceAu(luminositySolar, teffK, innerLimit);
        var outer = DistanceAu(luminositySolar, teffK, outerLimit);
        if (inner is null || outer is null || outer.Value <= inner.Value)
            return null;

        return new HabitableZone(
            inner.Value,
            outer.Value,
            teffK,
            luminositySolar,
            convention,
            innerLimit,
            outerLimit);
    }

    /// <summary>HZ distance in AU for one Kopparapu flux limit.</summary>
    public static double? DistanceAu(double luminositySolar, double teffK, HabitableZoneLimit limit)
    {
        if (luminositySolar <= 0 || teffK < MinTeffK || teffK > MaxTeffK)
            return null;

        var seff = EffectiveStellarFlux(teffK, limit);
        if (seff <= 0)
            return null;

        return Math.Sqrt(luminositySolar / seff);
    }

    /// <summary>
    /// S_eff(Teff) = S☉ + a T* + b T*^2 + c T*^3 + d T*^4, T* = Teff − 5780 K
    /// (Kopparapu et al. 2013 erratum Table 3).
    /// </summary>
    public static double EffectiveStellarFlux(double teffK, HabitableZoneLimit limit)
    {
        var c = Coefficients(limit);
        var t = teffK - 5780.0;
        return c.SeffSun + t * (c.A + t * (c.B + t * (c.C + t * c.D)));
    }

    /// <summary>L/L☉ from absolute visual magnitude (bolometric correction ignored).</summary>
    public static double LuminosityFromAbsoluteMagnitude(double absoluteMagnitude) =>
        Math.Pow(10.0, 0.4 * (SolarAbsoluteMagnitude - absoluteMagnitude));

    static Coeff Coefficients(HabitableZoneLimit limit) => limit switch
    {
        HabitableZoneLimit.RecentVenus => new(1.7763, 1.4335e-4, 3.3954e-9, -7.6364e-12, -1.1950e-15),
        HabitableZoneLimit.RunawayGreenhouse => new(1.0385, 1.2456e-4, 1.4612e-8, -7.6345e-12, -1.7511e-15),
        HabitableZoneLimit.MoistGreenhouse => new(1.0146, 8.1884e-5, 1.9394e-9, -4.3618e-12, -6.8260e-16),
        HabitableZoneLimit.MaximumGreenhouse => new(0.3507, 5.9578e-5, 1.6707e-9, -3.0058e-12, -5.1925e-16),
        HabitableZoneLimit.EarlyMars => new(0.3207, 5.4471e-5, 1.5275e-9, -2.1709e-12, -3.8282e-16),
        _ => throw new ArgumentOutOfRangeException(nameof(limit), limit, null)
    };

    readonly record struct Coeff(double SeffSun, double A, double B, double C, double D);
}
