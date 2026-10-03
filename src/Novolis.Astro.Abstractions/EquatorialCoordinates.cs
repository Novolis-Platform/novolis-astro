namespace Novolis.Astro.Abstractions;

/// <summary>Right ascension, declination, and distance measured from Sol.</summary>
public readonly record struct EquatorialCoordinates
{
    /// <summary>Creates normalized equatorial coordinates.</summary>
    public EquatorialCoordinates(
        double rightAscensionDegrees,
        double declinationDegrees,
        double distanceLightYears)
    {
        if (!double.IsFinite(rightAscensionDegrees))
            throw new ArgumentOutOfRangeException(nameof(rightAscensionDegrees));
        if (!double.IsFinite(declinationDegrees)
            || declinationDegrees is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(declinationDegrees));
        }

        if (!double.IsFinite(distanceLightYears) || distanceLightYears < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceLightYears));

        RightAscensionDegrees = NormalizeRightAscension(rightAscensionDegrees);
        DeclinationDegrees = declinationDegrees;
        DistanceLightYears = distanceLightYears;
    }

    /// <summary>Right ascension in degrees, normalized to [0, 360).</summary>
    public double RightAscensionDegrees { get; }

    /// <summary>Declination in degrees, in [-90, 90].</summary>
    public double DeclinationDegrees { get; }

    /// <summary>Distance from Sol in light-years.</summary>
    public double DistanceLightYears { get; }

    /// <summary>Normalizes any finite right ascension angle to [0, 360).</summary>
    public static double NormalizeRightAscension(double degrees)
    {
        if (!double.IsFinite(degrees))
            throw new ArgumentOutOfRangeException(nameof(degrees));

        var normalized = degrees % 360d;
        return normalized < 0 ? normalized + 360d : normalized;
    }
}
