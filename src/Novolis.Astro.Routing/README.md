<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-astro">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Astro.Routing

Interstellar route graphs with pluggable hop cost and transit profiles, Dijkstra planning, and route accumulation.

## Install

```bash
dotnet add package Novolis.Astro.Routing
```

## Quick start

```csharp
using Novolis.Astro.Abstractions;
using Novolis.Astro.Routing;

var cost = RangeBandCostModel.CreatePrototypeCompatible();
var graph = RouteGraph.Build(catalog.All, maxRangeLy: 12, cost);
var route = RoutePlanner.Find("sol", "proxima", graph,
    new ConstantSpeedTransitProfile(speedLyPerDay: 1));
```

## API

| Type | Role |
|------|------|
| `RouteGraph` | `Build(systems, maxRangeLy, costModel)` → adjacency of `RouteEdge` |
| `RoutePlanner` | `Find(fromId, toId, graph, transitProfile?)` → `RouteResult` |
| `RouteResult` | `Found`, `WaypointIds`, `Accumulation` |
| `RouteAccumulation` | `TotalLy`, `TotalCost`, `TotalDurationSeconds`, `CountsByBand` |
| `RangeBandCostModel` | Banded ly costs; `CreatePrototypeCompatible()` |
| `ConstantSpeedTransitProfile` | Fixed ly/day transit times |
| `RouteEdge` | `From`, `To`, `DistanceLy`, `Cost`, `BandTag` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Astro.Abstractions` | `IHopCostModel`, `ITransitProfile` contracts |
| `Novolis.Astro.Catalog` | `StarCatalog.All` input for graph build |
| `Novolis.Astro.Plotting` | Export `RouteResult` waypoints to SVG/TSV |

