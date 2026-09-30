using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class SystemRoleInvariantsTests
{
    [Test]
    public async Task CollectFailures_Flags_Mining_And_Settlement_Violations()
    {
        var failures = SystemRoleInvariants.CollectFailures([
            ("mine-low", SystemRole.Mining, new SystemEconomicPotential(0.1, 0, 0, 0)),
            ("settle-zero", SystemRole.Inhabited, new SystemEconomicPotential(0, 0, 0, 0.5)),
            ("ok-mine", SystemRole.Mining, new SystemEconomicPotential(0.5, 0, 0, 0)),
        ]);

        await Assert.That(failures.Count).IsEqualTo(2);
        await Assert.That(failures[0]).Contains("mine-low");
        await Assert.That(failures[1]).Contains("settle-zero");
    }
}
