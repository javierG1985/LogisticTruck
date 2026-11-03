using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.GetTruckById;

public sealed record GetTruckByIdQuery(Guid Id) : IQuery<TruckDetailsResponse>;
