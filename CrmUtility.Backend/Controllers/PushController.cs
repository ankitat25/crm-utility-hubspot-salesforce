using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CrmUtility.Backend.Models;
using CrmUtility.Backend.Services;

namespace CrmUtility.Backend.Controllers
{
    [ApiController]
    [Route("push")]
    public class PushController : ControllerBase
    {
        private readonly SalesforceCrmService _salesforceCrmService;
        private readonly HubSpotCrmService _hubSpotCrmService;

        public PushController(
            SalesforceCrmService salesforceCrmService,
            HubSpotCrmService hubSpotCrmService)
        {
            _salesforceCrmService = salesforceCrmService;
            _hubSpotCrmService = hubSpotCrmService;
        }

        [HttpPost("contact")]
        public async Task<IActionResult> PushContact(
            [FromBody] StandardContactDto contact,
            [FromQuery] string userId = "test-user")
        {
            if (contact == null)
                return BadRequest("Contact body is required.");

            if (string.IsNullOrWhiteSpace(contact.Crm))
                return BadRequest("crm field is required (hubspot or salesforce).");

            if (string.IsNullOrWhiteSpace(contact.LastName))
                return BadRequest("LastName is required for contact creation.");

            string crm = contact.Crm.ToLower().Trim();
            string newId;

            switch (crm)
            {
                case "salesforce":
                    newId = await _salesforceCrmService.CreateContactAsync(contact, userId);
                    return Ok(new
                    {
                        message = "Contact pushed to Salesforce.",
                        crm = "salesforce",
                        id = newId
                    });

                case "hubspot":
                    newId = await _hubSpotCrmService.CreateContactAsync(contact, userId);
                    return Ok(new
                    {
                        message = "Contact pushed to HubSpot.",
                        crm = "hubspot",
                        id = newId
                    });

                default:
                    return BadRequest("Unsupported CRM. Use 'hubspot' or 'salesforce'.");
            }
        }
    }
}
