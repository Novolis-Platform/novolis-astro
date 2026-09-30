using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class StrategicValueAssessorTests
{
    static StarSystem Sol() => new(
        "sol",
        "Sol",
        new StarCoords(0, 0, 0),
        SpectralClass.G);

    static StarSystem RemoteM() => new(
        "remote",
        "Remote",
        new StarCoords(50, 0, 0),
        SpectralClass.M);

    [Test]
    public async Task Sol_Rates_As_Hub()
    {
        var score = new StrategicValueAssessor().Assess(Sol());
        await Assert.That(score.Tier).IsEqualTo("hub");
        await Assert.That(score.Score).IsGreaterThanOrEqualTo(80.0);
    }

    [Test]
    public async Task RemoteM_Rates_As_Fringe()
    {
        var score = new StrategicValueAssessor().Assess(RemoteM());
        await Assert.That(score.Tier).IsEqualTo("fringe");
    }

    [Test]
    public async Task Assess_NullSystem_Throws()
    {
        await Assert.That(() => new StrategicValueAssessor().Assess(null!))
            .Throws<ArgumentNullException>();
    }
}
