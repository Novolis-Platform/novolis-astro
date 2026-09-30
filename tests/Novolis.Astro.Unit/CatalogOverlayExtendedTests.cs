using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Overlay;

namespace Novolis.Astro.Unit;

public sealed class CatalogOverlayExtendedTests
{
    [Test]
    public async Task Bind_Empty_Alias_Throws()
    {
        var overlay = new CatalogOverlay();
        var act = () => overlay.Bind(new OverlayEntry("  ", "sol"));
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Entries_Reflects_Bindings()
    {
        var overlay = new CatalogOverlay();
        overlay.Bind(new OverlayEntry("Home", "a"));
        overlay.Bind(new OverlayEntry("Away", "b"));

        await Assert.That(overlay.Entries.Count).IsEqualTo(2);
        await Assert.That(overlay.TryResolve("Away", out var id)).IsTrue();
        await Assert.That(id.Value).IsEqualTo("b");
    }

    [Test]
    public async Task Validate_NullCatalog_Throws()
    {
        var overlay = new CatalogOverlay();
        await Assert.That(() => overlay.Validate(null!)).Throws<ArgumentNullException>();
    }
}
