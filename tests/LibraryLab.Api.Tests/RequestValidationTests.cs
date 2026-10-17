using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace LibraryLab.Api.Tests;

public sealed class RequestValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RequestValidationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.ClearProviders())).CreateClient();
    }

    [Fact]
    public async Task Book_creation_rejects_missing_required_fields()
    {
        using var response = await _client.PostAsJsonAsync("/api/books", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
