using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Aggregates.Assignments;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Assignments.Commands.AssignDriverToTruck;

public sealed class AssignDriverToTruckHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<AssignDriverToTruckCommand, uint>
{
    public async ValueTask<uint> Handle(AssignDriverToTruckCommand command, CancellationToken cancellationToken)
    {
        var truck = await TruckAssignmentLookup.LoadTruckForChangeAsync(
            db, command.TruckId, command.ExpectedVersion, cancellationToken);

        var driver = await db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == command.DriverId, cancellationToken)
            ?? throw new NotFoundException($"No existe un conductor con id {command.DriverId}.");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        TruckAssignmentPolicy.EnsureCanAssign(truck, driver, DateOnly.FromDateTime(utcNow));

        // Comprobación previa para dar un mensaje claro. Si dos peticiones pasan a la vez, el índice único
        // parcial rechaza la segunda y SaveChanges la convierte en 409.
        var truckHasDriver = await db.TruckAssignments
            .AnyAsync(a => a.TruckId == truck.Id && a.ReleasedAt == null, cancellationToken);

        if (truckHasDriver)
        {
            throw new ConflictException("El camión ya tiene un conductor asignado.");
        }

        var driverHasTruck = await db.TruckAssignments
            .AnyAsync(a => a.DriverId == driver.Id && a.ReleasedAt == null, cancellationToken);

        if (driverHasTruck)
        {
            throw new ConflictException("El conductor ya está asignado a otro camión.");
        }

        db.TruckAssignments.Add(TruckAssignment.Start(truck.Id, driver.Id, utcNow));
        await db.SaveChangesAsync(cancellationToken);

        return truck.Version;
    }
}
