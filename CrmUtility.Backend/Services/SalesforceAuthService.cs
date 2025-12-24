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
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public string issued_at { get; set; }
        public string signature { get; set; }
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
            return
                "https://login.salesforce.com/services/oauth2/authorize" +
                $"?response_type=code" +
                $"&client_id={_options.ClientId}" +
                $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                $"&state={Uri.EscapeDataString(state)}";
        }



        public async Task<OAuthConnection> ExchangeCodeAsync(string code, string userId)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["redirect_uri"] = _options.RedirectUri,
                ["code"] = code
            };

            var res = await _client.PostAsync(
                "https://login.salesforce.com/services/oauth2/token",
                new FormUrlEncodedContent(form));

            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadFromJsonAsync<SalesforceTokenResponse>();

            var conn = new OAuthConnection
            {
                UserId = userId,
                Crm = CrmType.Salesforce,
                AccessToken = token.access_token,
                RefreshToken = token.refresh_token,
                InstanceUrl = token.instance_url,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(2),
                UpdatedAtUtc = DateTime.UtcNow
            };


            return await _tokenService.UpsertConnectionAsync(conn);
        }

        public async Task<string> EnsureAccessTokenAsync(string userId)
        {
            var conn = await _tokenService.GetConnectionAsync(userId, CrmType.Salesforce);
            if (conn == null)
                throw new Exception("Salesforce not connected");

            if (conn.ExpiresAtUtc > DateTime.UtcNow.AddMinutes(2))
                return conn.AccessToken;

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = conn.RefreshToken,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            };

            var res = await _client.PostAsync(
                "https://login.salesforce.com/services/oauth2/token",
                new FormUrlEncodedContent(form));

            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadFromJsonAsync<SalesforceTokenResponse>();

            conn.AccessToken = token.access_token;
            conn.InstanceUrl = token.instance_url;
            conn.ExpiresAtUtc = DateTime.UtcNow.AddHours(2);

            await _tokenService.UpsertConnectionAsync(conn);

            return conn.AccessToken;
        }
    }
}
