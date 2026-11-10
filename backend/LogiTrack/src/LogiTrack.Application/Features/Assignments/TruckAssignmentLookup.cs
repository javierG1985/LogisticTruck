using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Concurrency;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Aggregates.Trucks;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Assignments;

// Las asignaciones se protegen con la versión del camión: asignar o liberar cambia el vínculo, no el camión,
// así que dos peticiones con la misma versión compiten en el índice único de la base de datos.
internal static class TruckAssignmentLookup
{
    public static async ValueTask<Truck> LoadTruckForChangeAsync(
        IApplicationDbContext db,
        Guid truckId,
        uint expectedVersion,
        CancellationToken cancellationToken)
    {
        var truck = await db.Trucks.FirstOrDefaultAsync(t => t.Id == truckId, cancellationToken)
            ?? throw new NotFoundException($"No existe un camión con id {truckId}.");

        VersionCheck.EnsureMatches(truck.Version, expectedVersion, "El camión");

        return truck;
    }
}
