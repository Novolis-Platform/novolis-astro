namespace Novolis.Astro.Assessment;

/// <summary>Estimated circumstellar habitable zone in astronomical units.</summary>
public sealed record HabitableZone(
    double InnerAu,
    double OuterAu,
    double TeffK,
    double LuminositySolar,
    HabitableZoneConvention Convention,
    HabitableZoneLimit InnerLimit,
    HabitableZoneLimit OuterLimit)
{
    /// <summary>Outer − inner (AU).</summary>
    public double WidthAu => OuterAu - InnerAu;

    /// <summary>Midpoint of the zone (AU).</summary>
    public double MidAu => (InnerAu + OuterAu) * 0.5;
}
