using System.Text.Json;
using System.Text.Json.Serialization;
using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog.Data;

namespace Novolis.Astro.Unit;

public sealed class CatalogPackEnrichmentTests
{
    static readonly JsonSerializerOptions SnapshotJson = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Test]
    public async Task Generated_packs_preserve_provenance_and_normalize_Sol_exactly()
    {
        foreach (var pack in Enum.GetValues<CatalogPackId>())
        {
            var entries = CatalogPacks.GetEntries(pack);
            var provenance = CatalogPacks.GetProvenance(pack);
            var sol = entries.Single(entry => entry.SourceId == provenance.SolSourceId);

            await Assert.That(entries).Count().IsEqualTo(provenance.RecordCount);
            await Assert.That(provenance.IsSolNormalized).IsTrue();
            await Assert.That(sol.SolRelativeCartesian.X).IsEqualTo(0d);
            await Assert.That(sol.SolRelativeCartesian.Y).IsEqualTo(0d);
            await Assert.That(sol.SolRelativeCartesian.Z).IsEqualTo(0d);
            await Assert.That(entries.All(entry =>
                    double.IsFinite(entry.Equatorial.RightAscensionDegrees)
                    && double.IsFinite(entry.Equatorial.DeclinationDegrees)
                    && double.IsFinite(entry.Equatorial.DistanceLightYears)))
                .IsTrue();
        }

        await Assert.That(CatalogPacks.NearSol100Provenance.CartesianFrame)
            .IsEqualTo(CelestialCartesianFrame.Galactic);
        await Assert.That(CatalogPacks.HygLocal1901Provenance.CartesianFrame)
            .IsEqualTo(CelestialCartesianFrame.EquatorialJ2000);
    }

    [Test]
    public async Task Hyg_entries_preserve_equatorial_source_fields_and_magnitude()
    {
        var proxima = CatalogPacks.HygLocal1901Entries
            .Single(entry => entry.Name == "Proxima Centauri");

        await Assert.That(proxima.SourceId).IsEqualTo("70666");
        await Assert.That(proxima.Equatorial.RightAscensionDegrees)
            .IsEqualTo(217.439775d).Within(1e-6);
        await Assert.That(proxima.Equatorial.DeclinationDegrees)
            .IsEqualTo(-62.679485d).Within(1e-6);
        await Assert.That(proxima.ApparentMagnitude).IsEqualTo(11.01d);
        await Assert.That(proxima.SpectralDesignation).IsEqualTo("M5Ve");
    }

    [Test]
    public async Task Embedded_ndjson_snapshots_round_trip_the_generated_entry_contract()
    {
        foreach (var pack in Enum.GetValues<CatalogPackId>())
        {
            using var stream = CatalogPacks.OpenNdjson(pack);
            using var reader = new StreamReader(stream);
            var entries = new List<CelestialCatalogEntry>();
            while (await reader.ReadLineAsync() is { } line)
            {
                var entry = JsonSerializer.Deserialize<CelestialCatalogEntry>(line, SnapshotJson);
                await Assert.That(entry).IsNotNull();
                entries.Add(entry!);
            }

            var generated = CatalogPacks.GetEntries(pack);
            await Assert.That(entries).Count().IsEqualTo(generated.Count);
            await Assert.That(entries[0].Id).IsEqualTo(generated[0].Id);
            await Assert.That(entries[0].SolRelativeCartesian)
                .IsEqualTo(generated[0].SolRelativeCartesian);
            await Assert.That(entries[^1].Id).IsEqualTo(generated[^1].Id);
        }
    }
}
