using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;
using LogiTrack.Domain.Aggregates.Trucks;

namespace LogiTrack.Api.IntegrationTests.Controllers.Trucks;

public sealed class TruckDomainEventsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ChangeStatus_AfterSaveChanges_PublishesTruckStatusChanged()
    {
        var id = await CreateTruckAsync("EVT100");
        var etag = (await _client.GetAsync($"/api/trucks/{id}")).Headers.ETag!.Tag;

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/trucks/{id}/status")
        {
            Content = JsonContent.Create(new { status = "Maintenance" }),
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);

        var response = await _client.SendAsync(request);

        // El evento llega durante la misma petición, después de que el cambio ya está guardado.
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var published = TruckStatusChangedSpy.Received.Where(e => e.TruckId == id).ToList();
        var domainEvent = Assert.Single(published);
        Assert.Equal(TruckStatus.Available, domainEvent.PreviousStatus);
        Assert.Equal(TruckStatus.Maintenance, domainEvent.NewStatus);
    }

    private async Task<Guid> CreateTruckAsync(string plate)
    {
        var response = await _client.PostAsJsonAsync("/api/trucks", new
        {
            plate,
            brand = "Kenworth",
            modelYear = 2022,
            capacityKg = 8000,
            volumeM3 = 30,
            soatExpiresOn = "2027-01-31",
            techReviewExpiresOn = "2027-06-30",
        });

        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        return body!["id"]!.GetValue<Guid>();
    }
}
