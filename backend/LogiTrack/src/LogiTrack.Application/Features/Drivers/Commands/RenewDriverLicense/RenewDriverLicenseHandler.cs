using LogiTrack.Application.Common.Abstractions;
using LogiTrack.Application.Common.Time;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.RenewDriverLicense;

public sealed class RenewDriverLicenseHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<RenewDriverLicenseCommand, uint>
{
    public async ValueTask<uint> Handle(RenewDriverLicenseCommand command, CancellationToken cancellationToken)
    {
        var driver = await DriverLookup.LoadForChangeAsync(db, command.Id, command.ExpectedVersion, cancellationToken);

        driver.RenewLicense(
            command.LicenseCategory,
            command.LicenseIssuedOn,
            command.LicenseExpiresOn,
            ColombiaCalendar.TodayIn(timeProvider),
            timeProvider.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(cancellationToken);

        return driver.Version;
    }
}
