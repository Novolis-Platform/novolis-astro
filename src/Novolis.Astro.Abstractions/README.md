# Novolis.Astro.Abstractions

Core stellar coordinates and hop/transit evaluation contracts.

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
