using Mediator;

namespace LogiTrack.Application.Features.Drivers.Queries.GetDriverById;

public sealed record GetDriverByIdQuery(Guid Id) : IQuery<DriverDetailsResponse>;
