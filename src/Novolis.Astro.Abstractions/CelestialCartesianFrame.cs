namespace Novolis.Astro.Abstractions;

/// <summary>Reference frame used by Sol-relative Cartesian stellar coordinates.</summary>
public enum CelestialCartesianFrame
{
    /// <summary>Cartesian axes aligned with the Galactic coordinate system.</summary>
    Galactic,

    /// <summary>Cartesian axes aligned with equatorial J2000 coordinates.</summary>
    EquatorialJ2000,
}
