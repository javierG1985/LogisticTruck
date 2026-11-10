using LogiTrack.Domain.Aggregates.Assignments;
using LogiTrack.Domain.Aggregates.Assignments.Events;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.UnitTests.Aggregates.Assignments;

public sealed class TruckAssignmentTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Guid TruckId = Guid.CreateVersion7();
    private static readonly Guid DriverId = Guid.CreateVersion7();

    [Fact]
    public void Start_NewAssignment_IsActiveAndRaisesDriverAssigned()
    {
        var assignment = TruckAssignment.Start(TruckId, DriverId, Now);

        Assert.True(assignment.IsActive);
        Assert.Equal(Now, assignment.AssignedAt);

        var domainEvent = Assert.IsType<DriverAssigned>(Assert.Single(assignment.DomainEvents));
        Assert.Equal(assignment.Id, domainEvent.AssignmentId);
        Assert.Equal(TruckId, domainEvent.TruckId);
        Assert.Equal(DriverId, domainEvent.DriverId);
    }

    [Fact]
    public void Release_ActiveAssignment_StoresReasonAndRaisesAssignmentReleased()
    {
        var assignment = TruckAssignment.Start(TruckId, DriverId, Now);
        assignment.ClearDomainEvents();

        assignment.Release(AssignmentReleaseReason.TruckStatusChanged, Now.AddHours(2));

        Assert.False(assignment.IsActive);
        Assert.Equal(Now.AddHours(2), assignment.ReleasedAt);
        Assert.Equal(AssignmentReleaseReason.TruckStatusChanged, assignment.ReleaseReason);

        var domainEvent = Assert.IsType<AssignmentReleased>(Assert.Single(assignment.DomainEvents));
        Assert.Equal(AssignmentReleaseReason.TruckStatusChanged, domainEvent.Reason);
    }

    [Fact]
    public void Release_TwiceThrowsDomainException()
    {
        var assignment = TruckAssignment.Start(TruckId, DriverId, Now);
        assignment.Release(AssignmentReleaseReason.Manual, Now);

        Assert.Throws<DomainException>(() => assignment.Release(AssignmentReleaseReason.Manual, Now));
    }
}
