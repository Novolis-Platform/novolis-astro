using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

/// <summary>Assesses a catalog system for a consumer-defined facet.</summary>
public interface ISystemAssessor
{
    /// <summary>Facet name (e.g. habitability, strategic-value).</summary>
    string Facet { get; }

    /// <summary>Scores the system.</summary>
    AssessmentScore Assess(StarSystem system);
}
