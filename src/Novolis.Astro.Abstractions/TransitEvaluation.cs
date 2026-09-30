namespace Novolis.Astro.Abstractions;

/// <summary>Result of evaluating transit timing/resources for a hop.</summary>
/// <param name="DurationSeconds">Travel duration in seconds.</param>
/// <param name="ResourceDelta">Signed resource change (fuel, stress, …); sign is consumer-defined.</param>
public readonly record struct TransitEvaluation(double DurationSeconds, double ResourceDelta);
