using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Aggregates.Assignments;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Assignments.Commands.ReleaseTruckAssignment;

public sealed class ReleaseTruckAssignmentHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<ReleaseTruckAssignmentCommand>
{
    public async ValueTask<Unit> Handle(ReleaseTruckAssignmentCommand command, CancellationToken cancellationToken)
    {
        var truck = await TruckAssignmentLookup.LoadTruckForChangeAsync(
            db, command.TruckId, command.ExpectedVersion, cancellationToken);

        var assignment = await db.TruckAssignments
            .FirstOrDefaultAsync(a => a.TruckId == truck.Id && a.ReleasedAt == null, cancellationToken)
            ?? throw new NotFoundException("El camión no tiene ningún conductor asignado.");

        assignment.Release(AssignmentReleaseReason.Manual, timeProvider.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
