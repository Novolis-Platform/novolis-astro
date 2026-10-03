namespace Novolis.Astro.Catalog;

/// <summary>Sol-centered views available for a stellar catalog scene.</summary>
public enum CelestialProjectionKind
{
    /// <summary>Source-frame Cartesian view with vertical axis preserved as depth.</summary>
    SolCenteredCartesian,

    /// <summary>Right ascension and declination plate carrée view.</summary>
    EquatorialPlate,

    /// <summary>North-pole-centered azimuthal equidistant equatorial view.</summary>
    PolarAzimuthal,
}
