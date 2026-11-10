using LogiTrack.Domain.Aggregates.Drivers;
using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.ChangeDriverStatus;

public sealed record ChangeDriverStatusCommand(Guid Id, uint ExpectedVersion, DriverStatus Status) : ICommand<uint>;
