using LogiTrack.Application.Common.Sorting;
using LogiTrack.Domain.Aggregates.Assignments;

namespace LogiTrack.Application.Features.Assignments;

public static class AssignmentSorts
{
    // Por defecto, las asignaciones más recientes primero.
    public static readonly SortMap<TruckAssignment> Map = new SortMap<TruckAssignment>("-assignedAt")
        .Field("assignedAt", assignment => assignment.AssignedAt)
        .Field("releasedAt", assignment => assignment.ReleasedAt);
}
