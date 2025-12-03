using CrmUtility.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmUtility.Backend.Controllers
{
    [ApiController]
    [Route("hubspot")]
    public class HubSpotContactsController : ControllerBase
    {
        private readonly HubSpotCrmService _crmService;

        public HubSpotContactsController(HubSpotCrmService crmService)
        {
            _crmService = crmService;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            string userId = "test-user";
            var contacts = await _crmService.GetContactsAsync(userId);
            return Ok(contacts);
        }
    }
}
