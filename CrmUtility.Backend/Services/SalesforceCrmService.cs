using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CrmUtility.Backend.Models;
using CrmUtility.Backend.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace CrmUtility.Backend.Services
{
    public class SalesforceQueryResult<T>
    {
        public List<T> records { get; set; }
    }

    public class SalesforceUser
    {
        public string Name { get; set; }
    }

    public class SalesforceAccount
    {
        public string Name { get; set; }
    }

    public class SalesforceContactRecord
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public SalesforceAccount Account { get; set; }
        public SalesforceUser Owner { get; set; }
    }

    public class SalesforceAccountRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public SalesforceUser Owner { get; set; }
    }

    public class SalesforceOpportunityRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StageName { get; set; }
        public decimal? Amount { get; set; }
        public SalesforceUser Owner { get; set; }
    }

    public class SalesforceCrmService
    {
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;
        private readonly SalesforceOptions _options;

        public SalesforceCrmService(
            IHttpClientFactory httpClientFactory,
            TokenService tokenService,
            IOptions<SalesforceOptions> options)
        {
            _client = httpClientFactory.CreateClient();
            _tokenService = tokenService;
            _options = options.Value;
        }

        private async Task<(string accessToken, string instanceUrl)> Resolve(string userId)
        {
            var conn = await _tokenService.GetConnectionAsync(userId, CrmType.Salesforce);
            if (conn == null) throw new Exception("Salesforce not connected");
            return (conn.AccessToken, conn.InstanceUrl);
        }

       
        public async Task<List<StandardContactDto>> GetContactsAsync(string userId)
        {
            var (token, urlBase) = await Resolve(userId);

            string soql =
                "SELECT Id, FirstName, LastName, Email, Account.Name, Owner.Name FROM Contact LIMIT 10";

            var req = new HttpRequestMessage(HttpMethod.Get,
                $"{urlBase}/services/data/{_options.ApiVersion}/query?q={Uri.EscapeDataString(soql)}");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var data =
                await res.Content.ReadFromJsonAsync<SalesforceQueryResult<SalesforceContactRecord>>();

            var list = new List<StandardContactDto>();

            foreach (var r in data.records)
            {
                list.Add(new StandardContactDto
                {
                    Crm = "salesforce",
                    Type = "contact",
                    Id = r.Id,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Email = r.Email,
                    Company = r.Account?.Name,
                    Owner = r.Owner?.Name
                });
            }

            return list;
        }

        public async Task<string> CreateContactAsync(StandardContactDto dto, string userId)
        {
            var (token, urlBase) = await Resolve(userId);

            var payload = new Dictionary<string, object?>
            {
                ["FirstName"] = dto.FirstName,
                ["LastName"] = string.IsNullOrWhiteSpace(dto.LastName) ? "Unknown" : dto.LastName,
                ["Email"] = dto.Email
            };

            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{urlBase}/services/data/{_options.ApiVersion}/sobjects/Contact")
            {
                Content = JsonContent.Create(payload)
            };

            req.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result?["id"]?.ToString();
        }

        public async Task<string> UpdateContactAsync(StandardContactDto dto, string userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
                throw new Exception("Contact Id required");

            var (token, urlBase) = await Resolve(userId);

            var payload = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                payload["FirstName"] = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                payload["LastName"] = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                payload["Email"] = dto.Email;

            var req = new HttpRequestMessage(HttpMethod.Patch,
                $"{urlBase}/services/data/{_options.ApiVersion}/sobjects/Contact/{dto.Id}")
            {
                Content = JsonContent.Create(payload)
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            return dto.Id;
        }

        public async Task<List<StandardCompanyDto>> GetCompaniesAsync(string userId)
        {
            var (token, urlBase) = await Resolve(userId);

            string soql =
                "SELECT Id, Name, Website, Owner.Name FROM Account LIMIT 10";

            var req = new HttpRequestMessage(HttpMethod.Get,
                $"{urlBase}/services/data/{_options.ApiVersion}/query?q={Uri.EscapeDataString(soql)}");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var data =
                await res.Content.ReadFromJsonAsync<SalesforceQueryResult<SalesforceAccountRecord>>();

            var list = new List<StandardCompanyDto>();

            foreach (var r in data.records)
            {
                list.Add(new StandardCompanyDto
                {
                    Crm = "salesforce",
                    Type = "company",
                    Id = r.Id,
                    Name = r.Name,
                    Domain = r.Website,
                    Owner = r.Owner?.Name
                });
            }

            return list;
        }

        public async Task<string> CreateCompanyAsync(StandardCompanyDto dto, string userId)
        {
            var (token, urlBase) = await Resolve(userId);

            var payload = new Dictionary<string, object?>
            {
                ["Name"] = dto.Name,
                ["Website"] = dto.Domain
            };

            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{urlBase}/services/data/{_options.ApiVersion}/sobjects/Account")
            {
                Content = JsonContent.Create(payload)
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result?["id"]?.ToString();
        }

        public async Task<string> UpdateCompanyAsync(StandardCompanyDto dto, string userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
                throw new Exception("Company Id required");

            var (token, urlBase) = await Resolve(userId);

            var payload = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(dto.Name))
                payload["Name"] = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Domain))
                payload["Website"] = dto.Domain;

            var req = new HttpRequestMessage(HttpMethod.Patch,
                $"{urlBase}/services/data/{_options.ApiVersion}/sobjects/Account/{dto.Id}")
            {
                Content = JsonContent.Create(payload)
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            return dto.Id;
        }

        
        public async Task<List<StandardDealDto>> GetDealsAsync(string userId)
        {
            var (token, urlBase) = await Resolve(userId);

            string soql =
                "SELECT Id, Name, StageName, Amount, Owner.Name FROM Opportunity LIMIT 10";

            var req = new HttpRequestMessage(HttpMethod.Get,
                $"{urlBase}/services/data/{_options.ApiVersion}/query?q={Uri.EscapeDataString(soql)}");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var data =
                await res.Content.ReadFromJsonAsync<SalesforceQueryResult<SalesforceOpportunityRecord>>();

            var list = new List<StandardDealDto>();

            foreach (var r in data.records)
            {
                list.Add(new StandardDealDto
                {
                    Crm = "salesforce",
                    Type = "deal",
                    Id = r.Id,
                    DealName = r.Name,
                    Stage = r.StageName,
                    Amount = r.Amount,
                    Owner = r.Owner?.Name
                });
            }

            return list;
        }

        public async Task<string> CreateDealAsync(StandardDealDto dto, string userId)
        {
            var (token, urlBase) = await Resolve(userId);

            var payload = new Dictionary<string, object?>
            {
                ["Name"] = dto.DealName,
                ["StageName"] = string.IsNullOrWhiteSpace(dto.Stage) ? "Prospecting" : dto.Stage,
                ["CloseDate"] = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")
            };

            if (dto.Amount.HasValue)
                payload["Amount"] = dto.Amount.Value;

            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{urlBase}/services/data/{_options.ApiVersion}/sobjects/Opportunity")
            {
                Content = JsonContent.Create(payload)
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result?["id"]?.ToString();
        }

        public async Task<string> UpdateDealAsync(StandardDealDto dto, string userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
                throw new Exception("Deal Id required");

            var (token, urlBase) = await Resolve(userId);

            var payload = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(dto.DealName))
                payload["Name"] = dto.DealName;

            if (!string.IsNullOrWhiteSpace(dto.Stage))
                payload["StageName"] = dto.Stage;

            if (dto.Amount.HasValue)
                payload["Amount"] = dto.Amount.Value;

            var req = new HttpRequestMessage(HttpMethod.Patch,
                $"{urlBase}/services/data/{_options.ApiVersion}/sobjects/Opportunity/{dto.Id}")
            {
                Content = JsonContent.Create(payload)
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            return dto.Id;
        }
    }
}
