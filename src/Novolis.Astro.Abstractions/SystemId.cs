namespace Novolis.Astro.Abstractions;

/// <summary>Stable identifier for a catalogued star system.</summary>
/// <param name="Value">Opaque id string (catalog-assigned).</param>
public readonly record struct SystemId(string Value)
{
    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Implicit conversion from string.</summary>
    public static implicit operator SystemId(string value) => new(value);

    /// <summary>Implicit conversion to string.</summary>
    public static implicit operator string(SystemId id) => id.Value;
}
