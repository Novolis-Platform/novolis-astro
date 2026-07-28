# Design

## Stack position

`Novolis.Astro.*` is a **domain family** for stellar catalogs and interstellar logistics abstractions. It is not part of Math → Physics → Simulation.

| Owns | Does not own |
|------|----------------|
| Catalog positions (ly/pc), hop graphs, route accumulation | Force/`dt` dynamics (Physics) |
| Pluggable hop cost and transit/speed profiles | Avalonia UI |
| Assessment scorers and campaign overlays | Product content packs |

## Routing

- `IHopCostModel` — feasibility + pathfinding cost (+ band tags)
- `ITransitProfile` — duration/resources (separate from cost)
- `RouteGraph` + `RoutePlanner` + `RouteAccumulation`

Stock `RangeBandCostModel.CreatePrototypeCompatible()` provides 10 ly @ 1× and 12 ly @ 3× bands for calibration against early tooling prototypes.
