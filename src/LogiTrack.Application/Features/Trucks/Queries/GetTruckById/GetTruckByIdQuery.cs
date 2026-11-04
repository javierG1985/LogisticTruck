using Mediator;

namespace LogiTrack.Application.Features.Trucks.Queries.GetTruckById;

public sealed record GetTruckByIdQuery(Guid Id) : IQuery<TruckDetailsResponse>;
