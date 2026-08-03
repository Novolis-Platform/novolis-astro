<!-- novolis-marketing:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-brand-transparent.svg" width="360" alt="Novolis"/>
  </a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/banners/novolis-astro.svg" width="100%" alt="novolis-astro"/>
</p>

<p align="center">
  <strong>Stars, catalogs, assessment</strong><br/>
  Astronomical catalog, assessment, and related libraries for space sims.
</p>

<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-astro/actions"><img src="https://img.shields.io/github/actions/workflow/status/Novolis-Platform/novolis-astro/merge.yml?branch=main&label=merge&logo=github" alt="merge"/></a>
  <a href="https://github.com/orgs/Novolis-Platform/packages?repo_name=novolis-astro"><img src="https://img.shields.io/badge/packages-GitHub%20Packages-0a7ea3?logo=nuget" alt="packages"/></a>
  <a href="https://github.com/Novolis-Platform"><img src="https://img.shields.io/badge/org-Novolis--Platform-111827" alt="org"/></a>
</p>

<p align="center">
  <a href="https://nuget.pkg.github.com/Novolis-Platform/index.json"><code>https://nuget.pkg.github.com/Novolis-Platform/index.json</code></a>
  ·
  <a href="https://github.com/Novolis-Platform/.github/blob/main/profile/README.md">Org landing</a>
  ·
  <a href="https://github.com/Novolis-Platform/novolis-governance">Governance</a>
</p>

---
<!-- novolis-marketing:end -->
<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Astro.Abstractions` | `dotnet add package Novolis.Astro.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Abstractions/README.md) |
| `Novolis.Astro.Assessment` | `dotnet add package Novolis.Astro.Assessment` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Assessment/README.md) |
| `Novolis.Astro.Catalog` | `dotnet add package Novolis.Astro.Catalog` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Catalog/README.md) |
| `Novolis.Astro.Catalog.Data` | `dotnet add package Novolis.Astro.Catalog.Data` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Catalog.Data/README.md) |
| `Novolis.Astro.Overlay` | `dotnet add package Novolis.Astro.Overlay` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Overlay/README.md) |
| `Novolis.Astro.Plotting` | `dotnet add package Novolis.Astro.Plotting` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Plotting/README.md) |
| `Novolis.Astro.Routing` | `dotnet add package Novolis.Astro.Routing` | [README](https://github.com/Novolis-Platform/novolis-astro/blob/main/src/Novolis.Astro.Routing/README.md) |

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

