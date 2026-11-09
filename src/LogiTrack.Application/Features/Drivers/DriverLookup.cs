using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Aggregates.Drivers;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Drivers;

// Carga el conductor para modificarlo y compara la versión esperada (If-Match) con la actual.
internal static class DriverLookup
{
    public static async ValueTask<Driver> LoadForChangeAsync(
        IApplicationDbContext db,
        Guid id,
        uint expectedVersion,
        CancellationToken cancellationToken)
    {
        var driver = await db.Drivers.FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No existe un conductor con id {id}.");

        if (driver.Version != expectedVersion)
        {
            throw new PreconditionFailedException("El conductor cambió desde la última lectura. Vuelve a leerlo y reintenta.");
        }

        return driver;
    }
}
