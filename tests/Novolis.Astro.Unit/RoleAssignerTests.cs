using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class RoleAssignerTests
{
    private const ulong CampaignSeed = 1001;

    [Test]
    public async Task Assign_NearSol100_Produces_Expected_Census()
    {
        var catalog = CatalogPacks.ToCatalog(CatalogPacks.NearSol100);
        var graph = RouteGraph.Build(catalog.All, maxRangeLy: 12, RangeBandCostModel.CreatePrototypeCompatible());
        var gen = new SystemProfileGenerator();
        var profiles = catalog.All.ToDictionary(
            s => s.Id.Value,
            s => gen.Generate(s, CampaignSeed));

        var roles = RoleAssigner.Assign(catalog, graph, profiles);

        await Assert.That(roles["sol"]).IsEqualTo(SystemRole.Capital);
        await Assert.That(roles.Values.Count(r => r == SystemRole.Inhabited)).IsGreaterThan(0);

        var summary = RoleAssigner.Summarize(roles);
        await Assert.That(summary.StartsWith("C1", StringComparison.Ordinal)).IsTrue();

        var hubs = roles.Select(kvp =>
        {
            profiles.TryGetValue(kvp.Key, out var profile);
            return (kvp.Value, profile!.Potential);
        });
        var potentialSummary = RoleAssigner.SummarizePotentials(hubs);
        await Assert.That(potentialSummary).Contains("miningHubs=");
    }
}
