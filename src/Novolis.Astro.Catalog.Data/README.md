# Novolis.Astro.Catalog.Data

Pregenerated stellar catalog packs as stable `IReadOnlyList<StarSystem>` sequences.

## Packs

| Property | Contents |
|----------|----------|
| `CatalogPacks.NearSol100` | 100 nearest systems (Johnston galactic XYZ, ly) |
| `CatalogPacks.HygLocal1901` | Local HYG-style slice (~1901); XYZ converted pc→ly |

## Refresh

After updating files under `novolis-astro/data/`:

```bash
dotnet run -c Release --file tools/pregen-catalog.cs
```

Commit the regenerated `src/Novolis.Astro.Catalog.Data/Generated/*.g.cs` files.

## Install

```bash
dotnet add package Novolis.Astro.Catalog.Data
```

```csharp
using Novolis.Astro.Catalog.Data;

var catalog = CatalogPacks.ToCatalog(CatalogPacks.NearSol100);
```
