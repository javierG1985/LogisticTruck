using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LogiTrack.Api.IntegrationTests.Infrastructure;

namespace LogiTrack.Api.IntegrationTests.Controllers.Trucks;

public sealed class TrucksSearchTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Search_IgnoresCaseAndMatchesPartOfTheBrand()
    {
        var plate = NewPlate();
        await CreateTruckAsync(plate, brand: "Freightliner Cascadia");

        var response = await _client.GetAsync("/api/trucks?search=FREIGHT&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.Contains(body!["items"]!.AsArray(), item => item!["plate"]!.GetValue<string>() == plate);
    }

    [Fact]
    public async Task Search_TreatsLikeWildcardsAsText()
    {
        await CreateTruckAsync(NewPlate(), brand: "Hino 500");

        // Sin escapar, "%" coincidiría con todas las marcas.
        var body = await _client.GetFromJsonAsync<JsonNode>("/api/trucks?search=%25");

        Assert.Equal(0, body!["totalCount"]!.GetValue<int>());
    }

    [Fact]
    public async Task Search_WithPlate_FindsTheExactTruck()
    {
        var plate = NewPlate();
        await CreateTruckAsync(plate, brand: "Chevrolet NPR");

        var body = await _client.GetFromJsonAsync<JsonNode>($"/api/trucks?search={plate}");

        Assert.Contains(body!["items"]!.AsArray(), item => item!["plate"]!.GetValue<string>() == plate);
    }

    private async Task CreateTruckAsync(string plate, string brand)
    {
        var response = await _client.PostAsJsonAsync("/api/trucks", new
        {
            plate,
            brand,
            modelYear = 2022,
            capacityKg = 8000,
            volumeM3 = 30,
            soatExpiresOn = "2027-01-31",
            techReviewExpiresOn = "2027-06-30",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // Placa aleatoria ABC123 con letras y números válidos; evita choques entre pruebas.
    private static string NewPlate()
    {
        const string letters = "ABCDEFGHJKLMNPRSTUVWXYZ";
        var random = Random.Shared;
        var prefix = new string([letters[random.Next(letters.Length)], letters[random.Next(letters.Length)], letters[random.Next(letters.Length)]]);
        return prefix + random.Next(100, 1000).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
