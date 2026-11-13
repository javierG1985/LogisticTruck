namespace LogiTrack.Domain.Aggregates.Drivers;

// Estado de una licencia respecto a una fecha. Se calcula; no se guarda. Aplica igual a todas las categorías.
public enum LicenseStatus
{
    Valid,
    ExpiringSoon,
    Expired,
}
