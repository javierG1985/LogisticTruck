using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Trucks;

// Placa colombiana (ABC123). Un Plate existente siempre tiene el formato correcto: no hay placas inválidas en memoria.
public sealed partial record Plate
{
    private Plate(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Plate Create(string? raw)
    {
        if (TryCreate(raw, out var plate))
        {
            return plate;
        }

        throw new DomainException("La placa debe tener el formato ABC123: tres letras y tres números.");
    }

    public static bool TryCreate(string? raw, [NotNullWhen(true)] out Plate? plate)
    {
        var normalized = raw?.Trim().ToUpperInvariant();

        if (normalized is not null && PlateFormat().IsMatch(normalized))
        {
            plate = new Plate(normalized);
            return true;
        }

        plate = null;
        return false;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z]{3}[0-9]{3}$")]
    private static partial Regex PlateFormat();
}
