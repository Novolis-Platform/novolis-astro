<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start - embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Astro.Abstractions` | `dotnet add package Novolis.Astro.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Abstractions/README.md) |
| `Novolis.Astro.Catalog` | `dotnet add package Novolis.Astro.Catalog` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Catalog/README.md) |
| `Novolis.Astro.Routing` | `dotnet add package Novolis.Astro.Routing` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Routing/README.md) |
| `Novolis.Astro.Assessment` | `dotnet add package Novolis.Astro.Assessment` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Assessment/README.md) |
| `Novolis.Astro.Overlay` | `dotnet add package Novolis.Astro.Overlay` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Overlay/README.md) |
| `Novolis.Astro.Plotting` | `dotnet add package Novolis.Astro.Plotting` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Plotting/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->

# novolis-astro

**Stellar catalog, interstellar routing, assessment, overlays, and headless plotting** for games, simulations, and worldbuilding.

Orthogonal to Math → Physics → Simulation. May use thin `Novolis.Physics.Astro` unit helpers at the product layer; this repo stays ly/pc-oriented.

## Build

```powershell
dotnet build Novolis.Astro.slnx
dotnet test Novolis.Astro.slnx
dotnet pack Novolis.Astro.slnx -c Release -o artifacts/packages
```

Packages publish to **GitHub Packages** on merge to `main`.
