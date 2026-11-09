using LogiTrack.Domain.Aggregates.Drivers;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.CreateDriver;

public sealed record CreateDriverCommand(
    string DocumentNumber,
    string FirstName,
    string LastName,
    string Address,
    string Phone,
    string Email,
    DateOnly BirthDate,
    LicenseCategory LicenseCategory,
    DateOnly LicenseIssuedOn,
    DateOnly LicenseExpiresOn,
    string? ProfilePhotoUrl) : ICommand<DriverDetailsResponse>;
