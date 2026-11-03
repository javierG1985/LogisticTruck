using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.DeleteTruck;

public sealed record DeleteTruckCommand(Guid Id, uint ExpectedVersion) : ICommand;
