using LogiTrack.Domain.Aggregates.Trucks;
using Mediator;

namespace LogiTrack.Application.Features.Trucks.Commands.ChangeTruckStatus;

public sealed record ChangeTruckStatusCommand(Guid Id, uint ExpectedVersion, TruckStatus Status) : ICommand<uint>;
