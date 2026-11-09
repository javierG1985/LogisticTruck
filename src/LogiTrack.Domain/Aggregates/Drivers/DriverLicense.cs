using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.Aggregates.Drivers;

// Licencia de servicio público. Vigencia: Ley 769 de 2002, art. 22, modificado por el Decreto 019 de 2012, art. 197:
// 3 años si el conductor tenía menos de 60 años al expedirla; 1 año si tenía 60 o más.
// Un DriverLicense existente siempre cumple esa vigencia: no hay licencias inválidas en memoria.
public sealed record DriverLicense
{
    private const int AgeThresholdForShortTerm = 60;

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
