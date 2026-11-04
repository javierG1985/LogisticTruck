using LogiTrack.Domain.Aggregates.Trucks;
using LogiTrack.Domain.Aggregates.Trucks.Events;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Domain.UnitTests.Aggregates.Trucks;

public sealed class TruckTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly SoatExpiresOn = new(2027, 1, 31);
    private static readonly DateOnly TechReviewExpiresOn = new(2027, 6, 30);

    [Fact]
    public void Register_WithZeroCapacity_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Truck.Register(
            Plate.Create("ABC123"), "Kenworth", 2022, 0m, 30m, SoatExpiresOn, TechReviewExpiresOn, Now));
    }

    [Fact]
    public void Register_WithZeroVolume_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Truck.Register(
            Plate.Create("ABC123"), "Kenworth", 2022, 8000m, 0m, SoatExpiresOn, TechReviewExpiresOn, Now));
    }

    [Fact]
    public void Register_NewTruck_IsAvailableAndRaisesTruckRegistered()
    {
        var truck = CreateTruck();

        Assert.Equal(TruckStatus.Available, truck.Status);

        var domainEvent = Assert.IsType<TruckRegistered>(Assert.Single(truck.DomainEvents));
        Assert.Equal(truck.Id, domainEvent.TruckId);
        Assert.Equal("ABC123", domainEvent.Plate);
    }

    [Fact]
    public void ChangeStatus_FromAvailableToInRoute_IsAllowed()
    {
        var truck = CreateTruck();

        truck.ChangeStatus(TruckStatus.InRoute, Now);

        Assert.Equal(TruckStatus.InRoute, truck.Status);
    }

    [Fact]
    public void ChangeStatus_FromMaintenanceToInRoute_ThrowsDomainException()
    {
        var truck = CreateTruck();
        truck.ChangeStatus(TruckStatus.Maintenance, Now);

        Assert.Throws<DomainException>(() => truck.ChangeStatus(TruckStatus.InRoute, Now));
    }

    [Fact]
    public void ChangeStatus_ToTheSameStatus_DoesNothing()
    {
        var truck = CreateTruck();
        truck.ClearDomainEvents();

        truck.ChangeStatus(TruckStatus.Available, Now);

        Assert.Empty(truck.DomainEvents);
    }

    [Fact]
    public void ChangeStatus_ToNewStatus_RaisesTruckStatusChangedWithPreviousAndNewStatus()
    {
        var truck = CreateTruck();
        truck.ClearDomainEvents();

        truck.ChangeStatus(TruckStatus.Maintenance, Now);

        var domainEvent = Assert.IsType<TruckStatusChanged>(Assert.Single(truck.DomainEvents));
        Assert.Equal(TruckStatus.Available, domainEvent.PreviousStatus);
        Assert.Equal(TruckStatus.Maintenance, domainEvent.NewStatus);
    }

    [Fact]
    public void Update_WithValidData_ChangesFieldsWithoutRaisingEvents()
    {
        var truck = CreateTruck();
        truck.ClearDomainEvents();

        truck.Update("Volvo FH", 2024, 9000m, 35m, SoatExpiresOn, TechReviewExpiresOn, Now);

        Assert.Equal("Volvo FH", truck.Brand);
        Assert.Equal(9000m, truck.CapacityKg);
        Assert.Empty(truck.DomainEvents);
    }

    [Fact]
    public void Update_WithZeroCapacity_ThrowsDomainException()
    {
        var truck = CreateTruck();

        Assert.Throws<DomainException>(() =>
            truck.Update("Volvo FH", 2024, 0m, 35m, SoatExpiresOn, TechReviewExpiresOn, Now));
    }

    [Fact]
    public void Update_OnDeletedTruck_ThrowsDomainException()
    {
        var truck = CreateTruck();
        truck.Delete(Now);

        Assert.Throws<DomainException>(() =>
            truck.Update("Volvo FH", 2024, 9000m, 35m, SoatExpiresOn, TechReviewExpiresOn, Now));
    }

    [Fact]
    public void ChangeStatus_OnDeletedTruck_ThrowsDomainException()
    {
        var truck = CreateTruck();
        truck.Delete(Now);

        Assert.Throws<DomainException>(() => truck.ChangeStatus(TruckStatus.InRoute, Now));
    }

    [Fact]
    public void Delete_CalledTwice_KeepsTheFirstDeletionDate()
    {
        var truck = CreateTruck();
        var firstDeletion = Now;
        var secondDeletion = Now.AddHours(1);

        truck.Delete(firstDeletion);
        truck.Delete(secondDeletion);

        Assert.Equal(firstDeletion, truck.DeletedAt);
    }

    private static Truck CreateTruck() =>
        Truck.Register(Plate.Create("ABC123"), "Kenworth", 2022, 8000m, 30m, SoatExpiresOn, TechReviewExpiresOn, Now);
}
