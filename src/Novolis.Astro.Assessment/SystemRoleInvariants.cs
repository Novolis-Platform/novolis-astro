namespace Novolis.Astro.Assessment;

/// <summary>Role ↔ economic-potential seed checks (host maps cohorts separately).</summary>
public static class SystemRoleInvariants
{
  /// <summary>
  /// Collects mining-threshold and settlement-agriculture failures for assigned hubs.
  /// </summary>
  public static IReadOnlyList<string> CollectFailures(
    IEnumerable<(string SystemId, SystemRole Role, SystemEconomicPotential Potential)> hubs,
    double miningThreshold = RoleAssigner.MiningThreshold)
  {
    var failures = new List<string>();

    foreach (var (systemId, role, potential) in hubs)
    {
      if (role == SystemRole.Mining && potential.Mining < miningThreshold)
      {
        failures.Add($"Mining hub {systemId} has Mining={potential.Mining:0.###} < {miningThreshold}");
      }

      if (role is SystemRole.Capital or SystemRole.Inhabited or SystemRole.Industrial)
      {
        if (potential.Agriculture <= 0)
        {
          failures.Add($"Settlement hub {systemId} ({role}) has Agriculture={potential.Agriculture:0.###}");
        }
      }
    }

    return failures;
  }
}
