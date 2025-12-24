using CrmUtility.Backend.Models;
using CrmUtility.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CrmUtility.Backend.Controllers
{
    [ApiController]
    [Route("hubspot")]
    public class HubSpotController : ControllerBase
    {
        private readonly HubSpotCrmService _crm;

        public HubSpotController(HubSpotCrmService crm)
        {
            _crm = crm;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts(
            [FromQuery] string userId = "test-user")
        {
            var result = await _crm.GetContactsAsync(userId);
            return Ok(result);
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies(
            [FromQuery] string userId = "test-user")
        {
            var result = await _crm.GetCompaniesAsync(userId);
            return Ok(result);
        }

        [HttpGet("deals")]
        public async Task<IActionResult> GetDeals(
            [FromQuery] string userId = "test-user")
        {
            var result = await _crm.GetDealsAsync(userId);
            return Ok(result);
        }

        [HttpPost("contacts/create")]
        public async Task<IActionResult> CreateContact(
            [FromBody] StandardContactDto contact,
            [FromQuery] string userId = "test-user")
        {
            var id = await _crm.CreateContactAsync(contact, userId);
            return Ok(new { id });
        }

        [HttpPost("companies/create")]
        public async Task<IActionResult> CreateCompany(
            [FromBody] StandardCompanyDto company,
            [FromQuery] string userId = "test-user")
        {
            var id = await _crm.CreateCompanyAsync(company, userId);
            return Ok(new { id });
        }

        [HttpPost("deals/create")]
        public async Task<IActionResult> CreateDeal(
            [FromBody] StandardDealDto deal,
            [FromQuery] string userId = "test-user")
        {
            var id = await _crm.CreateDealAsync(deal, userId);
            return Ok(new { id });
        }

        [HttpPut("contacts/update")]
        public async Task<IActionResult> UpdateContact(
            [FromBody] StandardContactDto contact,
            [FromQuery] string userId = "test-user")
        {
            var id = await _crm.UpdateContactAsync(contact, userId);
            return Ok(new { id });
        }

        [HttpPut("companies/update")]
        public async Task<IActionResult> UpdateCompany(
            [FromBody] StandardCompanyDto company,
            [FromQuery] string userId = "test-user")
        {
            var id = await _crm.UpdateCompanyAsync(company, userId);
            return Ok(new { id });
        }

        [HttpPut("deals/update")]
        public async Task<IActionResult> UpdateDeal(
            [FromBody] StandardDealDto deal,
            [FromQuery] string userId = "test-user")
        {
            var id = await _crm.UpdateDealAsync(deal, userId);
            return Ok(new { id });
        }
    }
}
