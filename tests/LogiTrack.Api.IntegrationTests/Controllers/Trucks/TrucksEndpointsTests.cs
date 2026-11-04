using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Trucks;

public sealed class TrucksEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheSameTruck()
    {
        var id = await CreateTruckAsync("TRK100");

        var response = await _client.GetAsync($"/api/trucks/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Headers.ETag);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Equal("TRK100", body!["plate"]!.GetValue<string>());
    }

    [Fact]
    public async Task Create_WithDuplicatePlate_ReturnsConflict()
    {
        await CreateTruckAsync("DUP100");

        var response = await _client.PostAsJsonAsync("/api/trucks", NewTruckBody("DUP100"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequestWithErrorsByField()
    {
        var response = await _client.PostAsJsonAsync("/api/trucks", NewTruckBody(string.Empty, brand: string.Empty));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.NotNull(body!["errors"]!["plate"]);
        Assert.NotNull(body["errors"]!["brand"]);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/trucks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task List_WithPageSizeAndSort_ReturnsOrderedPages()
    {
        foreach (var plate in new[] { "PGA101", "PGB102", "PGC103" })
        {
            await CreateTruckAsync(plate);
        }

        var firstPage = await _client.GetFromJsonAsync<JsonNode>("/api/trucks?page=1&pageSize=2&sort=plate");
        var secondPage = await _client.GetFromJsonAsync<JsonNode>("/api/trucks?page=2&pageSize=2&sort=plate");

        var first = firstPage!["items"]!.AsArray().Select(item => item!["plate"]!.GetValue<string>()).ToList();
        var second = secondPage!["items"]!.AsArray().Select(item => item!["plate"]!.GetValue<string>()).ToList();

        Assert.Equal(2, first.Count);
        Assert.Equal([.. first.Order(StringComparer.Ordinal)], first);
        Assert.True(string.CompareOrdinal(first[^1], second[0]) < 0);
    }

    [Fact]
    public async Task Update_WithValidIfMatch_ReturnsNoContentAndNewEtag()
    {
        var id = await CreateTruckAsync("UPD100");
        var etag = await ReadEtagAsync(id);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/trucks/{id}")
        {
            Content = JsonContent.Create(new
            {
                brand = "Volvo FH",
                modelYear = 2023,
                capacityKg = 9000,
                volumeM3 = 35,
                soatExpiresOn = "2027-02-28",
                techReviewExpiresOn = "2027-07-31",
            }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEqual(etag, response.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task ChangeStatus_WithValidIfMatch_ReturnsNoContentAndNewEtag()
    {
        var id = await CreateTruckAsync("PAT100");
        var etag = await ReadEtagAsync(id);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/trucks/{id}/status")
        {
            Content = JsonContent.Create(new { status = "Maintenance" }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEqual(etag, response.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task Update_WithoutIfMatch_ReturnsPreconditionRequired()
    {
        var id = await CreateTruckAsync("NOI100");

        var response = await _client.PutAsJsonAsync($"/api/trucks/{id}", NewUpdateBody());

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithStaleEtag_ReturnsPreconditionFailed()
    {
        var id = await CreateTruckAsync("STA100");
        var staleEtag = await ReadEtagAsync(id);
        await ChangeStatusAsync(id, "InRoute", await ReadEtagAsync(id));

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/trucks/{id}") { Content = JsonContent.Create(NewUpdateBody()) };
        request.Headers.TryAddWithoutValidation("If-Match", staleEtag);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_FromMaintenanceToInRoute_ReturnsConflict()
    {
        var id = await CreateTruckAsync("TRN100");
        await ChangeStatusAsync(id, "Maintenance", await ReadEtagAsync(id));

        var response = await ChangeStatusAsync(id, "InRoute", await ReadEtagAsync(id));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGetAndList_TruckIsGone()
    {
        var id = await CreateTruckAsync("DEL100");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/trucks/{id}");
        request.Headers.TryAddWithoutValidation("If-Match", await ReadEtagAsync(id));
        var deleteResponse = await _client.SendAsync(request);

        var getResponse = await _client.GetAsync($"/api/trucks/{id}");
        var list = await _client.GetFromJsonAsync<JsonNode>("/api/trucks?pageSize=100");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.DoesNotContain(list!["items"]!.AsArray(), item => item!["id"]!.GetValue<string>() == id.ToString());
    }

    [Fact]
    public async Task Delete_WithoutIfMatch_ReturnsPreconditionRequired()
    {
        var id = await CreateTruckAsync("DNO100");

        var response = await _client.DeleteAsync($"/api/trucks/{id}");

        Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
    }

    private async Task<Guid> CreateTruckAsync(string plate)
    {
        var response = await _client.PostAsJsonAsync("/api/trucks", NewTruckBody(plate));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        return body!["id"]!.GetValue<Guid>();
    }

    private async Task<string> ReadEtagAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/trucks/{id}");
        return response.Headers.ETag!.Tag;
    }

    private async Task<HttpResponseMessage> ChangeStatusAsync(Guid id, string status, string etag)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/trucks/{id}/status")
        {
            Content = JsonContent.Create(new { status }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        return await _client.SendAsync(request);
    }

    private static object NewTruckBody(string plate, string brand = "Kenworth") => new
    {
        plate,
        brand,
        modelYear = 2022,
        capacityKg = 8000,
        volumeM3 = 30,
        soatExpiresOn = "2027-01-31",
        techReviewExpiresOn = "2027-06-30",
    };

    private static object NewUpdateBody() => new
    {
        brand = "Kenworth",
        modelYear = 2022,
        capacityKg = 8000,
        volumeM3 = 30,
        soatExpiresOn = "2027-01-31",
        techReviewExpiresOn = "2027-06-30",
    };
}
