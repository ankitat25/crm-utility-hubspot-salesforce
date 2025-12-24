using CrmUtility.Backend.Models;
using CrmUtility.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CrmUtility.Backend.Controllers
{
    [ApiController]
    [Route("salesforce")]
    public class SalesforceController : ControllerBase
    {
        private readonly SalesforceCrmService _crm;

        public SalesforceController(SalesforceCrmService crm)
        {
            _crm = crm;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts([FromQuery] string userId = "test-user")
        {
            var list = await _crm.GetContactsAsync(userId);
            return Ok(list);
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies([FromQuery] string userId = "test-user")
        {
            var list = await _crm.GetCompaniesAsync(userId);
            return Ok(list);
        }

        [HttpGet("deals")]
        public async Task<IActionResult> GetDeals([FromQuery] string userId = "test-user")
        {
            var list = await _crm.GetDealsAsync(userId);
            return Ok(list);
        }

        [HttpPost("deal")]
        public async Task<IActionResult> CreateDeal(
            [FromBody] StandardDealDto deal,
            [FromQuery] string userId = "test-user")
        {
            deal.Crm = "salesforce";

            var id = await _crm.CreateDealAsync(deal, userId);

            return Ok(new
            {
                message = "Salesforce deal created",
                id
            });
        }

        [HttpPut("deal")]
        public async Task<IActionResult> UpdateDeal(
            [FromBody] StandardDealDto deal,
            [FromQuery] string userId = "test-user")
        {
            deal.Crm = "salesforce";

            var id = await _crm.UpdateDealAsync(deal, userId);

            return Ok(new
            {
                message = "Salesforce deal updated",
                id
            });
        }
    }
}
