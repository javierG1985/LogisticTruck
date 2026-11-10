using Mediator;

namespace LogiTrack.Application.Features.Trucks.Commands.DeleteTruck;

public sealed record DeleteTruckCommand(Guid Id, uint ExpectedVersion) : ICommand;
