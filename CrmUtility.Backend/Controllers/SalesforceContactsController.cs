using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CrmUtility.Backend.Services;
using CrmUtility.Backend.Models;

namespace CrmUtility.Backend.Controllers
{
    [ApiController]
    [Route("salesforce")]
    public class SalesforceContactsController : ControllerBase
    {
        private readonly SalesforceCrmService _salesforceCrmService;

        public SalesforceContactsController(SalesforceCrmService salesforceCrmService)
        {
            _salesforceCrmService = salesforceCrmService;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts([FromQuery] string userId = "test-user")
        {
            var contacts = await _salesforceCrmService.GetContactsAsync(userId);
            return Ok(contacts);
        }

        [HttpPost("contact")]
        public async Task<IActionResult> CreateContact(
            [FromBody] StandardContactDto contact,
            [FromQuery] string userId = "test-user")
        {
            if (contact == null)
                return BadRequest("Contact payload is required.");

            if (string.IsNullOrWhiteSpace(contact.LastName))
                return BadRequest("LastName is required for Salesforce Contact.");

            var newId = await _salesforceCrmService.CreateContactAsync(contact, userId);

            return Ok(new
            {
                message = "Salesforce contact created successfully!",
                crm = "salesforce",
                id = newId
            });
        }
    }
}
