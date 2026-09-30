using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Overlay;

namespace Novolis.Astro.Unit;

public sealed class HygCsvImporterTests
{
    const string SampleCsv =
        """
        id,name,x,y,z,spect,lum,absmag,teff
        sol,Sol,0,0,0,G2V,1,4.83,5780
        wd,WD,1,0,0,DA,,,
        skip,,0,0,0,,,,
        bad,Bad,not-a-number,0,0,,,,
        """;

    [Test]
    public async Task Import_Parses_Valid_Rows()
    {
        var catalog = new StarCatalog();
        var count = HygCsvImporter.Import(SampleCsv, catalog);

        await Assert.That(count).IsEqualTo(3);
        await Assert.That(catalog.TryGet("sol", out var sol)).IsTrue();
        await Assert.That(sol!.SpectralClass).IsEqualTo(SpectralClass.G);
        await Assert.That(sol.LuminositySolar).IsEqualTo(1.0);
        await Assert.That(catalog.TryGet("wd", out var wd)).IsTrue();
        await Assert.That(wd!.SpectralClass).IsEqualTo(SpectralClass.WD);
    }

    [Test]
    public async Task Enumerate_Missing_Columns_Throws()
    {
        var reader = new StringReader("name,x,y\na,1,2,3");
        var act = () => HygCsvImporter.Enumerate(reader).ToList();
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Import_NullArguments_Throw()
    {
        var catalog = new StarCatalog();
        await Assert.That(() => HygCsvImporter.Import(null!, catalog)).Throws<ArgumentNullException>();
        await Assert.That(() => HygCsvImporter.Import("id,x,y,z\n", null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Enumerate_Empty_Reader_Yields_Nothing()
    {
        var reader = new StringReader("");
        await Assert.That(HygCsvImporter.Enumerate(reader).ToList()).IsEmpty();
    }
}
