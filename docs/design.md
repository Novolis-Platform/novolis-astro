# Design

## Stack position

`Novolis.Astro.*` is a **domain family** for stellar catalogs and interstellar logistics abstractions. It is not part of Math → Physics → Simulation.

| Owns | Does not own |
|------|----------------|
| Catalog positions (ly/pc), hop graphs, route accumulation | Force/`dt` dynamics (Physics) |
| Pregenerated catalog packs (`Catalog.Data`) | Campaign fiction overlays / route maps |
| Pluggable hop cost and transit/speed profiles | Avalonia UI |
| Assessment scorers (Kopparapu HZ + habitability) | Product content packs |

## Catalog data

`Novolis.Astro.Catalog` is the store/query/import layer. `Novolis.Astro.Catalog.Data` ships frozen packs (`NearSol100`, `HygLocal1901`) as committed `*.g.cs` (refresh via `tools/pregen-catalog.cs`).

## Habitability

`StarSystem.EstimateHabitableZone` / `AssessHabitability` use Kopparapu et al. 2013 (erratum coeffs). Conservative = runaway greenhouse → maximum greenhouse.

## Routing

- `IHopCostModel` — feasibility + pathfinding cost (+ band tags)
- `ITransitProfile` — duration/resources (separate from cost)
- `RouteGraph` + `RoutePlanner` + `RouteAccumulation`

Stock `RangeBandCostModel.CreatePrototypeCompatible()` provides 10 ly @ 1× and 12 ly @ 3× bands for calibration against early tooling prototypes.
