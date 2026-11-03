using LogiTrack.Application.Common.Pagination;
using LogiTrack.Domain.Modules.Fleet.Trucks;
using Mediator;

namespace LogiTrack.Application.Modules.Fleet.Trucks.ListTrucks;

public sealed record ListTrucksQuery : PagedRequest, IQuery<PagedResult<TruckListItem>>
{
    public string? Search { get; init; }

    public TruckStatus? Status { get; init; }

    // Formato: plate,-createdAt (el guion indica descendente).
    public string? Sort { get; init; }
}
