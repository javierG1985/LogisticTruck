using LogiTrack.Application.Common.Pagination;
using Mediator;

namespace LogiTrack.Application.Features.Assignments.Queries.ListAssignments;

public sealed record ListAssignmentsQuery : PagedRequest, IQuery<PagedResult<AssignmentDetailsResponse>>
{
    public Guid? TruckId { get; init; }

    public Guid? DriverId { get; init; }

    // true: solo activas. false: solo liberadas (historial). Sin valor: todas.
    public bool? Active { get; init; }

    // Formato: assignedAt,-releasedAt (el guion indica descendente).
    public string? Sort { get; init; }
}
