# Novolis.Astro.Plotting

Headless orthographic XZ projection and SVG/TSV export for stellar routes — no UI dependencies.

## Install

```bash
dotnet add package Novolis.Astro.Plotting
```

## Quick start

```csharp
using Novolis.Astro.Abstractions;
using Novolis.Astro.Plotting;

var waypoints = new List<StarCoords> { /* route systems */ };
var svg = PathSvgExporter.Export(waypoints, width: 1024, height: 768);
var tsv = PathTsvExporter.Export(waypoints);
File.WriteAllText("route.svg", svg);
```

## API

| Type | Role |
|------|------|
| `OrthographicProjector` | `Project(StarCoords)` → `(U, V)` on the XZ plane |
| `PathSvgExporter` | `Export(waypoints, width?, height?, margin?)` → SVG polyline |
| `PathTsvExporter` | `Export(waypoints)` → TSV with `index`, `x_ly`, `y_ly`, `z_ly` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Astro.Routing` | Produce waypoint lists from Dijkstra planning |
| `Novolis.Avalonia.StarMap` | Interactive Avalonia star map (dogfood: StarMapLab) |
