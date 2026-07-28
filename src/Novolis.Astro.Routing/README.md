# Novolis.Astro.Routing

Interstellar route graphs with pluggable hop cost and transit profiles, Dijkstra planning, and route accumulation.

## Install

```bash
dotnet add package Novolis.Astro.Routing
```

## Quick start

```csharp
using Novolis.Astro.Routing;

var cost = RangeBandCostModel.CreatePrototypeCompatible();
var graph = RouteGraph.Build(catalog.All, maxRangeLy: 12, cost);
var route = RoutePlanner.Find("sol", "proxima", graph, new ConstantSpeedTransitProfile(speedLyPerDay: 1));
```
