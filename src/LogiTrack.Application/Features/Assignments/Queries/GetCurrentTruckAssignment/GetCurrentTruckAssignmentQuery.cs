using Mediator;

namespace LogiTrack.Application.Features.Assignments.Queries.GetCurrentTruckAssignment;

public sealed record GetCurrentTruckAssignmentQuery(Guid TruckId) : IQuery<AssignmentDetailsResponse>;
