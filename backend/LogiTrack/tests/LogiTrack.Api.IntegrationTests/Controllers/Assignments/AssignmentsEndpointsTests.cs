using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Assignments;

public sealed class AssignmentsEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly FleetClient _fleet = new(factory.CreateClient());

    [Fact]
    public async Task Assign_ThenGetCurrent_ReturnsTheDriver()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        var driverId = await _fleet.CreateDriverAsync("C2");

        var response = await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), driverId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(response.Headers.ETag);

        var current = await _fleet.Http.GetAsync($"/api/trucks/{truckId}/assignment");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        var body = await current.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal(driverId.ToString(), body!["driverId"]!.GetValue<string>());
        Assert.Null(body["releasedAt"]?.GetValue<string?>());
    }

    [Fact]
    public async Task Assign_WithoutIfMatch_ReturnsPreconditionRequired()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        var driverId = await _fleet.CreateDriverAsync("C2");

        var response = await _fleet.Http.PutAsJsonAsync($"/api/trucks/{truckId}/assignment", new { driverId });

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
    }

    [Fact]
    public async Task Assign_WithStaleTruckVersion_ReturnsPreconditionFailed()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        var staleEtag = await _fleet.ReadTruckEtagAsync(truckId);
        await _fleet.ChangeTruckStatusAsync(truckId, "Maintenance", staleEtag);
        var driverId = await _fleet.CreateDriverAsync("C2");

        var response = await _fleet.AssignAsync(truckId, staleEtag, driverId);

        Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
    }

    [Fact]
    public async Task Assign_TruckAlreadyAssigned_ReturnsConflict()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), await _fleet.CreateDriverAsync("C2"));

        var response = await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), await _fleet.CreateDriverAsync("C2"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertProblemMentionsAsync(response, "ya tiene un conductor asignado");
    }

    [Fact]
    public async Task Assign_DriverAlreadyAssignedToAnotherTruck_ReturnsConflict()
    {
        var driverId = await _fleet.CreateDriverAsync("C2");
        var firstTruck = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        await _fleet.AssignAsync(firstTruck, await _fleet.ReadTruckEtagAsync(firstTruck), driverId);

        var secondTruck = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        var response = await _fleet.AssignAsync(secondTruck, await _fleet.ReadTruckEtagAsync(secondTruck), driverId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertProblemMentionsAsync(response, "ya está asignado a otro camión");
    }

    [Fact]
    public async Task Assign_DriverLicenseBelowTruckRequirement_ReturnsConflict()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate(), "Articulated");
        var driverId = await _fleet.CreateDriverAsync("C2");

        var response = await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), driverId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertProblemMentionsAsync(response, "no alcanza la categoría");
    }

    [Fact]
    public async Task Assign_TruckInRoute_ReturnsConflict()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        await _fleet.ChangeTruckStatusAsync(truckId, "InRoute", await _fleet.ReadTruckEtagAsync(truckId));

        var response = await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), await _fleet.CreateDriverAsync("C2"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Release_ThenCurrentIsGone_AndHistoryKeepsTheAssignment()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), await _fleet.CreateDriverAsync("C2"));

        var release = await _fleet.ReleaseAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId));

        Assert.Equal(HttpStatusCode.NoContent, release.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _fleet.Http.GetAsync($"/api/trucks/{truckId}/assignment")).StatusCode);

        var history = await _fleet.ListAssignmentsAsync($"truckId={truckId}&active=false");
        var released = Assert.Single(history!["items"]!.AsArray());
        Assert.Equal("Manual", released!["releaseReason"]!.GetValue<string>());
    }

    [Fact]
    public async Task Release_WithoutActiveAssignment_ReturnsNotFound()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());

        var response = await _fleet.ReleaseAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Assign_ParallelRequestsForTheSameTruck_OnlyOneSucceeds()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        var etag = await _fleet.ReadTruckEtagAsync(truckId);
        var firstDriver = await _fleet.CreateDriverAsync("C2");
        var secondDriver = await _fleet.CreateDriverAsync("C2");

        // Las dos llevan la misma versión del camión: pasan la comprobación If-Match y compiten en el índice único.
        var responses = await Task.WhenAll(
            _fleet.AssignAsync(truckId, etag, firstDriver),
            _fleet.AssignAsync(truckId, etag, secondDriver));

        var statuses = responses.Select(response => response.StatusCode).OrderBy(status => (int)status).ToList();
        Assert.Equal([HttpStatusCode.NoContent, HttpStatusCode.Conflict], statuses);

        var active = await _fleet.ListAssignmentsAsync($"truckId={truckId}&active=true");
        Assert.Single(active!["items"]!.AsArray());
    }

    [Fact]
    public async Task List_FilteredByActive_ReturnsOnlyActiveAssignments()
    {
        var truckId = await _fleet.CreateTruckAsync(FleetClient.NewPlate());
        var driverId = await _fleet.CreateDriverAsync("C2");
        await _fleet.AssignAsync(truckId, await _fleet.ReadTruckEtagAsync(truckId), driverId);

        var body = await _fleet.ListAssignmentsAsync($"truckId={truckId}&active=true&sort=-assignedAt");

        var item = Assert.Single(body!["items"]!.AsArray());
        Assert.Equal(driverId.ToString(), item!["driverId"]!.GetValue<string>());
    }

    private static async Task AssertProblemMentionsAsync(HttpResponseMessage response, string fragment)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Contains(fragment, body!["detail"]!.GetValue<string>(), StringComparison.Ordinal);
    }
}
