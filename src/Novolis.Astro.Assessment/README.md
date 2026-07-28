# Novolis.Astro.Assessment

Pluggable system assessors plus Kopparapu-based habitable-zone and habitability rating.

## Habitable zone

Uses Kopparapu et al. (2013) erratum Table 3 coefficients (NASA/VPL calculator baseline):

- **Conservative:** runaway greenhouse → maximum greenhouse
- **Optimistic:** recent Venus → early Mars

\[
S_\mathrm{eff}(T_\mathrm{eff}) = S_\odot + a T_* + b T_*^2 + c T_*^3 + d T_*^4,\quad T_* = T_\mathrm{eff}-5780\,\mathrm{K}
\]

\[
d/\mathrm{AU} = \sqrt{L_*/S_\mathrm{eff}}
\]

Valid for \(2600\,\mathrm{K} \le T_\mathrm{eff} \le 7200\,\mathrm{K}\). Missing Teff/Lum fall back to spectral-type estimates (Pecaut & Mamajek–style main-sequence table) or \(L/L_\odot = 10^{0.4(4.83-M_V)}\).

## Habitability rating

`StarSystem.AssessHabitability()` blends stellar class, HZ geometry, evolutionary state, activity proxy, and data confidence (deterministic weights). Tiers: Excluded, Hostile, Marginal, Candidate, Favorable, Prime.

## System profile generation

`SystemProfileGenerator.Generate(system, campaignSeed)` deterministically rolls system **elements** (rocky, ice, gas giant, belt, volatiles) and rolled-up **economic potentials** (Mining, Volatiles, Agriculture, Industry) in `[0, 1]`. Agriculture is forced to `0` when habitability is `Excluded` or `Hostile`. Profiles do not model in-system travel — one system remains one travel unit for logistics hosts.

```csharp
var gen = new SystemProfileGenerator();
var profile = gen.Generate(sol, campaignSeed: 1001);
Console.WriteLine($"{profile.Potential.Mining:0.00} mining, {profile.Potential.Agriculture:0.00} agri");
```

## Install

```bash
dotnet add package Novolis.Astro.Assessment
```

```csharp
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;

var sol = new StarSystem("sol", "Sol", default, SpectralClass.G,
    spectralDesignation: "G2V", luminositySolar: 1, effectiveTemperatureK: 5780);
var zone = sol.EstimateHabitableZone();
var rating = sol.AssessHabitability();
```
