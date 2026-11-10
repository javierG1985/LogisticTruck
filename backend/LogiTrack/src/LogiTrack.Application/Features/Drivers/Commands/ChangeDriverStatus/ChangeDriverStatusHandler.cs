using LogiTrack.Application.Common.Abstractions;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.ChangeDriverStatus;

public sealed class ChangeDriverStatusHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<ChangeDriverStatusCommand, uint>
{
    public async ValueTask<uint> Handle(ChangeDriverStatusCommand command, CancellationToken cancellationToken)
    {
        var driver = await DriverLookup.LoadForChangeAsync(db, command.Id, command.ExpectedVersion, cancellationToken);

        driver.ChangeStatus(command.Status, timeProvider.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(cancellationToken);

        return driver.Version;
    }
}
