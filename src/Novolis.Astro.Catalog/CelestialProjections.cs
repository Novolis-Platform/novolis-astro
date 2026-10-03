using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Catalog;

/// <summary>Forward and inverse Sol-centered celestial projection transforms.</summary>
public static class CelestialProjections
{
    /// <summary>Projects a catalog entry into the selected Sol-centered view.</summary>
    public static CelestialProjectedCoordinate Project(
        CelestialCatalogEntry entry,
        CelestialProjectionKind kind)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return kind switch
        {
            CelestialProjectionKind.SolCenteredCartesian =>
                ProjectSolCenteredCartesian(entry.SolRelativeCartesian),
            CelestialProjectionKind.EquatorialPlate =>
                ProjectEquatorialPlate(entry.Equatorial),
            CelestialProjectionKind.PolarAzimuthal =>
                ProjectPolarAzimuthal(entry.Equatorial),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown projection kind."),
        };
    }

    /// <summary>Projects a catalog entry and calculates an accessible display radius.</summary>
    public static CelestialProjectedStar ProjectStar(
        CelestialCatalogEntry entry,
        CelestialProjectionKind kind,
        double minimumRadiusPixels = 2,
        double maximumRadiusPixels = 8) =>
        new(
            entry,
            Project(entry, kind),
            RadiusForMagnitude(
                entry.ApparentMagnitude,
                minimumRadiusPixels,
                maximumRadiusPixels));

    /// <summary>Projects source-frame Cartesian coordinates to a 2D view while retaining the Y axis.</summary>
    public static CelestialProjectedCoordinate ProjectSolCenteredCartesian(
        StarCoords coordinate) =>
        new(coordinate.X, coordinate.Z, coordinate.Y);

    /// <summary>Inverts <see cref="ProjectSolCenteredCartesian"/> exactly.</summary>
    public static StarCoords UnprojectSolCenteredCartesian(
        CelestialProjectedCoordinate coordinate) =>
        new(coordinate.X, coordinate.Depth, coordinate.Y);

    /// <summary>Projects right ascension and declination directly to a plate carrée view.</summary>
    public static CelestialProjectedCoordinate ProjectEquatorialPlate(
        EquatorialCoordinates coordinate) =>
        new(
            coordinate.RightAscensionDegrees,
            coordinate.DeclinationDegrees,
            coordinate.DistanceLightYears);

    /// <summary>Inverts <see cref="ProjectEquatorialPlate"/>.</summary>
    public static EquatorialCoordinates UnprojectEquatorialPlate(
        CelestialProjectedCoordinate coordinate)
    {
        if (coordinate.Y is < -90 or > 90)
            throw new ArgumentOutOfRangeException(
                nameof(coordinate),
                "Plate declination must be between -90 and 90 degrees.");
        if (coordinate.Depth < 0)
            throw new ArgumentOutOfRangeException(
                nameof(coordinate),
                "Plate distance cannot be negative.");

        return new EquatorialCoordinates(
            coordinate.X,
            coordinate.Y,
            coordinate.Depth);
    }

    /// <summary>Projects equatorial coordinates around the north celestial pole.</summary>
    public static CelestialProjectedCoordinate ProjectPolarAzimuthal(
        EquatorialCoordinates coordinate)
    {
        var angleRadians = DegreesToRadians(coordinate.RightAscensionDegrees);
        var radius = 90d - coordinate.DeclinationDegrees;
        return new CelestialProjectedCoordinate(
            radius * global::System.Math.Sin(angleRadians),
            -radius * global::System.Math.Cos(angleRadians),
            coordinate.DistanceLightYears);
    }

    /// <summary>Inverts <see cref="ProjectPolarAzimuthal"/>.</summary>
    public static EquatorialCoordinates UnprojectPolarAzimuthal(
        CelestialProjectedCoordinate coordinate)
    {
        var radius = global::System.Math.Sqrt(
            coordinate.X * coordinate.X
            + coordinate.Y * coordinate.Y);
        if (radius > 180d + 1e-9)
        {
            throw new ArgumentOutOfRangeException(
                nameof(coordinate),
                "Polar azimuthal radius cannot exceed 180 degrees.");
        }
        if (coordinate.Depth < 0)
            throw new ArgumentOutOfRangeException(
                nameof(coordinate),
                "Polar azimuthal distance cannot be negative.");

        var rightAscension = radius <= 1e-12
            ? 0d
            : RadiansToDegrees(global::System.Math.Atan2(coordinate.X, -coordinate.Y));
        var declination = global::System.Math.Clamp(90d - radius, -90d, 90d);
        return new EquatorialCoordinates(
            rightAscension,
            declination,
            coordinate.Depth);
    }

    /// <summary>Converts equatorial coordinates to a Sol-centered J2000 Cartesian vector.</summary>
    public static StarCoords ToEquatorialCartesian(EquatorialCoordinates coordinate)
    {
        var rightAscensionRadians = DegreesToRadians(coordinate.RightAscensionDegrees);
        var declinationRadians = DegreesToRadians(coordinate.DeclinationDegrees);
        var cosDeclination = global::System.Math.Cos(declinationRadians);
        return new StarCoords(
            coordinate.DistanceLightYears
                * cosDeclination
                * global::System.Math.Cos(rightAscensionRadians),
            coordinate.DistanceLightYears
                * cosDeclination
                * global::System.Math.Sin(rightAscensionRadians),
            coordinate.DistanceLightYears
                * global::System.Math.Sin(declinationRadians));
    }

    /// <summary>
    /// Produces a monotonic radius where lower (brighter) apparent magnitudes
    /// have a larger rendered size.
    /// </summary>
    public static double RadiusForMagnitude(
        double? apparentMagnitude,
        double minimumRadiusPixels = 2,
        double maximumRadiusPixels = 8)
    {
        if (!double.IsFinite(minimumRadiusPixels) || minimumRadiusPixels <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumRadiusPixels));
        if (!double.IsFinite(maximumRadiusPixels)
            || maximumRadiusPixels < minimumRadiusPixels)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumRadiusPixels));
        }
        if (apparentMagnitude is { } magnitude && !double.IsFinite(magnitude))
            throw new ArgumentOutOfRangeException(nameof(apparentMagnitude));

        if (apparentMagnitude is null)
            return (minimumRadiusPixels + maximumRadiusPixels) / 2d;

        return global::System.Math.Clamp(
            4.5d - apparentMagnitude.Value * 0.35d,
            minimumRadiusPixels,
            maximumRadiusPixels);
    }

    static double DegreesToRadians(double degrees) => degrees * global::System.Math.PI / 180d;

    static double RadiansToDegrees(double radians) => radians * 180d / global::System.Math.PI;
}
