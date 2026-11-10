using LogiTrack.Application.Common.Abstractions;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.DeleteDriver;

public sealed class DeleteDriverHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : ICommandHandler<DeleteDriverCommand>
{
    public async ValueTask<Unit> Handle(DeleteDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = await DriverLookup.LoadForChangeAsync(db, command.Id, command.ExpectedVersion, cancellationToken);

        driver.Delete(timeProvider.GetUtcNow().UtcDateTime);

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
