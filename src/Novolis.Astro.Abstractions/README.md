<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-astro">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Astro.Abstractions

Core stellar coordinates and hop/transit evaluation contracts shared by catalog, routing, and assessment packages.

## Install

```bash
dotnet add package Novolis.Astro.Abstractions
```

## Quick start

```csharp
using Novolis.Astro.Abstractions;

var a = new StarCoords(0, 0, 0);
var b = new StarCoords(4.3, 0, 0);
var ly = StarCoords.Distance(a, b);
```

## API

| Type | Role |
|------|------|
| `SystemId` | Opaque catalog key (`string` ↔ implicit conversion) |
| `StarCoords` | Galactic XYZ in light-years; `Distance`, `DistanceFromOrigin` |
| `SpectralClass` | Stellar class enum (`G`, `K`, `M`, …) |
| `HopEvaluation` | Feasibility, cost, distance, band tag from a hop model |
| `TransitEvaluation` | Duration and resource delta from a transit profile |
| `IHopCostModel` | `Evaluate(from, to, distanceLy)` |
| `ITransitProfile` | `Evaluate(from, to, distanceLy, bandTag?)` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Astro.Catalog` | `StarSystem` records and spatial queries |
| `Novolis.Astro.Routing` | Stock cost models and Dijkstra planning |

