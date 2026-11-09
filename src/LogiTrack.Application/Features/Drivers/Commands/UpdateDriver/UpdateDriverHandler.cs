using LogiTrack.Application.Common.Abstractions;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.UpdateDriver;

public sealed class UpdateDriverHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<UpdateDriverCommand, uint>
{
    public async ValueTask<uint> Handle(UpdateDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = await DriverLookup.LoadForChangeAsync(db, command.Id, command.ExpectedVersion, cancellationToken);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        driver.Update(
            command.FirstName,
            command.LastName,
            command.Address,
            command.Phone,
            command.Email,
            utcNow,
            command.ProfilePhotoUrl);

        driver.RenewLicense(command.LicenseCategory, command.LicenseIssuedOn, command.LicenseExpiresOn, utcNow);

        await db.SaveChangesAsync(cancellationToken);

        return driver.Version;
    }
}
