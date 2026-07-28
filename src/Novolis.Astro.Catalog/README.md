# Novolis.Astro.Catalog

In-memory star system catalog with spatial neighbor queries and a minimal HYG-like CSV importer.

## Install

```bash
dotnet add package Novolis.Astro.Catalog
```

## Quick start

```csharp
using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;

var catalog = new StarCatalog();
catalog.Add(new StarSystem("sol", "Sol", new StarCoords(0, 0, 0), SpectralClass.G));
var near = catalog.NeighborsWithin(new StarCoords(0, 0, 0), radiusLy: 12);
```
