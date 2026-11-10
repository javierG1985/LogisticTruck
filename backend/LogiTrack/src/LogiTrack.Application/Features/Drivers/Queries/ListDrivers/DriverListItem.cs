using LogiTrack.Domain.Aggregates.Drivers;

namespace LogiTrack.Application.Features.Drivers.Queries.ListDrivers;

public sealed record DriverListItem(
    Guid Id,
    string DocumentNumber,
    string FirstName,
    string LastName,
    DriverStatus Status,
    LicenseCategory LicenseCategory,
    DateOnly LicenseExpiresOn);
