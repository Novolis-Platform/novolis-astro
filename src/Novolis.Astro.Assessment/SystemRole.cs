namespace Novolis.Astro.Assessment;

/// <summary>Economic / settlement role for a star system on a hop graph.</summary>
public enum SystemRole
{
  /// <summary>Primary capital / export hub (typically Sol).</summary>
  Capital,

  /// <summary>Settled agri / consumer world.</summary>
  Inhabited,

  /// <summary>Manufacturing focus on an inhabited base.</summary>
  Industrial,

  /// <summary>Extractive / mining focus (often low agri).</summary>
  Mining,

  /// <summary>High-degree waypoint used as a transit junction.</summary>
  Transit,

  /// <summary>Default undeveloped hop node.</summary>
  Waypoint,
}
