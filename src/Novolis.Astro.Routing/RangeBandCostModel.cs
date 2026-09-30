using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Routing;

/// <summary>
/// Stock hop cost model: the first band whose <see cref="RangeBand.MaxLy"/> covers the distance wins.
/// Prototype-compatible defaults: short band 10 ly @ 1.0×, long band 12 ly @ 3.0×.
/// </summary>
public sealed class RangeBandCostModel : IHopCostModel
{
    readonly IReadOnlyList<RangeBand> _bands;

    /// <summary>Creates a model from ordered bands (ascending MaxLy).</summary>
    public RangeBandCostModel(IReadOnlyList<RangeBand> bands)
    {
        ArgumentNullException.ThrowIfNull(bands);
        if (bands.Count == 0)
            throw new ArgumentException("At least one band is required.", nameof(bands));
        _bands = bands.OrderBy(b => b.MaxLy).ToList();
    }

    /// <summary>Prototype-compatible stock bands (10 ly @ 1×, 12 ly @ 3×).</summary>
    public static RangeBandCostModel CreatePrototypeCompatible() =>
        new([
            new RangeBand(10.0, 1.0, "short"),
            new RangeBand(12.0, 3.0, "long")
        ]);

    /// <inheritdoc />
    public HopEvaluation Evaluate(SystemId from, SystemId to, double distanceLy)
    {
        foreach (var band in _bands)
        {
            if (distanceLy <= band.MaxLy)
            {
                return new HopEvaluation(
                    Feasible: true,
                    Cost: distanceLy * band.CostPerLy,
                    DistanceLy: distanceLy,
                    BandTag: band.Tag);
            }
        }

        return new HopEvaluation(false, double.PositiveInfinity, distanceLy, null);
    }
}
