using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

namespace Novolis.Astro.Unit;

public sealed class StellarParameterResolverTests
{
    [Test]
    public async Task ParseDesignation_Recognizes_Luminosity_Classes()
    {
        var (g2vSpec, g2vSub, g2vLum, _) = StellarParameterResolver.ParseDesignation("G2V");
        await Assert.That(g2vSpec).IsEqualTo(SpectralClass.G);
        await Assert.That(g2vSub).IsEqualTo(2);
        await Assert.That(g2vLum).IsEqualTo(StellarParameterResolver.LuminosityClass.MainSequence);

        var (_, _, brightGiant, _) = StellarParameterResolver.ParseDesignation("K5III");
        await Assert.That(brightGiant).IsEqualTo(StellarParameterResolver.LuminosityClass.BrightGiant);

        var (_, _, subgiant, _) = StellarParameterResolver.ParseDesignation("G2IV");
        await Assert.That(subgiant).IsEqualTo(StellarParameterResolver.LuminosityClass.Subgiant);

        var (_, _, supergiant, isWd) = StellarParameterResolver.ParseDesignation("K2Ia");
        await Assert.That(supergiant).IsEqualTo(StellarParameterResolver.LuminosityClass.Supergiant);
        await Assert.That(isWd).IsFalse();
    }

    [Test]
    public async Task Resolve_Uses_AbsMag_When_Luminosity_Missing()
    {
        var system = new StarSystem(
            "a",
            "A",
            new StarCoords(1, 0, 0),
            SpectralClass.G,
            spectralDesignation: "G2V",
            absoluteMagnitude: 4.83);

        var resolved = StellarParameterResolver.Resolve(system);
        await Assert.That(resolved.LuminositySolar).IsNotNull();
        await Assert.That(resolved.LuminositySolar!.Value).IsGreaterThan(0);
        await Assert.That(resolved.HasSpectralDesignation).IsTrue();
    }

    [Test]
    public async Task EstimateTeffK_Covers_Brown_Dwarfs()
    {
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.L, null)).IsEqualTo(2000);
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.T, null)).IsEqualTo(1200);
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.Y, null)).IsEqualTo(600);
        await Assert.That(StellarParameterResolver.EstimateTeffK(SpectralClass.Unknown, null)).IsNull();
    }

    [Test]
    public async Task EstimateLuminositySolar_Scales_For_Giants()
    {
        var ms = StellarParameterResolver.EstimateLuminositySolar(
            SpectralClass.G, 2, StellarParameterResolver.LuminosityClass.MainSequence);
        var giant = StellarParameterResolver.EstimateLuminositySolar(
            SpectralClass.G, 2, StellarParameterResolver.LuminosityClass.Giant);

        await Assert.That(ms).IsNotNull();
        await Assert.That(giant).IsNotNull();
        await Assert.That(giant!.Value).IsGreaterThan(ms!.Value);
    }
}
