using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

namespace Novolis.Astro.Assessment;

/// <summary>
/// Deterministic generator of <see cref="SystemProfile"/> from catalog star metadata.
/// Mixes a campaign seed with the system id; same inputs always yield the same profile.
/// </summary>
public sealed class SystemProfileGenerator
{
    /// <summary>Creates a generator using the given habitable-zone convention for agri gating.</summary>
    public SystemProfileGenerator(HabitableZoneConvention convention = HabitableZoneConvention.Conservative) =>
        Convention = convention;

    /// <summary>HZ convention used when assessing habitability.</summary>
    public HabitableZoneConvention Convention { get; }

    /// <summary>Generates an immutable system profile for <paramref name="system"/>.</summary>
    public SystemProfile Generate(StarSystem system, ulong campaignSeed)
    {
        ArgumentNullException.ThrowIfNull(system);

        var effectiveSeed = MixSeed(campaignSeed, system);
        var rng = new SplitMix64(effectiveSeed);
        var habitability = system.AssessHabitability(Convention);
        var elements = RollElements(system.SpectralClass, ref rng);
        elements = EnsureHabitableRocky(elements, habitability, ref rng);
        var potential = DerivePotential(elements, habitability);
        return new SystemProfile(system.Id, effectiveSeed, elements, potential, habitability);
    }

    private static ulong MixSeed(ulong campaignSeed, StarSystem system)
    {
        var h = campaignSeed == 0 ? 0x9E3779B97F4A7C15UL : campaignSeed;
        h = SplitMix64.Next(h ^ HashString(system.Id.Value));
        h = SplitMix64.Next(h ^ (ulong)(uint)system.SpectralClass);
        if (system.LuminositySolar is { } lum)
        {
            h = SplitMix64.Next(h ^ DoubleBits(lum));
        }

        if (system.EffectiveTemperatureK is { } teff)
        {
            h = SplitMix64.Next(h ^ DoubleBits(teff));
        }

        return h == 0 ? 0xBF58476D1CE4E5B9UL : h;
    }

    private static List<SystemElement> RollElements(SpectralClass spectral, ref SplitMix64 rng)
    {
        var elements = new List<SystemElement>(5);
        var (rockyBias, iceBias, gasBias, beltBias, volatileBias) = SpectralBiases(spectral);

        MaybeAdd(elements, SystemElementKind.RockyWorld, rockyBias, ref rng);
        MaybeAdd(elements, SystemElementKind.IceWorld, iceBias, ref rng);
        MaybeAdd(elements, SystemElementKind.GasGiant, gasBias, ref rng);
        MaybeAdd(elements, SystemElementKind.AsteroidBelt, beltBias, ref rng);
        MaybeAdd(elements, SystemElementKind.VolatileReservoir, volatileBias, ref rng);

        return elements;
    }

    private static List<SystemElement> EnsureHabitableRocky(
        List<SystemElement> elements,
        HabitabilityRating habitability,
        ref SplitMix64 rng)
    {
        if (habitability.Tier is HabitabilityTier.Excluded or HabitabilityTier.Hostile)
        {
            return elements;
        }

        if (habitability.Tier >= HabitabilityTier.Candidate
            && elements.All(e => e.Kind != SystemElementKind.RockyWorld))
        {
            // Candidate+ hosts always have at least one rocky body for agri potential.
            elements.Add(new SystemElement(SystemElementKind.RockyWorld, 0.35 + rng.NextDouble() * 0.35));
        }

        if (elements.Count == 0)
        {
            // Degenerate hosts still get a thin belt so mining potential can be non-zero.
            elements.Add(new SystemElement(SystemElementKind.AsteroidBelt, 0.15 + rng.NextDouble() * 0.25));
        }

        return elements;
    }

    private static void MaybeAdd(
        List<SystemElement> elements,
        SystemElementKind kind,
        double presenceChance,
        ref SplitMix64 rng)
    {
        if (rng.NextDouble() > presenceChance)
        {
            return;
        }

        var abundance = Clamp01(0.2 + rng.NextDouble() * 0.8 * presenceChance);
        elements.Add(new SystemElement(kind, abundance));
    }

    private static (double Rocky, double Ice, double Gas, double Belt, double Volatile) SpectralBiases(
        SpectralClass spectral) =>
        spectral switch
        {
            SpectralClass.G or SpectralClass.F => (0.85, 0.45, 0.70, 0.55, 0.40),
            SpectralClass.K => (0.75, 0.55, 0.55, 0.65, 0.50),
            SpectralClass.M => (0.40, 0.70, 0.35, 0.80, 0.65),
            SpectralClass.A or SpectralClass.B => (0.35, 0.30, 0.75, 0.50, 0.35),
            SpectralClass.O => (0.15, 0.20, 0.60, 0.40, 0.25),
            SpectralClass.L or SpectralClass.T or SpectralClass.Y => (0.20, 0.75, 0.15, 0.55, 0.70),
            SpectralClass.WD or SpectralClass.NS or SpectralClass.BH => (0.10, 0.25, 0.05, 0.70, 0.30),
            _ => (0.50, 0.45, 0.45, 0.50, 0.40),
        };

    private static SystemEconomicPotential DerivePotential(
        IReadOnlyList<SystemElement> elements,
        HabitabilityRating habitability)
    {
        double Abundance(SystemElementKind kind) =>
            elements.Where(e => e.Kind == kind).Sum(e => e.Abundance);

        var rocky = Abundance(SystemElementKind.RockyWorld);
        var ice = Abundance(SystemElementKind.IceWorld);
        var gas = Abundance(SystemElementKind.GasGiant);
        var belt = Abundance(SystemElementKind.AsteroidBelt);
        var volatiles = Abundance(SystemElementKind.VolatileReservoir);

        var mining = Clamp01(0.55 * belt + 0.45 * rocky);
        var volatilePotential = Clamp01(0.40 * ice + 0.30 * gas + 0.45 * volatiles);
        var industry = Clamp01(0.45 * rocky + 0.35 * gas + 0.15 * (habitability.Score / 100.0));

        double agriculture;
        if (habitability.Tier is HabitabilityTier.Excluded or HabitabilityTier.Hostile)
        {
            agriculture = 0;
        }
        else
        {
            var hzProxy = rocky * (habitability.Score / 100.0);
            agriculture = Clamp01(hzProxy);
        }

        return new SystemEconomicPotential(mining, volatilePotential, agriculture, industry);
    }

    private static double Clamp01(double value) => Math.Clamp(value, 0.0, 1.0);

    private static ulong HashString(string value)
    {
        // FNV-1a 64-bit over UTF-16 code units (stable across runtimes).
        const ulong Offset = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        var hash = Offset;
        foreach (var c in value)
        {
            hash ^= c;
            hash *= Prime;
        }

        return hash;
    }

    private static ulong DoubleBits(double value) =>
        unchecked((ulong)BitConverter.DoubleToInt64Bits(value));

    /// <summary>SplitMix64 PRNG for deterministic rolls (local; no Economy dependency).</summary>
    private struct SplitMix64
    {
        private ulong _state;

        public SplitMix64(ulong seed) => _state = seed;

        public double NextDouble() => (NextUInt64() >> 11) * (1.0 / (1UL << 53));

        public ulong NextUInt64()
        {
            _state = Next(_state);
            return _state;
        }

        public static ulong Next(ulong z)
        {
            z += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}
