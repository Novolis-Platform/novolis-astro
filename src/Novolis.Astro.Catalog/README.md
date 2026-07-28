# Novolis.Astro.Catalog

In-memory star system catalog with spatial neighbor queries and a HYG-like CSV importer.

## Install

```bash
dotnet add package Novolis.Astro.Catalog
```

For pregenerated packs, also add `Novolis.Astro.Catalog.Data`.

## Quick start

```csharp
using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

var catalog = StarCatalog.From([
    new StarSystem("sol", "Sol", new StarCoords(0, 0, 0), SpectralClass.G)
]);
var near = catalog.NeighborsWithin(new StarCoords(0, 0, 0), radiusLy: 12);
```

`All` is an ordered `IReadOnlyList<StarSystem>`. Prefer `StarCatalog.From(...)` for stable pack order.
