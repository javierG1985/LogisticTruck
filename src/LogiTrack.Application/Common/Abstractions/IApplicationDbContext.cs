using LogiTrack.Domain.Aggregates.Trucks;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Common.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Truck> Trucks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
