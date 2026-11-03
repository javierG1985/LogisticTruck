using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Modules.Fleet.Trucks;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Modules.Fleet.Trucks;

// Carga el camión para modificarlo y compara la versión esperada (If-Match) con la actual.
internal static class TruckLookup
{
    public static async ValueTask<Truck> LoadForChangeAsync(
        IApplicationDbContext db,
        Guid id,
        uint expectedVersion,
        CancellationToken cancellationToken)
    {
        var truck = await db.Trucks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No existe un camión con id {id}.");

        if (truck.Version != expectedVersion)
        {
            throw new PreconditionFailedException("El camión cambió desde la última lectura. Vuelve a leerlo y reintenta.");
        }

        return truck;
    }
}
