using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Drivers;

public sealed class DriversSearchAndConcurrencyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Search_IgnoresCase()
    {
        var lastName = "Zzbusca" + Guid.NewGuid().ToString("N")[..8];
        await CreateDriverAsync(NewDocument(), lastName);

        var body = await _client.GetFromJsonAsync<JsonNode>($"/api/drivers?search={lastName.ToUpperInvariant()}");

        Assert.Equal(1, body!["totalCount"]!.GetValue<int>());
    }

    [Fact]
    public async Task Search_TreatsLikeWildcardsAsText()
    {
        await CreateDriverAsync(NewDocument(), "Zzguion_bajo");

        // Sin escapar, "_" coincidiría con cualquier carácter y encontraría otros conductores.
        var body = await _client.GetFromJsonAsync<JsonNode>("/api/drivers?search=zz_");

        Assert.Equal(0, body!["totalCount"]!.GetValue<int>());
    }

    [Fact]
    public async Task Create_ConcurrentWithTheSameDocument_OneCreatedAndOneConflict()
    {
        var document = NewDocument();

        var responses = await Task.WhenAll(
            _client.PostAsJsonAsync("/api/drivers", NewDriverBody(document)),
            _client.PostAsJsonAsync("/api/drivers", NewDriverBody(document)));

        var statuses = responses.Select(response => response.StatusCode).Order().ToArray();
        Assert.Equal([HttpStatusCode.Created, HttpStatusCode.Conflict], statuses);
    }

    private async Task CreateDriverAsync(string document, string lastName)
    {
        var response = await _client.PostAsJsonAsync("/api/drivers", NewDriverBody(document, lastName));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static string NewDocument() => Random.Shared.Next(10_000_000, 100_000_000).ToString(CultureInfo.InvariantCulture);

    private static object NewDriverBody(string documentNumber, string lastName = "Gómez Ruiz") => new
    {
        documentNumber,
        firstName = "Ana",
        lastName,
        address = "Calle 10 # 20-30",
        phone = "3001234567",
        email = "ana@example.com",
        birthDate = "1981-01-01",
        licenseCategory = "C2",
        licenseIssuedOn = "2026-01-15",
        licenseExpiresOn = "2029-01-15",
    };
}
