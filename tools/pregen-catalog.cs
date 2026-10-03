// Pregenerates Catalog.Data C# packs and packageable NDJSON snapshots.
// Run from novolis-astro repo root:
//   dotnet run -c Release --file tools/pregen-catalog.cs

#:property PublishAot=false
#:property PackAsTool=false
#:property Nullable=enable
#:property ImplicitUsings=enable
#:property TargetFramework=net10.0

#pragma warning disable NOV2201 // File-based generator keeps its private schema beside the transformation.

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var root = FindRepoRoot(Environment.CurrentDirectory);
var dataDir = Path.Combine(root, "data");
var outDir = Path.Combine(root, "src", "Novolis.Astro.Catalog.Data", "Generated");
Directory.CreateDirectory(outDir);

const double LyPerPc = 3.261563777;
const string NearSolSource = "Johnston near-Sol (ly); vendored from nearsol-100.json";
const string HygSource = "HYG-style local slice; source XYZ converted pc to ly from hyg-local.json";

var nearSolPath = Path.Combine(dataDir, "nearsol-100.json");
var hygPath = Path.Combine(dataDir, "hyg-local.json");
if (!File.Exists(nearSolPath) || !File.Exists(hygPath))
{
    Console.Error.WriteLine("Missing data/nearsol-100.json or data/hyg-local.json");
    return 1;
}

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

var nearSol = JsonSerializer.Deserialize<List<NearSolEntry>>(
    File.ReadAllText(nearSolPath),
    jsonOptions) ?? [];
var nearSolSol = nearSol.SingleOrDefault(
    entry => string.Equals(entry.Id, "sol", StringComparison.OrdinalIgnoreCase))
    ?? throw new InvalidOperationException(
        "The Near-Sol source must contain a 'sol' row for origin normalization.");
var nearSystems = nearSol
    .Select(entry =>
    {
        var normalized = (
            X: entry.X - nearSolSol.X,
            Y: entry.Y - nearSolSol.Y,
            Z: entry.Z - nearSolSol.Z);
        var equatorial = EquatorialFromGalactic(
            normalized.X,
            normalized.Y,
            normalized.Z);
        return new GenSystem(
            entry.Id,
            entry.Id,
            entry.Name,
            "Galactic",
            normalized.X,
            normalized.Y,
            normalized.Z,
            equatorial.RightAscensionDegrees,
            equatorial.DeclinationDegrees,
            equatorial.DistanceLightYears,
            null,
            null,
            ParseSpectralClass(entry.Spectral),
            entry.Spectral,
            null,
            entry.Tags ?? []);
    })
    .OrderBy(DistanceOf)
    .ThenBy(system => system.Id, StringComparer.Ordinal)
    .ToList();

WritePack(
    Path.Combine(outDir, "NearSol100.g.cs"),
    "NearSol100",
    nearSystems,
    "data/nearsol-100.json",
    NearSolSource,
    "Galactic",
    "sol");
WriteNdjson(Path.Combine(outDir, "NearSol100.ndjson"), nearSystems);

var hyg = JsonSerializer.Deserialize<List<HygEntry>>(
    File.ReadAllText(hygPath),
    jsonOptions) ?? [];
var hygSol = hyg.SingleOrDefault(entry => entry.Id == 0)
    ?? throw new InvalidOperationException("The HYG local slice must contain source id 0 for Sol.");
var hygSolRaw = ToLightYears(hygSol);
var hygSystems = hyg
    .Where(entry => entry.X is not null && entry.Y is not null && entry.Z is not null)
    .Select(entry =>
    {
        var raw = ToLightYears(entry);
        var normalized = (
            X: raw.X - hygSolRaw.X,
            Y: raw.Y - hygSolRaw.Y,
            Z: raw.Z - hygSolRaw.Z);
        var equatorial = entry.Ra is { } ra && entry.Dec is { } dec
            ? new Equatorial((double)ra * 15d, (double)dec, DistanceFromPosition(normalized))
            : EquatorialFromCartesian(normalized.X, normalized.Y, normalized.Z);
        var distanceLy = entry.Dist is { } distancePc && distancePc >= 0
            ? (double)distancePc * LyPerPc
            : equatorial.DistanceLightYears;
        return new GenSystem(
            entry.Id.ToString(CultureInfo.InvariantCulture),
            entry.Id.ToString(CultureInfo.InvariantCulture),
            DisplayName(entry),
            "EquatorialJ2000",
            normalized.X,
            normalized.Y,
            normalized.Z,
            equatorial.RightAscensionDegrees,
            equatorial.DeclinationDegrees,
            distanceLy,
            entry.Mag is null ? null : (double)entry.Mag.Value,
            entry.Absmag is null ? null : (double)entry.Absmag.Value,
            ParseSpectralClass(entry.Spect),
            entry.Spect,
            entry.Lum is > 0 ? (double)entry.Lum.Value : null,
            []);
    })
    .OrderBy(DistanceOf)
    .ThenBy(system => int.TryParse(system.Id, out var number) ? number : int.MaxValue)
    .ThenBy(system => system.Id, StringComparer.Ordinal)
    .ToList();

WritePack(
    Path.Combine(outDir, "HygLocal1901.g.cs"),
    "HygLocal1901",
    hygSystems,
    "data/hyg-local.json",
    HygSource,
    "EquatorialJ2000",
    "0");
WriteNdjson(Path.Combine(outDir, "HygLocal1901.ndjson"), hygSystems);

Console.WriteLine(
    $"Wrote NearSol100 ({nearSystems.Count}) and HygLocal1901 ({hygSystems.Count}) C# + NDJSON artifacts to {outDir}");
return 0;

static string FindRepoRoot(string start)
{
    var directory = new DirectoryInfo(start);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Novolis.Astro.slnx"))
            || (Directory.Exists(Path.Combine(directory.FullName, "data"))
                && Directory.Exists(Path.Combine(directory.FullName, "src"))))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return start;
}

static (double X, double Y, double Z) ToLightYears(HygEntry entry) =>
    (
        (double)(entry.X ?? throw new InvalidOperationException("Missing HYG X.")) * LyPerPc,
        (double)(entry.Y ?? throw new InvalidOperationException("Missing HYG Y.")) * LyPerPc,
        (double)(entry.Z ?? throw new InvalidOperationException("Missing HYG Z.")) * LyPerPc);

static double DistanceOf(GenSystem system) =>
    DistanceFromPosition((system.X, system.Y, system.Z));

static double DistanceFromPosition((double X, double Y, double Z) position) =>
    Math.Sqrt(
        position.X * position.X
        + position.Y * position.Y
        + position.Z * position.Z);

static Equatorial EquatorialFromGalactic(double x, double y, double z)
{
    // IAU/J2000 Galactic → equatorial rotation (the transpose of the
    // standard equatorial → Galactic matrix).
    var equatorialX = -0.0548755604 * x + 0.4941094279 * y - 0.8676661490 * z;
    var equatorialY = -0.8734370902 * x - 0.4448296300 * y - 0.1980763734 * z;
    var equatorialZ = -0.4838350155 * x + 0.7469822445 * y + 0.4559837762 * z;
    return EquatorialFromCartesian(equatorialX, equatorialY, equatorialZ);
}

static Equatorial EquatorialFromCartesian(double x, double y, double z)
{
    var distance = Math.Sqrt(x * x + y * y + z * z);
    if (distance <= 0)
        return new Equatorial(0, 0, 0);

    var rightAscension = Math.Atan2(y, x) * 180d / Math.PI;
    if (rightAscension < 0)
        rightAscension += 360d;
    var declination = Math.Asin(z / distance) * 180d / Math.PI;
    return new Equatorial(rightAscension, declination, distance);
}

static string DisplayName(HygEntry entry)
{
    if (!string.IsNullOrWhiteSpace(entry.Proper))
        return entry.Proper!;
    if (!string.IsNullOrWhiteSpace(entry.Bf))
        return entry.Bf!;
    if (!string.IsNullOrWhiteSpace(entry.Gl))
        return entry.Gl!;
    if (!string.IsNullOrWhiteSpace(entry.Hd))
        return "HD " + entry.Hd;
    if (!string.IsNullOrWhiteSpace(entry.Hip))
        return "HIP " + entry.Hip;
    return entry.Id.ToString(CultureInfo.InvariantCulture);
}

static string ParseSpectralClass(string? spectral)
{
    if (string.IsNullOrWhiteSpace(spectral))
        return "Unknown";

    var text = spectral.Trim().ToUpperInvariant();
    if (text.Contains("WD", StringComparison.Ordinal) || text.StartsWith('D'))
        return "WD";

    return text[0] switch
    {
        'O' => "O",
        'B' => "B",
        'A' => "A",
        'F' => "F",
        'G' => "G",
        'K' => "K",
        'M' => "M",
        'L' => "L",
        'T' => "T",
        'Y' => "Y",
        _ => "Unknown",
    };
}

static void WritePack(
    string path,
    string propertyName,
    IReadOnlyList<GenSystem> systems,
    string sourceFile,
    string sourceDescription,
    string frame,
    string solSourceId)
{
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated/>");
    builder.AppendLine($"// {sourceDescription}");
    builder.AppendLine("// Sol-relative coordinates are normalized exactly around the source Sol row.");
    builder.AppendLine("// Re-run: dotnet run -c Release --file tools/pregen-catalog.cs");
    builder.AppendLine("#nullable enable");
    builder.AppendLine("using Novolis.Astro.Abstractions;");
    builder.AppendLine("using Novolis.Astro.Catalog;");
    builder.AppendLine();
    builder.AppendLine("namespace Novolis.Astro.Catalog.Data;");
    builder.AppendLine();
    builder.AppendLine("public static partial class CatalogPacks");
    builder.AppendLine("{");
    builder.AppendLine($"    /// <summary>Generated enriched {sourceDescription}</summary>");
    builder.AppendLine(
        $"    public static IReadOnlyList<CelestialCatalogEntry> {propertyName}Entries {{ get; }} = Create{propertyName}Entries();");
    builder.AppendLine();
    builder.AppendLine($"    /// <summary>Source and normalization facts for {propertyName}.</summary>");
    builder.AppendLine(
        $"    public static CatalogPackProvenance {propertyName}Provenance {{ get; }} = new({Literal(propertyName)}, {Literal(sourceFile)}, {Literal(sourceDescription)}, CelestialCartesianFrame.{frame}, {Literal(solSourceId)}, {systems.Count}, true);");
    builder.AppendLine();
    builder.AppendLine($"    /// <summary>Generated compatibility pack for {propertyName}.</summary>");
    builder.AppendLine(
        $"    public static IReadOnlyList<StarSystem> {propertyName} {{ get; }} = ToCatalog({propertyName}Entries).All;");
    builder.AppendLine();
    builder.AppendLine($"    static CelestialCatalogEntry[] Create{propertyName}Entries()");
    builder.AppendLine("    {");
    builder.AppendLine("        return");
    builder.AppendLine("        [");
    foreach (var system in systems)
    {
        builder.Append("            new CelestialCatalogEntry(");
        builder.Append(Literal(system.Id));
        builder.Append(", ");
        builder.Append(Literal(system.SourceId));
        builder.Append(", ");
        builder.Append(Literal(system.Name));
        builder.Append(", CelestialCartesianFrame.");
        builder.Append(system.CartesianFrame);
        builder.Append(", new StarCoords(");
        builder.Append(Format(system.X));
        builder.Append(", ");
        builder.Append(Format(system.Y));
        builder.Append(", ");
        builder.Append(Format(system.Z));
        builder.Append("), new EquatorialCoordinates(");
        builder.Append(Format(system.RightAscensionDegrees));
        builder.Append(", ");
        builder.Append(Format(system.DeclinationDegrees));
        builder.Append(", ");
        builder.Append(Format(system.DistanceLightYears));
        builder.Append("), apparentMagnitude: ");
        builder.Append(NullableFormat(system.ApparentMagnitude));
        builder.Append(", absoluteMagnitude: ");
        builder.Append(NullableFormat(system.AbsoluteMagnitude));
        builder.Append(", spectralClass: SpectralClass.");
        builder.Append(system.SpectralClass);
        builder.Append(", spectralDesignation: ");
        builder.Append(system.SpectralDesignation is null ? "null" : Literal(system.SpectralDesignation));
        builder.Append(", luminositySolar: ");
        builder.Append(NullableFormat(system.LuminositySolar));
        builder.Append(", tags: ");
        builder.Append(TagsLiteral(system.Tags));
        builder.AppendLine("),");
    }

    builder.AppendLine("        ];");
    builder.AppendLine("    }");
    builder.AppendLine("}");
    File.WriteAllText(path, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static void WriteNdjson(string path, IReadOnlyList<GenSystem> systems)
{
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };
    var builder = new StringBuilder();
    foreach (var system in systems)
    {
        var payload = new
        {
            id = system.Id,
            sourceId = system.SourceId,
            name = system.Name,
            cartesianFrame = system.CartesianFrame,
            solRelativeCartesian = new { x = system.X, y = system.Y, z = system.Z },
            equatorial = new
            {
                rightAscensionDegrees = system.RightAscensionDegrees,
                declinationDegrees = system.DeclinationDegrees,
                distanceLightYears = system.DistanceLightYears,
            },
            apparentMagnitude = system.ApparentMagnitude,
            absoluteMagnitude = system.AbsoluteMagnitude,
            spectralClass = system.SpectralClass,
            spectralDesignation = system.SpectralDesignation,
            luminositySolar = system.LuminositySolar,
            tags = system.Tags,
        };
        builder.AppendLine(JsonSerializer.Serialize(payload, options));
    }

    File.WriteAllText(path, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static string Literal(string value) =>
    "\"" + value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\"", "\\\"", StringComparison.Ordinal)
        + "\"";

static string Format(double value) => value.ToString("G17", CultureInfo.InvariantCulture);

static string NullableFormat(double? value) =>
    value is null ? "null" : Format(value.Value);

static string TagsLiteral(IReadOnlyList<string> tags) =>
    tags.Count == 0
        ? "[]"
        : "[" + string.Join(", ", tags.Select(Literal)) + "]";

sealed record GenSystem(
    string Id,
    string SourceId,
    string Name,
    string CartesianFrame,
    double X,
    double Y,
    double Z,
    double RightAscensionDegrees,
    double DeclinationDegrees,
    double DistanceLightYears,
    double? ApparentMagnitude,
    double? AbsoluteMagnitude,
    string SpectralClass,
    string? SpectralDesignation,
    double? LuminositySolar,
    IReadOnlyList<string> Tags);

readonly record struct Equatorial(
    double RightAscensionDegrees,
    double DeclinationDegrees,
    double DistanceLightYears);

sealed class NearSolEntry
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public string? Spectral { get; set; }
    public List<string>? Tags { get; set; }
}

sealed class HygEntry
{
    public int Id { get; set; }
    public string? Hip { get; set; }
    public string? Hd { get; set; }
    public string? Gl { get; set; }
    public string? Bf { get; set; }
    public string? Proper { get; set; }
    public decimal? Ra { get; set; }
    public decimal? Dec { get; set; }
    public decimal? Dist { get; set; }
    public decimal? Mag { get; set; }
    public string? Spect { get; set; }
    public decimal? X { get; set; }
    public decimal? Y { get; set; }
    public decimal? Z { get; set; }
    public decimal? Lum { get; set; }
    public decimal? Absmag { get; set; }
}
