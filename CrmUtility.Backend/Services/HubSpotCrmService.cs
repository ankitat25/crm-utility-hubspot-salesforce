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

        private async Task<string> GetToken(string userId)
        {
            var conn = await _db.OAuthConnections
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Crm == CrmType.HubSpot);

            if (conn == null)
                throw new InvalidOperationException("HubSpot not connected");

            return conn.AccessToken;
        }

        // ---------------- CONTACTS ----------------

        public async Task<string> CreateContactAsync(StandardContactDto contact, string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(HttpMethod.Post,
                "https://api.hubapi.com/crm/v3/objects/contacts")
            {
                Content = JsonContent.Create(new
                {
                    properties = new
                    {
                        firstname = contact.FirstName,
                        lastname = contact.LastName,
                        email = contact.Email
                    }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result?["id"]?.ToString();
        }

        public async Task<string> UpdateContactAsync(StandardContactDto contact, string userId)
        {
            var token = await GetToken(userId);

            var props = new Dictionary<string, object?>
            {
                ["firstname"] = contact.FirstName,
                ["lastname"] = contact.LastName,
                ["email"] = contact.Email
            };

            var req = new HttpRequestMessage(
                HttpMethod.Patch,
                $"https://api.hubapi.com/crm/v3/objects/contacts/{contact.Id}")
            {
                Content = JsonContent.Create(new { properties = props })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            return contact.Id;
        }

        public async Task<List<StandardContactDto>> GetContactsAsync(string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.hubapi.com/crm/v3/objects/contacts/search")
            {
                Content = JsonContent.Create(new
                {
                    limit = 10,
                    sorts = new[]
                    {
                        new { propertyName = "createdAt", direction = "DESCENDING" }
                    },
                    properties = new[]
                    {
                        "firstname", "lastname", "email", "company"
                    }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var list = new List<StandardContactDto>();

            foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
            {
                var props = item.GetProperty("properties");

                list.Add(new StandardContactDto
                {
                    Crm = "hubspot",
                    Type = "contact",
                    Id = item.GetProperty("id").GetString(),
                    FirstName = props.GetProperty("firstname").GetString(),
                    LastName = props.GetProperty("lastname").GetString(),
                    Email = props.GetProperty("email").GetString()
                });
            }

            return list;
        }

        // ---------------- COMPANIES ----------------

        public async Task<string> CreateCompanyAsync(StandardCompanyDto company, string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.hubapi.com/crm/v3/objects/companies")
            {
                Content = JsonContent.Create(new
                {
                    properties = new
                    {
                        name = company.Name,
                        domain = company.Domain
                    }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result?["id"]?.ToString();
        }

        public async Task<string> UpdateCompanyAsync(StandardCompanyDto company, string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(
                HttpMethod.Patch,
                $"https://api.hubapi.com/crm/v3/objects/companies/{company.Id}")
            {
                Content = JsonContent.Create(new
                {
                    properties = new
                    {
                        name = company.Name,
                        domain = company.Domain
                    }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            return company.Id;
        }

        public async Task<List<StandardCompanyDto>> GetCompaniesAsync(string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.hubapi.com/crm/v3/objects/companies/search")
            {
                Content = JsonContent.Create(new
                {
                    limit = 10,
                    sorts = new[]
                    {
                        new { propertyName = "createdAt", direction = "DESCENDING" }
                    },
                    properties = new[] { "name", "domain" }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var list = new List<StandardCompanyDto>();

            foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
            {
                var props = item.GetProperty("properties");

                list.Add(new StandardCompanyDto
                {
                    Crm = "hubspot",
                    Type = "company",
                    Id = item.GetProperty("id").GetString(),
                    Name = props.GetProperty("name").GetString(),
                    Domain = props.GetProperty("domain").GetString()
                });
            }

            return list;
        }

        // ---------------- DEALS ----------------

        public async Task<string> CreateDealAsync(StandardDealDto deal, string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.hubapi.com/crm/v3/objects/deals")
            {
                Content = JsonContent.Create(new
                {
                    properties = new
                    {
                        dealname = deal.DealName,
                        dealstage = deal.Stage ?? "appointmentscheduled",
                        amount = deal.Amount
                    }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result?["id"]?.ToString();
        }

        public async Task<string> UpdateDealAsync(StandardDealDto deal, string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(
                HttpMethod.Patch,
                $"https://api.hubapi.com/crm/v3/objects/deals/{deal.Id}")
            {
                Content = JsonContent.Create(new
                {
                    properties = new
                    {
                        dealname = deal.DealName,
                        dealstage = deal.Stage,
                        amount = deal.Amount
                    }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            return deal.Id;
        }

        public async Task<List<StandardDealDto>> GetDealsAsync(string userId)
        {
            var token = await GetToken(userId);

            var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.hubapi.com/crm/v3/objects/deals/search")
            {
                Content = JsonContent.Create(new
                {
                    limit = 10,
                    sorts = new[]
                    {
                        new { propertyName = "createdAt", direction = "DESCENDING" }
                    },
                    properties = new[] { "dealname", "dealstage", "amount" }
                })
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var list = new List<StandardDealDto>();

            foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
            {
                var props = item.GetProperty("properties");

                decimal.TryParse(props.GetProperty("amount").GetString(), out var amt);

                list.Add(new StandardDealDto
                {
                    Crm = "hubspot",
                    Type = "deal",
                    Id = item.GetProperty("id").GetString(),
                    DealName = props.GetProperty("dealname").GetString(),
                    Stage = props.GetProperty("dealstage").GetString(),
                    Amount = amt
                });
            }

            return list;
        }
    }
}
