<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-astro/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-astro/) · [Source](https://github.com/Novolis-Platform/novolis-astro)
<!-- novolis-pkg-brand:end -->

# Novolis.Astro.Overlay

Campaign and worldbuilding overlays: bind fictional aliases and optional labels to canonical catalog system ids without duplicating stellar data.

## Install

```bash
dotnet add package Novolis.Astro.Overlay
```

## Quick start

```csharp
using Novolis.Astro.Abstractions;
using Novolis.Astro.Catalog;
using Novolis.Astro.Overlay;

var overlay = new CatalogOverlay();
overlay.Bind(new OverlayEntry("The Reach", "sol",
    new Dictionary<string, string> { ["faction"] = "Terran" }));

overlay.TryResolve("The Reach", out var systemId);
var errors = overlay.Validate(catalog);
```

## API

| Type | Role |
|------|------|
| `OverlayEntry` | Alias → `SystemId` binding with optional string labels |
| `CatalogOverlay` | `Bind`, `TryResolve`, `Validate`, `Entries` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Astro.Catalog` | Source `StarCatalog` for validation and lookup |
| `Novolis.Astro.Assessment` | Habitability/strategic scoring on resolved systems |

