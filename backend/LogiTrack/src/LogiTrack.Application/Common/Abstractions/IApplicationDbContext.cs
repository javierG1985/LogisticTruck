using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Catalogs;
using LogiTrack.Domain.Aggregates.Drivers;
using LogiTrack.Domain.Aggregates.Trucks;
using LogiTrack.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Common.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Truck> Trucks { get; }

    DbSet<Driver> Drivers { get; }

    DbSet<TruckAssignment> TruckAssignments { get; }

    DbSet<User> Users { get; }

    DbSet<Role> Roles { get; }

    DbSet<RolePrivilege> RolePrivileges { get; }

    DbSet<CatalogItem> CatalogItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
