using LogiTrack.Application.Common.Abstractions;
using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.ChangeTruckStatus;

public sealed class ChangeTruckStatusHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<ChangeTruckStatusCommand, uint>
{
    public async ValueTask<uint> Handle(ChangeTruckStatusCommand command, CancellationToken cancellationToken)
    {
        var truck = await TruckLookup.LoadForChangeAsync(db, command.Id, command.ExpectedVersion, cancellationToken);

        truck.ChangeStatus(command.Status, timeProvider.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(cancellationToken);

        return truck.Version;
    }
}
