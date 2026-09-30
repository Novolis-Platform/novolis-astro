using Novolis.Astro.Abstractions;

namespace Novolis.Astro.Routing;

/// <summary>Constant speed transit: duration = distance / speed.</summary>
public sealed class ConstantSpeedTransitProfile : ITransitProfile
{
    /// <summary>Creates a profile with speed in light-years per day.</summary>
    public ConstantSpeedTransitProfile(double speedLyPerDay)
    {
        if (speedLyPerDay <= 0)
            throw new ArgumentOutOfRangeException(nameof(speedLyPerDay));
        SpeedLyPerDay = speedLyPerDay;
    }

    /// <summary>Cruise speed in light-years per day.</summary>
    public double SpeedLyPerDay { get; }

    /// <inheritdoc />
    public TransitEvaluation Evaluate(SystemId from, SystemId to, double distanceLy, string? bandTag)
    {
        var days = distanceLy / SpeedLyPerDay;
        return new TransitEvaluation(days * 86400.0, ResourceDelta: 0);
    }
}
