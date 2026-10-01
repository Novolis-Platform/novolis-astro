<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-astro/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-astro/) · [Source](https://github.com/Novolis-Platform/novolis-astro)
<!-- novolis-pkg-brand:end -->

# Novolis.Astro.Catalog.Data

Pregenerated stellar catalog packs as stable `IReadOnlyList<StarSystem>` sequences.

## Install

```bash
dotnet add package Novolis.Astro.Catalog.Data
```

## Quick start

```csharp
using Novolis.Astro.Catalog;
using Novolis.Astro.Catalog.Data;

var catalog = CatalogPacks.ToCatalog(CatalogPacks.NearSol100);
```

## Packs

| Property | Contents |
|----------|----------|
| `CatalogPacks.NearSol100` | 100 nearest systems (Johnston galactic XYZ, ly) |
| `CatalogPacks.HygLocal1901` | Local HYG-style slice (~1901); XYZ converted pc→ly |

## API

| Type | Role |
|------|------|
| `CatalogPacks` | `NearSol100`, `HygLocal1901` lists; `ToCatalog(pack)` → `StarCatalog` |

## Refresh

After updating files under `novolis-astro/data/`:

```bash
dotnet run -c Release --file tools/pregen-catalog.cs
```

Commit the regenerated `src/Novolis.Astro.Catalog.Data/Generated/*.g.cs` files.

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Astro.Catalog` | Runtime catalog queries and CSV import |
| `Novolis.Astro.Routing` | Route planning over pack systems |

