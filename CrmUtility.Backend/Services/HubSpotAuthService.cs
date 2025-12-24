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

     
        public string GetLoginUrl(string state)
        {
            return
                $"https://app.hubspot.com/oauth/authorize" +
                $"?client_id={_options.ClientId}" +
                $"&scope={Uri.EscapeDataString(_options.Scopes)}" +
                $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                $"&state={state}";
        }

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

            var res = await _client.PostAsync(
                "https://api.hubapi.com/oauth/v1/token",
                new FormUrlEncodedContent(data));

            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadFromJsonAsync<HubSpotTokenResponse>();

            if (token == null)
                throw new Exception("Invalid HubSpot token response");

            var conn = new OAuthConnection
            {
                UserId = userId,
                Crm = CrmType.HubSpot,
                AccessToken = token.access_token,
                RefreshToken = token.refresh_token,
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(token.expires_in),
                UpdatedAtUtc = DateTime.UtcNow
            };

            return await _tokenService.UpsertConnectionAsync(conn);
        }

        public async Task<string> EnsureAccessTokenAsync(string userId)
        {
            var conn = await _tokenService.GetConnectionAsync(userId, CrmType.HubSpot);
            if (conn == null)
                throw new Exception("HubSpot not connected");

            
            if (conn.ExpiresAtUtc > DateTime.UtcNow.AddMinutes(2))
                return conn.AccessToken;

            
            var data = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = conn.RefreshToken,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            };

            var res = await _client.PostAsync(
                "https://api.hubapi.com/oauth/v1/token",
                new FormUrlEncodedContent(data));

            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadFromJsonAsync<HubSpotTokenResponse>();

            conn.AccessToken = token.access_token;
            conn.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(token.expires_in);
            conn.UpdatedAtUtc = DateTime.UtcNow;

            await _tokenService.UpsertConnectionAsync(conn);

            return conn.AccessToken;
        }
    }
}
