using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Assignments.Queries.GetCurrentTruckAssignment;

public sealed class GetCurrentTruckAssignmentHandler(IApplicationDbContext db)
    : IQueryHandler<GetCurrentTruckAssignmentQuery, AssignmentDetailsResponse>
{
    public async ValueTask<AssignmentDetailsResponse> Handle(GetCurrentTruckAssignmentQuery query, CancellationToken cancellationToken)
    {
        var assignment = await db.TruckAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TruckId == query.TruckId && a.ReleasedAt == null, cancellationToken)
            ?? throw new NotFoundException("El camión no tiene ningún conductor asignado.");

        return AssignmentDetailsResponse.From(assignment);
    }
}
