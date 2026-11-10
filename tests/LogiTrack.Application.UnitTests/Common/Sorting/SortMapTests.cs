using LogiTrack.Application.Common.Sorting;
using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Application.UnitTests.Common.Sorting;

public sealed class SortMapTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    private static readonly SortMap<Truck> Map = new SortMap<Truck>("modelYear")
        .Field("plate", truck => truck.Plate)
        .Field("modelYear", truck => truck.ModelYear);

    [Fact]
    public void Accepts_KnownFieldsAndEmptySort_IsTrue()
    {
        Assert.True(Map.Accepts("plate,-modelYear"));
        Assert.True(Map.Accepts(null));
        Assert.True(Map.Accepts(string.Empty));
    }

    [Fact]
    public void Accepts_UnknownField_IsFalse()
    {
        Assert.False(Map.Accepts("salary"));
    }

    [Fact]
    public void FieldNames_ListsTheRegisteredFields()
    {
        Assert.Equal(["plate", "modelYear"], Map.FieldNames);
    }

    [Fact]
    public void Apply_WithDescendingField_OrdersDescending()
    {
        var trucks = new[] { Truck("AAA101", 2020), Truck("AAA102", 2024), Truck("AAA103", 2022) }.AsQueryable();

        var sorted = Map.Apply(trucks, "-modelYear").Select(truck => truck.ModelYear).ToList();

        Assert.Equal([2024, 2022, 2020], sorted);
    }

    [Fact]
    public void Apply_WithoutSort_UsesTheDefaultOrder()
    {
        var trucks = new[] { Truck("CCC101", 2020), Truck("AAA102", 2024), Truck("BBB103", 2022) }.AsQueryable();

        var sorted = Map.Apply(trucks, null).Select(truck => truck.Plate.Value).ToList();

        Assert.Equal(["CCC101", "BBB103", "AAA102"], sorted);
    }

    [Fact]
    public void Apply_WithOnlyUnknownFields_FallsBackToTheDefaultOrder()
    {
        var trucks = new[] { Truck("CCC101", 2020), Truck("AAA102", 2024) }.AsQueryable();

        var sorted = Map.Apply(trucks, "salary").Select(truck => truck.Plate.Value).ToList();

        Assert.Equal(["CCC101", "AAA102"], sorted);
    }

    [Fact]
    public void Apply_BreaksTiesByIdSoPagesAreStable()
    {
        var first = Truck("AAA101", 2020);
        var second = Truck("AAA102", 2020);
        var trucks = new[] { second, first }.AsQueryable();

        var sorted = Map.Apply(trucks, "modelYear").Select(truck => truck.Id).ToList();

        Assert.Equal(sorted.Order(), sorted);
    }

    private static Truck Truck(string plate, int modelYear) =>
        LogiTrack.Domain.Aggregates.Trucks.Truck.Register(
            Plate.Create(plate), "Kenworth", TruckType.Rigid, modelYear, 8000m, 30m,
            new DateOnly(2027, 1, 31), new DateOnly(2027, 6, 30), Now);
}
