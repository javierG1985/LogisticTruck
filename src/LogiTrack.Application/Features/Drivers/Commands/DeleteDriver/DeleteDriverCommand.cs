using Mediator;

namespace LogiTrack.Application.Features.Drivers.Commands.DeleteDriver;

public sealed record DeleteDriverCommand(Guid Id, uint ExpectedVersion) : ICommand;
