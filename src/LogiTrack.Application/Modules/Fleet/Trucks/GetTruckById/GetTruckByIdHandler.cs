using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Modules.Fleet.Trucks.GetTruckById;

public sealed class GetTruckByIdHandler(IApplicationDbContext db) : IQueryHandler<GetTruckByIdQuery, TruckDetailsResponse>
{
    public async ValueTask<TruckDetailsResponse> Handle(GetTruckByIdQuery query, CancellationToken cancellationToken)
    {
        var truck = await db.Trucks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException($"No existe un camión con id {query.Id}.");

        return TruckDetailsResponse.From(truck);
    }
}
