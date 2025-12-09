using System.Threading.Tasks;
using CrmUtility.Backend.Models;
using CrmUtility.Backend.Services;
using Microsoft.AspNetCore.Mvc;

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

       
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies([FromQuery] string userId = "test-user")
        {
            var result = await _crm.GetCompaniesAsync(userId);
            return Ok(result);
        }

       
        [HttpPut("contacts/{id}")]
        public async Task<IActionResult> UpdateContact(
            string id,
            [FromBody] StandardContactDto contact,
            [FromQuery] string userId = "test-user")
        {
            
            contact.Id = id;

            var updatedId = await _crm.UpdateContactAsync(contact, userId);

            return Ok(new
            {
                message = "Salesforce contact updated successfully",
                contactId = updatedId
            });
        }
    }
}
