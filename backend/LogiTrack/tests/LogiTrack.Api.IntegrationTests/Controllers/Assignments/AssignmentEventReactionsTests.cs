using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Assignments;

// Cada cambio de estado o baja de un camión o un conductor libera (o no) su asignación activa, vía eventos de dominio.
public sealed class AssignmentEventReactionsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly FleetClient _fleet = new(factory.CreateClient());

    [Theory]
    [InlineData("Maintenance")]
    [InlineData("Inactive")]
    public async Task TruckLeavesService_ReleasesItsAssignment(string status)
    {
        var (truckId, _) = await AssignedPairAsync();

        var response = await _fleet.ChangeTruckStatusAsync(truckId, status, await _fleet.ReadTruckEtagAsync(truckId));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await AssertReleasedWithReasonAsync(truckId, "TruckStatusChanged");
    }

    [Fact]
    public async Task TruckGoesInRoute_KeepsItsAssignment()
    {
        var (truckId, _) = await AssignedPairAsync();

        await _fleet.ChangeTruckStatusAsync(truckId, "InRoute", await _fleet.ReadTruckEtagAsync(truckId));

        Assert.Equal(HttpStatusCode.OK, (await _fleet.Http.GetAsync($"/api/trucks/{truckId}/assignment")).StatusCode);
    }

    [Fact]
    public async Task TruckIsDeleted_ReleasesItsAssignment()
    {
        var (truckId, _) = await AssignedPairAsync();

        var response = await _fleet.DeleteTruckAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await AssertReleasedWithReasonAsync(truckId, "TruckDeleted");
    }

    [Theory]
    [InlineData("OnLeave")]
    [InlineData("Inactive")]
    public async Task DriverLeavesService_ReleasesHisAssignment(string status)
    {
        var (truckId, driverId) = await AssignedPairAsync();

        var response = await _fleet.ChangeDriverStatusAsync(driverId, status, await _fleet.ReadDriverEtagAsync(driverId));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await AssertReleasedWithReasonAsync(truckId, "DriverStatusChanged");
    }

    [Fact]
    public async Task DriverIsDeleted_ReleasesHisAssignment()
    {
        var (truckId, driverId) = await AssignedPairAsync();

        var response = await _fleet.DeleteDriverAsync(driverId, await _fleet.ReadDriverEtagAsync(driverId));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await AssertReleasedWithReasonAsync(truckId, "DriverDeleted");
    }

    [Fact]
    public async Task TruckGoesToMaintenanceWithoutAssignment_StillSucceeds()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());

        var response = await _fleet.ChangeTruckStatusAsync(truckId, "Maintenance", await _fleet.ReadTruckEtagAsync(truckId));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<(Guid TruckId, Guid DriverId)> AssignedPairAsync()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        var driverId = await _fleet.CreateDriverAsync("C2");

        var response = await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), driverId);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        return (truckId, driverId);
    }

    private async Task AssertReleasedWithReasonAsync(Guid truckId, string expectedReason)
    {
        var body = await _fleet.ListAssignmentsAsync($"truckId={truckId}&active=false");

        var released = Assert.Single(body!["items"]!.AsArray());
        Assert.Equal(expectedReason, released!["releaseReason"]!.GetValue<string>());
        Assert.NotNull(released["releasedAt"]);
    }
}
