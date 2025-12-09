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
    // ===========================
    // 🔹 SALESFORCE MODELS
    // ===========================

    public class SalesforceAccountRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public SalesforceUser Owner { get; set; }
    }

    public class SalesforceQueryResult<T>
    {
        public List<T> records { get; set; }
        public int totalSize { get; set; }
        public bool done { get; set; }
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

    public class SalesforceAccount
    {
        public string Name { get; set; }
    }

    public class SalesforceUser
    {
        public string Name { get; set; }
    }

    // ===========================
    // 🔹 SALESFORCE CRM SERVICE
    // ===========================

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

        // ===========================
        // ✅ GET CONTACTS
        // ===========================

        public async Task<List<StandardContactDto>> GetContactsAsync(string userId)
        {
            var connection = await _tokenService.GetConnectionAsync(userId, CrmType.Salesforce);
            if (connection == null)
                throw new InvalidOperationException("No Salesforce connection found for user " + userId);

            string soql =
                "SELECT Id, FirstName, LastName, Email, Account.Name, Owner.Name " +
                "FROM Contact LIMIT 10";

            var requestUrl =
                $"{connection.InstanceUrl}/services/data/{_options.ApiVersion}/query" +
                $"?q={Uri.EscapeDataString(soql)}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", connection.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var data =
                await response.Content.ReadFromJsonAsync<SalesforceQueryResult<SalesforceContactRecord>>();

            var result = new List<StandardContactDto>();

            if (data?.records != null)
            {
                foreach (var r in data.records)
                {
                    result.Add(new StandardContactDto
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
            }

            return result;
        }

        // ===========================
        // ✅ CREATE CONTACT
        // ===========================

        public async Task<string> CreateContactAsync(StandardContactDto contact, string userId)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            var connection = await _tokenService.GetConnectionAsync(userId, CrmType.Salesforce);
            if (connection == null)
                throw new InvalidOperationException("No Salesforce connection found for user " + userId);

            var url =
                $"{connection.InstanceUrl}/services/data/{_options.ApiVersion}/sobjects/Contact";

            var payload = new Dictionary<string, object?>
            {
                ["FirstName"] = contact.FirstName,
                ["LastName"] = contact.LastName,
                ["Email"] = contact.Email
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", connection.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            return result != null && result.TryGetValue("id", out var idObj)
                ? idObj?.ToString()
                : null;
        }

        // ===========================
        // ✅ GET COMPANIES (ACCOUNTS)
        // ===========================

        public async Task<List<StandardCompanyDto>> GetCompaniesAsync(string userId)
        {
            var connection = await _tokenService.GetConnectionAsync(userId, CrmType.Salesforce);
            if (connection == null)
                throw new InvalidOperationException("No Salesforce connection found for user " + userId);

            string soql =
                "SELECT Id, Name, Website, Owner.Name FROM Account LIMIT 10";

            var requestUrl =
                $"{connection.InstanceUrl}/services/data/{_options.ApiVersion}/query" +
                $"?q={Uri.EscapeDataString(soql)}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", connection.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var data =
                await response.Content.ReadFromJsonAsync<SalesforceQueryResult<SalesforceAccountRecord>>();

            var result = new List<StandardCompanyDto>();

            if (data?.records != null)
            {
                foreach (var r in data.records)
                {
                    result.Add(new StandardCompanyDto
                    {
                        Crm = "salesforce",
                        Type = "company",
                        Id = r.Id,
                        Name = r.Name,
                        Domain = r.Website,
                        Owner = r.Owner?.Name
                    });
                }
            }

            return result;
        }

        // ===========================
        // ✅ UPDATE CONTACT
        // ===========================

        public async Task<string> UpdateContactAsync(StandardContactDto contact, string userId)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            if (string.IsNullOrWhiteSpace(contact.Id))
                throw new InvalidOperationException("Contact Id is required.");

            var connection = await _tokenService.GetConnectionAsync(userId, CrmType.Salesforce);
            if (connection == null)
                throw new InvalidOperationException("No Salesforce connection found for user " + userId);

            var url =
                $"{connection.InstanceUrl}/services/data/{_options.ApiVersion}/sobjects/Contact/{contact.Id}";

            var payload = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(contact.FirstName))
                payload["FirstName"] = contact.FirstName;

            if (!string.IsNullOrWhiteSpace(contact.LastName))
                payload["LastName"] = contact.LastName;

            if (!string.IsNullOrWhiteSpace(contact.Email))
                payload["Email"] = contact.Email;

            if (payload.Count == 0)
                throw new InvalidOperationException("No fields to update.");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", connection.AccessToken);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return contact.Id;
        }
    }
}
