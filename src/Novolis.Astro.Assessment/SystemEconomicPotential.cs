using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Assessment;

/// <summary>
/// Rolled-up economic potentials for a star system (each in [0, 1]).
/// Agriculture is forced to 0 when habitability is Excluded or Hostile.
/// </summary>
public sealed record SystemEconomicPotential(
    double Mining,
    double Volatiles,
    double Agriculture,
    double Industry);
