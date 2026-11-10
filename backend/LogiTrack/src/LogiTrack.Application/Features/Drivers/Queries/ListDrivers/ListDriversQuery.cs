using LogiTrack.Application.Common.Pagination;
using LogiTrack.Domain.Aggregates.Drivers;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Queries.ListDrivers;

public sealed record ListDriversQuery : PagedRequest, IQuery<PagedResult<DriverListItem>>
{
    public string? Search { get; init; }

    public DriverStatus? Status { get; init; }

    public LicenseCategory? LicenseCategory { get; init; }

    // Conductores cuya licencia vence antes de esta fecha (exclusiva).
    public DateOnly? LicenseExpiringBefore { get; init; }

    // Formato: lastName,-createdAt (el guion indica descendente).
    public string? Sort { get; init; }
}
