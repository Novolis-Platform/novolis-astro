using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Overlay;

namespace Novolis.Astro.Unit;

public sealed class AbstractionsTests
{
    [Test]
    public async Task SystemId_ToString_And_Implicit_Conversions()
    {
        SystemId id = "alpha-centauri";
        await Assert.That(id.ToString()).IsEqualTo("alpha-centauri");

        string asString = id;
        await Assert.That(asString).IsEqualTo("alpha-centauri");

        SystemId roundTrip = asString;
        await Assert.That(roundTrip.Value).IsEqualTo("alpha-centauri");
    }

    [Test]
    public async Task StarCoords_DistanceFromOrigin_And_Distance()
    {
        var origin = new StarCoords(0, 0, 0);
        var point = new StarCoords(3, 4, 0);

        await Assert.That(origin.DistanceFromOrigin).IsEqualTo(0.0);
        await Assert.That(Math.Abs(point.DistanceFromOrigin - 5.0)).IsLessThan(1e-9);
        await Assert.That(Math.Abs(StarCoords.Distance(origin, point) - 5.0)).IsLessThan(1e-9);
        await Assert.That(Math.Abs(StarCoords.Distance(point, point))).IsLessThan(1e-9);
    }
}
