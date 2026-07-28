# Getting started

## Prerequisites

- .NET 10 SDK
- GitHub Packages credentials for `Novolis.*` restores when consuming published packages

## Local build

```powershell
dotnet build Novolis.Astro.slnx -c Release
dotnet test Novolis.Astro.slnx -c Release
```

## Minimal route

```csharp
using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Routing;

var catalog = new StarCatalog();
catalog.Add(new StarSystem("a", "A", new StarCoords(0, 0, 0)));
catalog.Add(new StarSystem("b", "B", new StarCoords(8, 0, 0)));
var graph = RouteGraph.Build(catalog.All, 12, RangeBandCostModel.CreatePrototypeCompatible());
var route = RoutePlanner.Find("a", "b", graph);
```
