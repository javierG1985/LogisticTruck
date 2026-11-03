using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Modules.Fleet.Trucks;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Modules.Fleet.Trucks.CreateTruck;

public sealed class CreateTruckHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<CreateTruckCommand, TruckDetailsResponse>
{
    public async ValueTask<TruckDetailsResponse> Handle(CreateTruckCommand command, CancellationToken cancellationToken)
    {
        var plate = Plate.Create(command.Plate);

        // IgnoreQueryFilters: la placa es única en la tabla, también entre camiones dados de baja.
        var plateInUse = await db.Trucks
            .IgnoreQueryFilters()
            .AnyAsync(truck => truck.Plate == plate, cancellationToken);

        if (plateInUse)
        {
            throw new ConflictException($"Ya existe un camión con la placa {plate.Value}.");
        }

        var truck = Truck.Register(
            plate,
            command.Brand,
            command.ModelYear,
            command.CapacityKg,
            command.VolumeM3,
            command.SoatExpiresOn,
            command.TechReviewExpiresOn,
            timeProvider.GetUtcNow().UtcDateTime);

        db.Trucks.Add(truck);
        await db.SaveChangesAsync(cancellationToken);

        return TruckDetailsResponse.From(truck);
    }
}
