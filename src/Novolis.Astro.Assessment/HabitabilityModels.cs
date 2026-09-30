namespace Novolis.Astro.Assessment;

/// <summary>Kopparapu et al. (2013) habitable-zone flux limits.</summary>
public enum HabitableZoneLimit
{
    /// <summary>Optimistic inner edge (empirical).</summary>
    RecentVenus = 0,
    /// <summary>Conservative inner edge (1D climate model).</summary>
    RunawayGreenhouse,
    /// <summary>Moist greenhouse (water-loss) inner edge.</summary>
    MoistGreenhouse,
    /// <summary>Conservative outer edge.</summary>
    MaximumGreenhouse,
    /// <summary>Optimistic outer edge (empirical).</summary>
    EarlyMars
}
