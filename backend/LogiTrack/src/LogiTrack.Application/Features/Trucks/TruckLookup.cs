using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Concurrency;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Aggregates.Trucks;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Trucks;

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

        VersionCheck.EnsureMatches(truck.Version, expectedVersion, "El camión");

        return truck;
    }
}
