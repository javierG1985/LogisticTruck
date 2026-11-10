using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Assignments;

public sealed class AssignmentsEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Assign_ThenGetCurrent_ReturnsTheDriver()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        var driverId = await CreateDriverAsync("C2");

        var response = await AssignAsync(truckId, await ReadTruckEtagAsync(truckId), driverId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(response.Headers.ETag);

        var current = await _client.GetAsync($"/api/trucks/{truckId}/assignment");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        var body = await current.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal(driverId.ToString(), body!["driverId"]!.GetValue<string>());
        Assert.Null(body["releasedAt"]?.GetValue<string?>());
    }

    [Fact]
    public async Task Assign_WithoutIfMatch_ReturnsPreconditionRequired()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        var driverId = await CreateDriverAsync("C2");

        var response = await _client.PutAsJsonAsync($"/api/trucks/{truckId}/assignment", new { driverId });

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
    }

    [Fact]
    public async Task Assign_WithStaleTruckVersion_ReturnsPreconditionFailed()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        var staleEtag = await ReadTruckEtagAsync(truckId);
        await ChangeTruckStatusAsync(truckId, "Maintenance", staleEtag);
        var driverId = await CreateDriverAsync("C2");

        var response = await AssignAsync(truckId, staleEtag, driverId);

        Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
    }

    [Fact]
    public async Task Assign_TruckAlreadyAssigned_ReturnsConflict()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        await AssignAsync(truckId, await ReadTruckEtagAsync(truckId), await CreateDriverAsync("C2"));

        var response = await AssignAsync(truckId, await ReadTruckEtagAsync(truckId), await CreateDriverAsync("C2"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertProblemMentionsAsync(response, "ya tiene un conductor asignado");
    }

    [Fact]
    public async Task Assign_DriverAlreadyAssignedToAnotherTruck_ReturnsConflict()
    {
        var driverId = await CreateDriverAsync("C2");
        var firstTruck = await CreateTruckAsync(NewPlate(), "Rigid");
        await AssignAsync(firstTruck, await ReadTruckEtagAsync(firstTruck), driverId);

        var secondTruck = await CreateTruckAsync(NewPlate(), "Rigid");
        var response = await AssignAsync(secondTruck, await ReadTruckEtagAsync(secondTruck), driverId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertProblemMentionsAsync(response, "ya está asignado a otro camión");
    }

    [Fact]
    public async Task Assign_DriverLicenseBelowTruckRequirement_ReturnsConflict()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Articulated");
        var driverId = await CreateDriverAsync("C2");

        var response = await AssignAsync(truckId, await ReadTruckEtagAsync(truckId), driverId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertProblemMentionsAsync(response, "no alcanza la categoría");
    }

    [Fact]
    public async Task Assign_TruckInRoute_ReturnsConflict()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        await ChangeTruckStatusAsync(truckId, "InRoute", await ReadTruckEtagAsync(truckId));

        var response = await AssignAsync(truckId, await ReadTruckEtagAsync(truckId), await CreateDriverAsync("C2"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Release_ThenCurrentIsGone_AndHistoryKeepsTheAssignment()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        await AssignAsync(truckId, await ReadTruckEtagAsync(truckId), await CreateDriverAsync("C2"));

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/trucks/{truckId}/assignment");
        request.Headers.TryAddWithoutValidation("If-Match", await ReadTruckEtagAsync(truckId));
        var release = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, release.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/trucks/{truckId}/assignment")).StatusCode);

        var history = await _client.GetFromJsonAsync<JsonNode>($"/api/assignments?truckId={truckId}&active=false");
        var released = Assert.Single(history!["items"]!.AsArray());
        Assert.Equal("Manual", released!["releaseReason"]!.GetValue<string>());
    }

    [Fact]
    public async Task Release_WithoutActiveAssignment_ReturnsNotFound()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/trucks/{truckId}/assignment");
        request.Headers.TryAddWithoutValidation("If-Match", await ReadTruckEtagAsync(truckId));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Assign_ParallelRequestsForTheSameTruck_OnlyOneSucceeds()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        var etag = await ReadTruckEtagAsync(truckId);
        var firstDriver = await CreateDriverAsync("C2");
        var secondDriver = await CreateDriverAsync("C2");

        // Las dos llevan la misma versión del camión: pasan la comprobación If-Match y compiten en el índice único.
        var responses = await Task.WhenAll(
            AssignAsync(truckId, etag, firstDriver),
            AssignAsync(truckId, etag, secondDriver));

        var statuses = responses.Select(response => response.StatusCode).OrderBy(status => (int)status).ToList();
        Assert.Equal([HttpStatusCode.NoContent, HttpStatusCode.Conflict], statuses);

        var active = await _client.GetFromJsonAsync<JsonNode>($"/api/assignments?truckId={truckId}&active=true");
        Assert.Single(active!["items"]!.AsArray());
    }

    [Fact]
    public async Task List_FilteredByActive_ReturnsOnlyActiveAssignments()
    {
        var truckId = await CreateTruckAsync(NewPlate(), "Rigid");
        var driverId = await CreateDriverAsync("C2");
        await AssignAsync(truckId, await ReadTruckEtagAsync(truckId), driverId);

        var body = await _client.GetFromJsonAsync<JsonNode>($"/api/assignments?truckId={truckId}&active=true&sort=-assignedAt");

        var item = Assert.Single(body!["items"]!.AsArray());
        Assert.Equal(driverId.ToString(), item!["driverId"]!.GetValue<string>());
    }

    private async Task<Guid> CreateTruckAsync(string plate, string truckType)
    {
        var response = await _client.PostAsJsonAsync("/api/trucks", new
        {
            plate,
            brand = "Kenworth",
            truckType,
            modelYear = 2022,
            capacityKg = 8000,
            volumeM3 = 30,
            soatExpiresOn = "2027-01-31",
            techReviewExpiresOn = "2027-06-30",
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        return body!["id"]!.GetValue<Guid>();
    }

    private async Task<Guid> CreateDriverAsync(string licenseCategory)
    {
        var response = await _client.PostAsJsonAsync("/api/drivers", new
        {
            documentNumber = NewDocumentNumber(),
            firstName = "Ana",
            lastName = "Gómez Ruiz",
            address = "Calle 10 # 20-30",
            phone = "3001234567",
            email = "ana@example.com",
            birthDate = "1981-01-01",
            licenseCategory,
            licenseIssuedOn = "2026-01-15",
            licenseExpiresOn = "2029-01-15",
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        return body!["id"]!.GetValue<Guid>();
    }

    private async Task<string> ReadTruckEtagAsync(Guid truckId)
    {
        var response = await _client.GetAsync($"/api/trucks/{truckId}");
        return response.Headers.ETag!.Tag;
    }

    private async Task<HttpResponseMessage> AssignAsync(Guid truckId, string etag, Guid driverId)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/trucks/{truckId}/assignment")
        {
            Content = JsonContent.Create(new { driverId }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        return await _client.SendAsync(request);
    }

    private async Task ChangeTruckStatusAsync(Guid truckId, string status, string etag)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/trucks/{truckId}/status")
        {
            Content = JsonContent.Create(new { status }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task AssertProblemMentionsAsync(HttpResponseMessage response, string fragment)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Contains(fragment, body!["detail"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    // Placa aleatoria con letras y números válidos; evita choques entre pruebas.
    private static string NewPlate()
    {
        const string letters = "ABCDEFGHJKLMNPRSTUVWXYZ";
        var random = Random.Shared;
        var prefix = new string([letters[random.Next(letters.Length)], letters[random.Next(letters.Length)], letters[random.Next(letters.Length)]]);
        return prefix + random.Next(100, 1000).ToString(CultureInfo.InvariantCulture);
    }

    private static string NewDocumentNumber() =>
        Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(CultureInfo.InvariantCulture);
}
