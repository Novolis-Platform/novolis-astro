using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Assessment;

/// <summary>Coarse system-body categories used for economic potential rollup.</summary>
public enum SystemElementKind
{
    /// <summary>Terrestrial / rocky world.</summary>
    RockyWorld = 0,

    /// <summary>Icy world or outer ice body.</summary>
    IceWorld,

    /// <summary>Gas giant.</summary>
    GasGiant,

    /// <summary>Asteroid / debris belt.</summary>
    AsteroidBelt,

    /// <summary>Cometary / volatile ice reservoir.</summary>
    VolatileReservoir
}
