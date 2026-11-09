using LogiTrack.Application.Common.Sorting;
using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Application.Features.Drivers;

// Campos por los que se puede ordenar la lista de conductores. Lo usan el handler y el validador.
public static class DriverSorts
{
    public static readonly SortMap<Driver> Map = new SortMap<Driver>("lastName,firstName")
        .Field("firstName", driver => driver.FirstName)
        .Field("lastName", driver => driver.LastName)
        .Field("documentNumber", driver => driver.DocumentNumber)
        .Field("status", driver => driver.Status)
        .Field("licenseExpiresOn", driver => driver.License.ExpiresOn)
        .Field("createdAt", driver => driver.CreatedAt);
}
