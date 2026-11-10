using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Drivers.Queries.GetDriverById;

public sealed class GetDriverByIdHandler(IApplicationDbContext db) : IQueryHandler<GetDriverByIdQuery, DriverDetailsResponse>
{
    public async ValueTask<DriverDetailsResponse> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken)
    {
        var driver = await db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException($"No existe un conductor con id {query.Id}.");

        return DriverDetailsResponse.From(driver);
    }
}
