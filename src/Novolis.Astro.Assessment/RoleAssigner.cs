using Novolis.Astro.Catalog;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Assessment;

/// <summary>Deterministic role assignment from <see cref="SystemProfile"/> potentials + graph degree.</summary>
public static class RoleAssigner
{
  /// <summary>Target inhabited (non-Sol) count.</summary>
  public const int TargetInhabited = 16;

  /// <summary>Target industrial count.</summary>
  public const int TargetIndustrial = 5;

  /// <summary>Target mining count.</summary>
  public const int TargetMining = 10;

  /// <summary>Target transit junction count.</summary>
  public const int TargetTransit = 20;

  /// <summary>Minimum agriculture potential for inhabited selection.</summary>
  public const double AgricultureThreshold = 0.35;

  /// <summary>Minimum industry potential for industrial selection.</summary>
  public const double IndustryThreshold = 0.35;

  /// <summary>Minimum mining potential for mining selection.</summary>
  public const double MiningThreshold = 0.35;

  /// <summary>
  /// Assigns one role per catalog system. Sol is always <see cref="SystemRole.Capital"/>.
  /// </summary>
  public static IReadOnlyDictionary<string, SystemRole> Assign(
    StarCatalog catalog,
    RouteGraph graph,
    IReadOnlyDictionary<string, SystemProfile> profiles)
  {
    var systems = catalog.All.ToList();
    var roles = new Dictionary<string, SystemRole>(StringComparer.OrdinalIgnoreCase);

    foreach (var s in systems)
    {
      roles[s.Id.Value] = SystemRole.Waypoint;
    }

    roles["sol"] = SystemRole.Capital;

    var inhabited = systems
      .Where(s => !s.Id.Value.Equals("sol", StringComparison.OrdinalIgnoreCase))
      .Where(s =>
      {
        var p = profiles[s.Id.Value].Potential;
        return p.Agriculture >= AgricultureThreshold;
      })
      .OrderByDescending(s => profiles[s.Id.Value].Potential.Agriculture)
      .ThenBy(s => s.Id.Value, StringComparer.Ordinal)
      .Take(TargetInhabited)
      .ToList();

    foreach (var s in inhabited)
    {
      roles[s.Id.Value] = SystemRole.Inhabited;
    }

    var industrialPool = systems
      .Where(s => roles[s.Id.Value] is SystemRole.Inhabited or SystemRole.Capital
        || profiles[s.Id.Value].Potential.Agriculture > 0)
      .Where(s => !s.Id.Value.Equals("sol", StringComparison.OrdinalIgnoreCase))
      .Where(s => profiles[s.Id.Value].Potential.Industry >= IndustryThreshold)
      .Where(s => profiles[s.Id.Value].Potential.Agriculture > 0)
      .OrderByDescending(s => profiles[s.Id.Value].Potential.Industry)
      .ThenBy(s => s.Id.Value, StringComparer.Ordinal)
      .Take(TargetIndustrial)
      .ToList();

    foreach (var s in industrialPool)
    {
      roles[s.Id.Value] = SystemRole.Industrial;
    }

    if (!industrialPool.Any() && inhabited.Count > 0)
    {
      roles[inhabited[0].Id.Value] = SystemRole.Industrial;
    }

    var mining = systems
      .Where(s => roles[s.Id.Value] is SystemRole.Waypoint or SystemRole.Transit)
      .Where(s => profiles[s.Id.Value].Potential.Mining >= MiningThreshold)
      .OrderByDescending(s => profiles[s.Id.Value].Potential.Mining)
      .ThenBy(s => s.Coords.DistanceFromOrigin)
      .ThenBy(s => s.Id.Value, StringComparer.Ordinal)
      .Take(TargetMining)
      .ToList();

    foreach (var s in mining)
    {
      roles[s.Id.Value] = SystemRole.Mining;
    }

    var degree = systems.ToDictionary(
      s => s.Id.Value,
      s => graph.Adjacency.TryGetValue(s.Id.Value, out var edges) ? edges.Count : 0,
      StringComparer.OrdinalIgnoreCase);

    var transit = systems
      .Where(s => roles[s.Id.Value] == SystemRole.Waypoint)
      .Select(s =>
      {
        var deg = degree[s.Id.Value];
        var industry = profiles[s.Id.Value].Potential.Industry;
        var score = industry * 100.0 + deg * 2.0;
        return (System: s, Score: score);
      })
      .OrderByDescending(x => x.Score)
      .ThenBy(x => x.System.Id.Value, StringComparer.Ordinal)
      .Take(TargetTransit)
      .ToList();

    foreach (var t in transit)
    {
      roles[t.System.Id.Value] = SystemRole.Transit;
    }

    return roles;
  }

  /// <summary>Compact role census string (C/I/Ind/M/T/W counts).</summary>
  public static string Summarize(IReadOnlyDictionary<string, SystemRole> roles)
  {
    string C(SystemRole r) => roles.Values.Count(v => v == r).ToString();
    return $"C{C(SystemRole.Capital)} I{C(SystemRole.Inhabited)} Ind{C(SystemRole.Industrial)} M{C(SystemRole.Mining)} T{C(SystemRole.Transit)} W{C(SystemRole.Waypoint)}";
  }

  /// <summary>Compact potential hub census from role + potential pairs.</summary>
  public static string SummarizePotentials(
    IEnumerable<(SystemRole Role, SystemEconomicPotential Potential)> hubs)
  {
    var list = hubs as IList<(SystemRole Role, SystemEconomicPotential Potential)> ?? hubs.ToList();
    var mining = list.Count(h => h.Role == SystemRole.Mining);
    var agri = list.Count(h =>
      h.Role is SystemRole.Capital or SystemRole.Inhabited or SystemRole.Industrial);
    var barren = list.Count(h => h.Potential.Agriculture == 0);
    return $"miningHubs={mining} agriHubs={agri} barrenAgri0={barren}";
  }
}
