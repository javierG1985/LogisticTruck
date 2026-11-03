using LogiTrack.Domain.Modules.Fleet.Trucks;
using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.ChangeTruckStatus;

public sealed record ChangeTruckStatusCommand(Guid Id, uint ExpectedVersion, TruckStatus Status) : ICommand<uint>;
