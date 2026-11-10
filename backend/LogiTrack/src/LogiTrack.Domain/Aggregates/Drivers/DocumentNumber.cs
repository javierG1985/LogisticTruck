using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Drivers;

// Cédula colombiana: solo dígitos, entre 6 y 10. Un DocumentNumber existente siempre tiene el formato correcto.
public sealed partial record DocumentNumber
{
    private DocumentNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DocumentNumber Create(string? raw)
    {
        if (TryCreate(raw, out var documentNumber))
        {
            return documentNumber;
        }

        throw new DomainException("La cédula debe tener entre 6 y 10 dígitos, sin puntos ni letras.");
    }

    public static bool TryCreate(string? raw, [NotNullWhen(true)] out DocumentNumber? documentNumber)
    {
        var normalized = raw?.Trim();

        if (normalized is not null && DocumentFormat().IsMatch(normalized))
        {
            documentNumber = new DocumentNumber(normalized);
            return true;
        }

        documentNumber = null;
        return false;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[0-9]{6,10}$")]
    private static partial Regex DocumentFormat();
}
