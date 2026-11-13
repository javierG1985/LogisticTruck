using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Drivers;

// Licencia de servicio público (categorías C). Vigencia de 3 años; 1 año si el conductor tenía 60 años o más al expedirla.
// Fuente: tabla de categorías de licencia pegada por el usuario (no es el texto oficial). La regla usa la edad al expedir, no la actual.
// Un DriverLicense existente siempre cumple esa vigencia: no hay licencias inválidas en memoria.
public sealed record DriverLicense
{
    private const int AgeThresholdForShortTerm = 60;

    public const int ExpiringSoonWindowDays = 60;

    private DriverLicense(LicenseCategory category, DateOnly issuedOn, DateOnly expiresOn)
    {
        Category = category;
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
    }

    public LicenseCategory Category { get; }

    public DateOnly IssuedOn { get; }

    public DateOnly ExpiresOn { get; }

    public static DriverLicense Create(LicenseCategory category, DateOnly issuedOn, DateOnly expiresOn, DateOnly birthDate)
    {
        if (issuedOn < birthDate)
        {
            throw new DomainException("La licencia no puede expedirse antes de la fecha de nacimiento del conductor.");
        }

        if (expiresOn <= issuedOn)
        {
            throw new DomainException("El vencimiento de la licencia debe ser posterior a su expedición.");
        }

        var latestExpiry = LatestAllowedExpiry(issuedOn, birthDate);

        if (expiresOn > latestExpiry)
        {
            throw new DomainException(
                $"Según la norma, esta licencia no puede vencer después del {latestExpiry:dd/MM/yyyy}.");
        }

        return new DriverLicense(category, issuedOn, expiresOn);
    }

    public bool IsValidOn(DateOnly date) => date >= IssuedOn && date <= ExpiresOn;

    public LicenseStatus StatusOn(DateOnly today) => StatusFor(ExpiresOn, today);

    // Alerta de vencimiento: "próxima a vencer" cuando faltan ExpiringSoonWindowDays días o menos (incluido el día de vencimiento).
    public static LicenseStatus StatusFor(DateOnly expiresOn, DateOnly today)
    {
        if (today > expiresOn)
        {
            return LicenseStatus.Expired;
        }

        return expiresOn.DayNumber - today.DayNumber <= ExpiringSoonWindowDays
            ? LicenseStatus.ExpiringSoon
            : LicenseStatus.Valid;
    }

    // Una categoría superior habilita las inferiores (Resolución 1500 de 2005, art. 5, parágrafo 1).
    public bool Allows(LicenseCategory requiredCategory) => Category >= requiredCategory;

    private static DateOnly LatestAllowedExpiry(DateOnly issuedOn, DateOnly birthDate)
    {
        var years = AgeOn(birthDate, issuedOn) >= AgeThresholdForShortTerm ? 1 : 3;

        return issuedOn.AddYears(years);
    }

    private static int AgeOn(DateOnly birthDate, DateOnly date)
    {
        var age = date.Year - birthDate.Year;

        if (date < birthDate.AddYears(age))
        {
            age--;
        }

        return age;
    }
}
