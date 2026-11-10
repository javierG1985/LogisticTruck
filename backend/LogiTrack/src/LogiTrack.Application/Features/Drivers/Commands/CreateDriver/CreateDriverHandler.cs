using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Exceptions;
using LogiTrack.Domain.Aggregates.Drivers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Application.Features.Drivers.Commands.CreateDriver;

public sealed class CreateDriverHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<CreateDriverCommand, DriverDetailsResponse>
{
    public async ValueTask<DriverDetailsResponse> Handle(CreateDriverCommand command, CancellationToken cancellationToken)
    {
        var documentNumber = DocumentNumber.Create(command.DocumentNumber);

        // IgnoreQueryFilters: la cédula es única en la tabla, también entre conductores dados de baja.
        var documentInUse = await db.Drivers
            .IgnoreQueryFilters()
            .AnyAsync(driver => driver.DocumentNumber == documentNumber, cancellationToken);

        if (documentInUse)
        {
            throw new ConflictException($"Ya existe un conductor con la cédula {documentNumber.Value}.");
        }

        var driver = Driver.Register(
            documentNumber,
            command.FirstName,
            command.LastName,
            command.Address,
            command.Phone,
            command.Email,
            command.BirthDate,
            command.LicenseCategory,
            command.LicenseIssuedOn,
            command.LicenseExpiresOn,
            timeProvider.GetUtcNow().UtcDateTime,
            command.ProfilePhotoUrl);

        db.Drivers.Add(driver);
        await db.SaveChangesAsync(cancellationToken);

        return DriverDetailsResponse.From(driver);
    }
}
