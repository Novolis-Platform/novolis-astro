# Getting started

## Prerequisites

- .NET 10 SDK
- GitHub Packages credentials for `Novolis.*` restores when consuming published packages

## Local build

```powershell
dotnet build Novolis.Astro.slnx -c Release
dotnet test Novolis.Astro.slnx -c Release
```

Refresh catalog packs after editing `data/*.json`:

```powershell
dotnet run -c Release --file tools/pregen-catalog.cs
```

## Minimal route (pregenerated pack)

```csharp
using Novolis.Astro.Catalog.Data;
using Novolis.Astro.Routing;

var catalog = CatalogPacks.ToCatalog(CatalogPacks.NearSol100);
var graph = RouteGraph.Build(catalog.All, 12, RangeBandCostModel.CreatePrototypeCompatible());
var route = RoutePlanner.Find("sol", "proxima-centauri", graph);
```

## Habitability / Goldilocks zone

```csharp
using Novolis.Astro.Abstractions;
using Novolis.Astro.Assessment;
using Novolis.Astro.Catalog;

var sol = new StarSystem("sol", "Sol", new StarCoords(0, 0, 0), SpectralClass.G,
    spectralDesignation: "G2V", luminositySolar: 1.0, effectiveTemperatureK: 5780);
var zone = sol.EstimateHabitableZone();           // ~0.97–1.67 AU conservative
var rating = sol.AssessHabitability();            // Prime for Sol-like hosts
```
