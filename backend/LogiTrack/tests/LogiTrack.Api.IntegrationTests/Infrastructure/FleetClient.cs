using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace LogiTrack.Api.IntegrationTests.Infrastructure;

// Operaciones HTTP sobre camiones, conductores y asignaciones, compartidas por las pruebas de integración de la flota.
public sealed class FleetClient
{
    private readonly HttpClient http;

    public FleetClient(HttpClient http)
    {
        this.http = http;
        Http = http;
    }

    public HttpClient Http { get; }

    public async Task<Guid> CreateTruckAsync(string plate, string truckType = "Rigid")
    {
        var response = await http.PostAsJsonAsync("/api/trucks", new
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

    // Licencia vigente hoy (expedida en 2026, vence en 2029) para un conductor nacido en 1981.
    public async Task<Guid> CreateDriverAsync(string licenseCategory = "C2")
    {
        var response = await http.PostAsJsonAsync("/api/drivers", new
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

    public Task<string> ReadTruckEtagAsync(Guid truckId) => ReadEtagAsync($"/api/trucks/{truckId}");

    public Task<string> ReadDriverEtagAsync(Guid driverId) => ReadEtagAsync($"/api/drivers/{driverId}");

    public Task<HttpResponseMessage> AssignAsync(Guid truckId, string etag, Guid driverId) =>
        SendConditionalAsync(HttpMethod.Put, $"/api/trucks/{truckId}/assignment", etag, new { driverId });

    public Task<HttpResponseMessage> ReleaseAsync(Guid truckId, string etag) =>
        SendConditionalAsync(HttpMethod.Delete, $"/api/trucks/{truckId}/assignment", etag);

    public Task<HttpResponseMessage> ChangeTruckStatusAsync(Guid truckId, string status, string etag) =>
        SendConditionalAsync(HttpMethod.Patch, $"/api/trucks/{truckId}/status", etag, new { status });

    public Task<HttpResponseMessage> DeleteTruckAsync(Guid truckId, string etag) =>
        SendConditionalAsync(HttpMethod.Delete, $"/api/trucks/{truckId}", etag);

    public Task<HttpResponseMessage> ChangeDriverStatusAsync(Guid driverId, string status, string etag) =>
        SendConditionalAsync(HttpMethod.Patch, $"/api/drivers/{driverId}/status", etag, new { status });

    public Task<HttpResponseMessage> DeleteDriverAsync(Guid driverId, string etag) =>
        SendConditionalAsync(HttpMethod.Delete, $"/api/drivers/{driverId}", etag);

    public Task<JsonNode?> ListAssignmentsAsync(string query) =>
        http.GetFromJsonAsync<JsonNode>($"/api/assignments?{query}");

    // Placa aleatoria con letras y números válidos; evita choques entre pruebas.
    public static string NewPlate()
    {
        const string letters = "ABCDEFGHJKLMNPRSTUVWXYZ";
        var random = Random.Shared;
        var prefix = new string([letters[random.Next(letters.Length)], letters[random.Next(letters.Length)], letters[random.Next(letters.Length)]]);
        return prefix + random.Next(100, 1000).ToString(CultureInfo.InvariantCulture);
    }

    public static string NewDocumentNumber() =>
        Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(CultureInfo.InvariantCulture);

    private async Task<string> ReadEtagAsync(string url)
    {
        var response = await http.GetAsync(url);
        return response.Headers.ETag!.Tag;
    }

    private async Task<HttpResponseMessage> SendConditionalAsync(HttpMethod method, string url, string etag, object? body = null)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = body is null ? null : JsonContent.Create(body),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        return await http.SendAsync(request);
    }
}
