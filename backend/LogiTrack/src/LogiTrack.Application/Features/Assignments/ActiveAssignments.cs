using System.Linq.Expressions;
using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Domain.Aggregates.Assignments;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Assignments;

// Libera las asignaciones activas que cumplan una condición. Lo usan los handlers de eventos de camiones y conductores.
// Es idempotente: si no hay nada activo que liberar, no hace nada.
internal static class ActiveAssignments
{
    public static async ValueTask ReleaseAsync(
        IApplicationDbContext db,
        TimeProvider timeProvider,
        Expression<Func<TruckAssignment, bool>> match,
        AssignmentReleaseReason reason,
        CancellationToken cancellationToken)
    {
        var active = await db.TruckAssignments
            .Where(match)
            .Where(assignment => assignment.ReleasedAt == null)
            .ToListAsync(cancellationToken);

        if (active.Count == 0)
        {
            return;
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var assignment in active)
        {
            assignment.Release(reason, utcNow);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
