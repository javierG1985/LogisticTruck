using LogiTrack.Application.Common.Abstractions;
using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.DeleteTruck;

public sealed class DeleteTruckHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<DeleteTruckCommand>
{
    public async ValueTask<Unit> Handle(DeleteTruckCommand command, CancellationToken cancellationToken)
    {
        var truck = await TruckLookup.LoadForChangeAsync(db, command.Id, command.ExpectedVersion, cancellationToken);

        truck.Delete(timeProvider.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
