using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

/// <summary>A scored assessment facet with optional tier label and reasons.</summary>
public sealed record AssessmentScore(double Score, string Tier, IReadOnlyList<string> Reasons);
