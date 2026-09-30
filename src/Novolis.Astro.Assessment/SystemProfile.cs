using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Assessment;

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
