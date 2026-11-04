using LogiTrack.Application.Common.Abstractions;
using Mediator;

namespace LogiTrack.Application.Features.Trucks.Commands.UpdateTruck;

public sealed class UpdateTruckHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<UpdateTruckCommand, uint>
{
    public async ValueTask<uint> Handle(UpdateTruckCommand command, CancellationToken cancellationToken)
    {
        var truck = await TruckLookup.LoadForChangeAsync(db, command.Id, command.ExpectedVersion, cancellationToken);

        truck.Update(
            command.Brand,
            command.ModelYear,
            command.CapacityKg,
            command.VolumeM3,
            command.SoatExpiresOn,
            command.TechReviewExpiresOn,
            timeProvider.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(cancellationToken);

        return truck.Version;
    }
}
