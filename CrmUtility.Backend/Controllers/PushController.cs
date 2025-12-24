using CrmUtility.Backend.Models;
using CrmUtility.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CrmUtility.Backend.Controllers
{
    [ApiController]
    [Route("push")]
    public class PushController : ControllerBase
    {
        private readonly HubSpotCrmService _hubspot;
        private readonly SalesforceCrmService _salesforce;

        public PushController(
            HubSpotCrmService hubspot,
            SalesforceCrmService salesforce)
        {
            _hubspot = hubspot;
            _salesforce = salesforce;
        }
        [HttpPost("contact")]
        public async Task<IActionResult> PushContact(
            [FromBody] StandardContactDto contact,
            [FromQuery] string userId = "test-user")
        {
            if (contact == null)
                return BadRequest("Contact body required.");

            if (string.IsNullOrWhiteSpace(contact.Crm))
                return BadRequest("crm field required");

            var crm = contact.Crm.ToLower();

            if (crm == "hubspot")
            {
                var id = await _hubspot.CreateContactAsync(contact, userId);
                return Ok(new { crm, message = "Contact created", contactId = id });
            }

            if (crm == "salesforce")
            {
                var id = await _salesforce.CreateContactAsync(contact, userId);
                return Ok(new { crm, message = "Contact created", contactId = id });
            }

            return BadRequest("Invalid crm");
        }

        [HttpPut("contact")]
        public async Task<IActionResult> UpdateContact(
            [FromBody] StandardContactDto contact,
            [FromQuery] string userId = "test-user")
        {
            if (contact == null)
                return BadRequest("Contact body required.");

            if (string.IsNullOrWhiteSpace(contact.Crm))
                return BadRequest("crm required");

            if (string.IsNullOrWhiteSpace(contact.Id))
                return BadRequest("Id required for update");

            var crm = contact.Crm.ToLower();

            if (crm == "hubspot")
            {
                var id = await _hubspot.UpdateContactAsync(contact, userId);
                return Ok(new { crm, message = "Contact updated", contactId = id });
            }

            if (crm == "salesforce")
            {
                var id = await _salesforce.UpdateContactAsync(contact, userId);
                return Ok(new { crm, message = "Contact updated", contactId = id });
            }

            return BadRequest("Invalid crm");
        }

        [HttpPost("company")]
        public async Task<IActionResult> PushCompany(
            [FromBody] StandardCompanyDto company,
            [FromQuery] string userId = "test-user")
        {
            if (company == null)
                return BadRequest("Company body required.");

            if (string.IsNullOrWhiteSpace(company.Crm))
                return BadRequest("crm required");

            var crm = company.Crm.ToLower();

            if (crm == "hubspot")
            {
                var id = await _hubspot.CreateCompanyAsync(company, userId);
                return Ok(new { crm, message = "Company created", companyId = id });
            }

            if (crm == "salesforce")
            {
                var id = await _salesforce.CreateCompanyAsync(company, userId);
                return Ok(new { crm, message = "Company created", companyId = id });
            }

            return BadRequest("Invalid crm");
        }

        [HttpPut("company")]
        public async Task<IActionResult> UpdateCompany(
            [FromBody] StandardCompanyDto company,
            [FromQuery] string userId = "test-user")
        {
            if (company == null)
                return BadRequest("Company body required.");

            if (string.IsNullOrWhiteSpace(company.Crm))
                return BadRequest("crm required");

            if (string.IsNullOrWhiteSpace(company.Id))
                return BadRequest("Id required for update");

            var crm = company.Crm.ToLower();

            if (crm == "hubspot")
            {
                var id = await _hubspot.UpdateCompanyAsync(company, userId);
                return Ok(new { crm, message = "Company updated", companyId = id });
            }

            if (crm == "salesforce")
            {
                var id = await _salesforce.UpdateCompanyAsync(company, userId);
                return Ok(new { crm, message = "Company updated", companyId = id });
            }

            return BadRequest("Invalid crm");
        }

        [HttpPost("deal")]
        public async Task<IActionResult> PushDeal(
            [FromBody] StandardDealDto deal,
            [FromQuery] string userId = "test-user")
        {
            if (deal == null)
                return BadRequest("Deal body required.");

            if (string.IsNullOrWhiteSpace(deal.Crm))
                return BadRequest("crm required");

            var crm = deal.Crm.ToLower();

            if (crm == "hubspot")
            {
                var id = await _hubspot.CreateDealAsync(deal, userId);
                return Ok(new { crm, message = "Deal created", dealId = id });
            }

            if (crm == "salesforce")
            {
                var id = await _salesforce.CreateDealAsync(deal, userId);
                return Ok(new { crm, message = "Deal created", dealId = id });
            }

            return BadRequest("Invalid crm");
        }

        
        [HttpPut("deal")]
        public async Task<IActionResult> UpdateDeal(
            [FromBody] StandardDealDto deal,
            [FromQuery] string userId = "test-user")
        {
            if (deal == null)
                return BadRequest("Deal body required.");

            if (string.IsNullOrWhiteSpace(deal.Crm))
                return BadRequest("crm required");

            if (string.IsNullOrWhiteSpace(deal.Id))
                return BadRequest("Id required for update");

            var crm = deal.Crm.ToLower();

            if (crm == "hubspot")
            {
                var id = await _hubspot.UpdateDealAsync(deal, userId);
                return Ok(new { crm, message = "Deal updated", dealId = id });
            }

            if (crm == "salesforce")
            {
                var id = await _salesforce.UpdateDealAsync(deal, userId);
                return Ok(new { crm, message = "Deal updated", dealId = id });
            }

            return BadRequest("Invalid crm");
        }
    }
}
