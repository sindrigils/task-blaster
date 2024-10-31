using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TaskBlaster.TaskManagement.API.Services.Interfaces;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class M2MAuthenticationService : IM2MAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public M2MAuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    public async Task<string?> RetrieveAccessToken(string fullName, string emailAddress)
    {
        var clientId = _configuration["Auth0:ClientId"];
        var clientSecret = _configuration["Auth0:ClientSecret"];
        var auth0Domain = _configuration["Auth0:Authority"];
        var _audience = _configuration["Auth0:AudienceNotifications"] ?? "";

        var request = new HttpRequestMessage(HttpMethod.Post, $"{auth0Domain}/oauth/token");

        var requestBody = new
        {
            client_id = clientId,
            client_secret = clientSecret,
            audience = _audience,
            grant_type = "client_credentials",
            custom_claims = new
            {
                full_name = fullName,
                email_address = emailAddress
            }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
            return tokenResponse?.access_token;
        }
        else
        {
            throw new Exception($"Failed to retrieve M2M access token. Status: {response.StatusCode}, Details: {responseBody}");
        }
    }


    // Response class for token response
    private class TokenResponse
    {
        public string access_token { get; set; } = null!;
        public string token_type { get; set; } = null!;
        public int expires_in { get; set; }
    }
}

