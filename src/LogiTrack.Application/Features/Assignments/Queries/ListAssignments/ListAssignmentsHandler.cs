using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Pagination;
using LogiTrack.Domain.Aggregates.Assignments;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Assignments.Queries.ListAssignments;

public sealed class ListAssignmentsHandler(IApplicationDbContext db)
    : IQueryHandler<ListAssignmentsQuery, PagedResult<AssignmentDetailsResponse>>
{
    public async ValueTask<PagedResult<AssignmentDetailsResponse>> Handle(ListAssignmentsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<TruckAssignment> assignments = db.TruckAssignments.AsNoTracking();

        if (query.TruckId is { } truckId)
        {
            assignments = assignments.Where(assignment => assignment.TruckId == truckId);
        }

        if (query.DriverId is { } driverId)
        {
            assignments = assignments.Where(assignment => assignment.DriverId == driverId);
        }

        if (query.Active is { } active)
        {
            assignments = active
                ? assignments.Where(assignment => assignment.ReleasedAt == null)
                : assignments.Where(assignment => assignment.ReleasedAt != null);
        }

        var totalCount = await assignments.CountAsync(cancellationToken);

        var items = await AssignmentSorts.Map.Apply(assignments, query.Sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(assignment => new AssignmentDetailsResponse(
                assignment.Id,
                assignment.TruckId,
                assignment.DriverId,
                assignment.AssignedAt,
                assignment.ReleasedAt,
                assignment.ReleaseReason))
            .ToListAsync(cancellationToken);

        return new PagedResult<AssignmentDetailsResponse>(items, query.Page, query.PageSize, totalCount);
    }
}
