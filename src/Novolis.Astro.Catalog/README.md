<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-astro">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

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

## API

| Type | Role |
|------|------|
| `StarSystem` | `Id`, `Name`, `Coords`, `SpectralClass`, luminosity/Teff tags |
| `StarCatalog` | `From`, `Add`, `TryGet`, `GetRequired`, `NeighborsWithin`, `All`, `Count` |
| `HygCsvImporter` | `Import(csvText, catalog)`, `Enumerate(reader)` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Astro.Catalog.Data` | Pregenerated `NearSol100`, `HygLocal1901` packs |
| `Novolis.Astro.Routing` | Build hop graphs from catalog systems |
| `Novolis.Astro.Overlay` | Alias bindings validated against catalog ids |

