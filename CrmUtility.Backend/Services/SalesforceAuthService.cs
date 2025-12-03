using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CrmUtility.Backend.Models;
using CrmUtility.Backend.Options;
using Microsoft.Extensions.Options;

namespace CrmUtility.Backend.Services
{
    public class SalesforceTokenResponse
    {
        public string access_token { get; set; }
        public string instance_url { get; set; }
        public string id { get; set; }
        public string token_type { get; set; }
        public string issued_at { get; set; }
        public string signature { get; set; }
        public string refresh_token { get; set; }
    }

    public class SalesforceAuthService
    {
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;
        private readonly SalesforceOptions _options;

        public SalesforceAuthService(
            IHttpClientFactory httpClientFactory,
            TokenService tokenService,
            IOptions<SalesforceOptions> options)
        {
            _client = httpClientFactory.CreateClient();
            _tokenService = tokenService;
            _options = options.Value;
        }

        public string GetLoginUrl(string state)
        {
            var url =
                "https://login.salesforce.com/services/oauth2/authorize" +
                $"?response_type=code" +
                $"&client_id={_options.ClientId}" +
                $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                $"&state={Uri.EscapeDataString(state)}";

            return url;
        }

        public async Task<OAuthConnection> ExchangeCodeAsync(string code, string userId)
        {
            var data = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["redirect_uri"] = _options.RedirectUri
            };

            var content = new FormUrlEncodedContent(data);

            var response = await _client.PostAsync("https://login.salesforce.com/services/oauth2/token", content);
            response.EnsureSuccessStatusCode();

            var tokenData = await response.Content.ReadFromJsonAsync<SalesforceTokenResponse>();

            var connection = new OAuthConnection
            {
                UserId = userId,
                Crm = CrmType.Salesforce,
                AccessToken = tokenData.access_token,
                RefreshToken = tokenData.refresh_token,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(2),
                InstanceUrl = tokenData.instance_url
            };

            return await _tokenService.UpsertConnectionAsync(connection);
        }
    }
}
