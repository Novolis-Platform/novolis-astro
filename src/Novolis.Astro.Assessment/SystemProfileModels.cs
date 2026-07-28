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

/// <summary>A generated system element with relative abundance in [0, 1].</summary>
public sealed record SystemElement(SystemElementKind Kind, double Abundance);

/// <summary>
/// Rolled-up economic potentials for a star system (each in [0, 1]).
/// Agriculture is forced to 0 when habitability is Excluded or Hostile.
/// </summary>
public sealed record SystemEconomicPotential(
    double Mining,
    double Volatiles,
    double Agriculture,
    double Industry);

/// <summary>
/// Deterministic system generation result: elements, potentials, and habitability.
/// Does not model in-system travel or locations — one system remains one travel unit.
/// </summary>
public sealed record SystemProfile(
    SystemId SystemId,
    ulong EffectiveSeed,
    IReadOnlyList<SystemElement> Elements,
    SystemEconomicPotential Potential,
    HabitabilityRating Habitability);
