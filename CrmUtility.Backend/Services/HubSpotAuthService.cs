using CrmUtility.Backend.Models;
using Microsoft.Extensions.Options;
using CrmUtility.Backend.Options;

namespace CrmUtility.Backend.Services
{
    public class HubSpotTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
    }

    public class HubSpotAuthService
    {
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;
        private readonly HubSpotOptions _options;

        public HubSpotAuthService(
            IHttpClientFactory httpClientFactory,
            TokenService tokenService,
            IOptions<HubSpotOptions> options)
        {
            _client = httpClientFactory.CreateClient();
            _tokenService = tokenService;
            _options = options.Value;
        }

        // STEP A: Generate HubSpot login URL
        public string GetLoginUrl(string state)
        {
            var url =
                $"https://app.hubspot.com/oauth/authorize" +
                $"?client_id={_options.ClientId}" +
                $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                $"&scope={Uri.EscapeDataString(_options.Scopes)}" +
                $"&state={state}";

            return url;
        }

        // STEP B: Exchange code for tokens
        public async Task<OAuthConnection> ExchangeCodeAsync(string code, string userId)
        {
            var data = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["redirect_uri"] = _options.RedirectUri,
                ["code"] = code
            };

            var content = new FormUrlEncodedContent(data);

            var response = await _client.PostAsync("https://api.hubapi.com/oauth/v1/token", content);
            response.EnsureSuccessStatusCode();

            var tokenData = await response.Content.ReadFromJsonAsync<HubSpotTokenResponse>();

            // Save to DB
            var connection = new OAuthConnection
            {
                UserId = userId,
                Crm = CrmType.HubSpot,
                AccessToken = tokenData.access_token,
                RefreshToken = tokenData.refresh_token,
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(tokenData.expires_in)
            };

            return await _tokenService.UpsertConnectionAsync(connection);
        }
    }
}
