using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CrmUtility.Backend.Data;
using CrmUtility.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmUtility.Backend.Services
{
    public class HubSpotCrmService
    {
        private readonly HttpClient _client;
        private readonly AppDbContext _db;

        public HubSpotCrmService(IHttpClientFactory httpClientFactory, AppDbContext db)
        {
            _client = httpClientFactory.CreateClient();
            _db = db;
        }

      
        public async Task<string> CreateContactAsync(StandardContactDto contact, string userId)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            var connection = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == CrmType.HubSpot);

            if (connection == null)
                throw new InvalidOperationException("No HubSpot connection found for user " + userId);

            var url = "https://api.hubapi.com/crm/v3/objects/contacts";

            var payload = new
            {
                properties = new Dictionary<string, object?>
                {
                    ["firstname"] = contact.FirstName,
                    ["lastname"] = contact.LastName,
                    ["email"] = contact.Email,
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", connection.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            return result != null && result.TryGetValue("id", out var idObj)
                ? idObj?.ToString()
                : null;
        }

        
        public async Task<List<StandardContactDto>> GetContactsAsync(string userId)
        {
            var connection = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == CrmType.HubSpot);

            if (connection == null)
                throw new Exception("HubSpot is not connected for this user.");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", connection.AccessToken);

            string url =
                "https://api.hubapi.com/crm/v3/objects/contacts" +
                "?limit=10" +
                "&properties=firstname,lastname,email,company,hubspot_owner_id";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement.GetProperty("results");

            var list = new List<StandardContactDto>();

            foreach (var item in root.EnumerateArray())
            {
                var id = item.GetProperty("id").GetString();
                var props = item.GetProperty("properties");

                list.Add(new StandardContactDto
                {
                    Crm = "hubspot",
                    Type = "contact",
                    Id = id,
                    FirstName = props.TryGetProperty("firstname", out var fn) ? fn.GetString() : "",
                    LastName = props.TryGetProperty("lastname", out var ln) ? ln.GetString() : "",
                    Email = props.TryGetProperty("email", out var em) ? em.GetString() : "",
                    Company = props.TryGetProperty("company", out var cm) ? cm.GetString() : "",
                    Owner = props.TryGetProperty("hubspot_owner_id", out var ow) ? ow.GetString() : ""
                });
            }

            return list;
        }

      
        public async Task<List<StandardCompanyDto>> GetCompaniesAsync(string userId)
        {
            var connection = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == CrmType.HubSpot);

            if (connection == null)
                throw new Exception("HubSpot is not connected for this user.");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", connection.AccessToken);

            string url =
                "https://api.hubapi.com/crm/v3/objects/companies" +
                "?limit=10" +
                "&properties=name,domain,hubspot_owner_id";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement.GetProperty("results");

            var list = new List<StandardCompanyDto>();

            foreach (var item in root.EnumerateArray())
            {
                var id = item.GetProperty("id").GetString();
                var props = item.GetProperty("properties");

                list.Add(new StandardCompanyDto
                {
                    Crm = "hubspot",
                    Id = id,
                    Name = props.TryGetProperty("name", out var name) ? name.GetString() : "",
                    Domain = props.TryGetProperty("domain", out var domain) ? domain.GetString() : "",
                    Owner = props.TryGetProperty("hubspot_owner_id", out var owner) ? owner.GetString() : ""
                });
            }

            return list;
        }

     
        public async Task<List<StandardDealDto>> GetDealsAsync(string userId)
        {
            var connection = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == CrmType.HubSpot);

            if (connection == null)
                throw new Exception("HubSpot is not connected for this user.");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", connection.AccessToken);

            string url =
                "https://api.hubapi.com/crm/v3/objects/deals" +
                "?limit=10" +
                "&properties=dealname,dealstage,amount,pipeline,hubspot_owner_id";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement.GetProperty("results");

            var list = new List<StandardDealDto>();

            foreach (var item in root.EnumerateArray())
            {
                var id = item.GetProperty("id").GetString();
                var props = item.GetProperty("properties");

                decimal? amount = null;
                if (props.TryGetProperty("amount", out var amElement))
                {
                    var amStr = amElement.GetString();
                    if (!string.IsNullOrWhiteSpace(amStr) &&
                        decimal.TryParse(amStr, out var parsed))
                    {
                        amount = parsed;
                    }
                }

                list.Add(new StandardDealDto
                {
                    Crm = "hubspot",
                    Id = id,
                    DealName = props.TryGetProperty("dealname", out var dn) ? dn.GetString() : "",
                    Stage = props.TryGetProperty("dealstage", out var st) ? st.GetString() : "",
                    Amount = amount,
                    Pipeline = props.TryGetProperty("pipeline", out var pl) ? pl.GetString() : "",
                    Owner = props.TryGetProperty("hubspot_owner_id", out var ow) ? ow.GetString() : ""
                });
            }

            return list;
        }
       
        public async Task<string> CreateCompanyAsync(StandardCompanyDto company, string userId)
        {
            if (company == null) throw new ArgumentNullException(nameof(company));

            var connection = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == CrmType.HubSpot);

            if (connection == null)
                throw new InvalidOperationException("No HubSpot connection found for user " + userId);

            var url = "https://api.hubapi.com/crm/v3/objects/companies";

            var payload = new
            {
                properties = new Dictionary<string, object?>
                {
                    ["name"] = company.Name,
                    ["domain"] = company.Domain
                 
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result != null && result.TryGetValue("id", out var idObj) ? idObj?.ToString() : null;
        }

    
        public async Task<string> CreateDealAsync(StandardDealDto deal, string userId)
        {
            if (deal == null) throw new ArgumentNullException(nameof(deal));

            var connection = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == CrmType.HubSpot);

            if (connection == null)
                throw new InvalidOperationException("No HubSpot connection found for user " + userId);

            var url = "https://api.hubapi.com/crm/v3/objects/deals";

            
            var properties = new Dictionary<string, object?>()
            {
                ["dealname"] = deal.DealName,
                ["dealstage"] = deal.Stage,
                ["pipeline"] = deal.Pipeline
            };

            if (deal.Amount.HasValue)
                properties["amount"] = deal.Amount.Value;

            var payload = new
            {
                properties = properties,
                
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result != null && result.TryGetValue("id", out var idObj) ? idObj?.ToString() : null;
        }

    }
}
