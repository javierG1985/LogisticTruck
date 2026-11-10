using Mediator;

namespace LogiTrack.Application.Features.Assignments.Commands.ReleaseTruckAssignment;

public sealed record ReleaseTruckAssignmentCommand(Guid TruckId, uint ExpectedVersion) : ICommand;
