using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;

namespace Novolis.Astro.Unit;

public sealed class CelestialProjectionTests
{
    [Test]
    public async Task Sol_centered_cartesian_projection_round_trips_all_axes()
    {
        var source = new StarCoords(4.5, -2.25, 7.75);

        var projected = CelestialProjections.ProjectSolCenteredCartesian(source);
        var roundTrip = CelestialProjections.UnprojectSolCenteredCartesian(projected);

        await Assert.That(roundTrip).IsEqualTo(source);
    }

    [Test]
    public async Task Equatorial_plate_round_trips_the_dateline_and_distance()
    {
        var source = new EquatorialCoordinates(359.75, -23.5, 12.25);

        var projected = CelestialProjections.ProjectEquatorialPlate(source);
        var roundTrip = CelestialProjections.UnprojectEquatorialPlate(projected);

        await Assert.That(roundTrip.RightAscensionDegrees)
            .IsEqualTo(source.RightAscensionDegrees)
            .Within(1e-12);
        await Assert.That(roundTrip.DeclinationDegrees)
            .IsEqualTo(source.DeclinationDegrees)
            .Within(1e-12);
        await Assert.That(roundTrip.DistanceLightYears)
            .IsEqualTo(source.DistanceLightYears)
            .Within(1e-12);
    }

    [Test]
    public async Task Polar_azimuthal_round_trips_pole_and_dateline_coordinates()
    {
        var pole = new EquatorialCoordinates(231, 90, 3);
        var dateline = new EquatorialCoordinates(359.5, -45, 18);

        var poleRoundTrip = CelestialProjections.UnprojectPolarAzimuthal(
            CelestialProjections.ProjectPolarAzimuthal(pole));
        var datelineRoundTrip = CelestialProjections.UnprojectPolarAzimuthal(
            CelestialProjections.ProjectPolarAzimuthal(dateline));

        await Assert.That(poleRoundTrip.DeclinationDegrees).IsEqualTo(90d).Within(1e-12);
        await Assert.That(poleRoundTrip.RightAscensionDegrees).IsEqualTo(0d);
        await Assert.That(datelineRoundTrip.RightAscensionDegrees)
            .IsEqualTo(dateline.RightAscensionDegrees)
            .Within(1e-12);
        await Assert.That(datelineRoundTrip.DeclinationDegrees)
            .IsEqualTo(dateline.DeclinationDegrees)
            .Within(1e-12);
        await Assert.That(datelineRoundTrip.DistanceLightYears)
            .IsEqualTo(dateline.DistanceLightYears)
            .Within(1e-12);
    }

    [Test]
    public async Task Projected_stars_retain_selection_coordinates_and_scale_by_magnitude()
    {
        var sol = CatalogPacks.HygLocal1901Entries.Single(entry => entry.Id == "0");
        var proxima = CatalogPacks.HygLocal1901Entries
            .Single(entry => entry.Name == "Proxima Centauri");

        var projectedSol = CelestialProjections.ProjectStar(
            sol,
            CelestialProjectionKind.SolCenteredCartesian);
        var projectedProxima = CelestialProjections.ProjectStar(
            proxima,
            CelestialProjectionKind.SolCenteredCartesian);

        await Assert.That(projectedSol.Id).IsEqualTo("0");
        await Assert.That(projectedSol.Coordinate.X).IsEqualTo(0d);
        await Assert.That(projectedSol.Coordinate.Y).IsEqualTo(0d);
        await Assert.That(projectedSol.Coordinate.Depth).IsEqualTo(0d);
        await Assert.That(projectedSol.RadiusPixels)
            .IsGreaterThan(projectedProxima.RadiusPixels);
        await Assert.That(projectedProxima.Coordinate.X)
            .IsEqualTo(proxima.SolRelativeCartesian.X)
            .Within(1e-12);
    }

    [Test]
    public async Task Magnitude_radius_is_monotonic_and_bounded()
    {
        var bright = CelestialProjections.RadiusForMagnitude(-1);
        var dim = CelestialProjections.RadiusForMagnitude(12);
        var unknown = CelestialProjections.RadiusForMagnitude(null);

        await Assert.That(bright).IsGreaterThan(dim);
        await Assert.That(dim).IsGreaterThanOrEqualTo(2d);
        await Assert.That(bright).IsLessThanOrEqualTo(8d);
        await Assert.That(unknown).IsEqualTo(5d);
    }
}
