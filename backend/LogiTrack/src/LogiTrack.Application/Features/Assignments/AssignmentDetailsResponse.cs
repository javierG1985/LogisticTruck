using LogiTrack.Domain.Aggregates.Assignments;

namespace LogiTrack.Application.Features.Assignments;

// Forma de una asignación. La comparten la consulta por camión y el listado, por eso vive en la raíz de la feature.
public sealed record AssignmentDetailsResponse(
    Guid Id,
    Guid TruckId,
    Guid DriverId,
    DateTime AssignedAt,
    DateTime? ReleasedAt,
    AssignmentReleaseReason? ReleaseReason)
{
    public static AssignmentDetailsResponse From(TruckAssignment assignment) =>
        new(
            assignment.Id,
            assignment.TruckId,
            assignment.DriverId,
            assignment.AssignedAt,
            assignment.ReleasedAt,
            assignment.ReleaseReason);
}
