using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Trucks;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Common.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Truck> Trucks { get; }

    DbSet<Driver> Drivers { get; }

    DbSet<TruckAssignment> TruckAssignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
