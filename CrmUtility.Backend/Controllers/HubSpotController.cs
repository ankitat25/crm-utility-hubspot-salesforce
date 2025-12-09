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

      
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies([FromQuery] string userId = "test-user")
        {
            var result = await _crm.GetCompaniesAsync(userId);
            return Ok(result);
        }

       
        [HttpGet("deals")]
        public async Task<IActionResult> GetDeals([FromQuery] string userId = "test-user")
        {
            var result = await _crm.GetDealsAsync(userId);
            return Ok(result);
        }

      
        [HttpPost("companies/create")]
        public async Task<IActionResult> CreateCompany([FromBody] StandardCompanyDto company, [FromQuery] string userId = "test-user")
        {
            var id = await _crm.CreateCompanyAsync(company, userId);
            return Ok(new { id });
        }

    
        [HttpPost("deals/create")]
        public async Task<IActionResult> CreateDeal([FromBody] StandardDealDto deal, [FromQuery] string userId = "test-user")
        {
            var id = await _crm.CreateDealAsync(deal, userId);
            return Ok(new { id });
        }
    }
}
