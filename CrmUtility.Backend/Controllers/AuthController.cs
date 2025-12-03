using Microsoft.AspNetCore.Mvc;
using CrmUtility.Backend.Services;
using System.Threading.Tasks;

namespace CrmUtility.Backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly HubSpotAuthService _hubSpotAuthService;
        private readonly SalesforceAuthService _salesforceAuthService;

        public AuthController(
            HubSpotAuthService hubSpotAuthService,
            SalesforceAuthService salesforceAuthService)
        {
            _hubSpotAuthService = hubSpotAuthService;
            _salesforceAuthService = salesforceAuthService;
        }

        [HttpGet("hubspot/login")]
        public IActionResult HubSpotLogin()
        {
            string userId = "test-user";
            string url = _hubSpotAuthService.GetLoginUrl(userId);
            return Redirect(url);
        }

        [HttpGet("salesforce/login")]
        public IActionResult SalesforceLogin()
        {
            string userId = "test-user";
            var url = _salesforceAuthService.GetLoginUrl(userId);
            return Redirect(url);
        }

        [HttpGet("hubspot/callback")]
        public async Task<IActionResult> HubSpotCallback([FromQuery] string code, [FromQuery] string state)
        {
            string userId = state;
            var tokenData = await _hubSpotAuthService.ExchangeCodeAsync(code, userId);

            return Ok(new
            {
                message = "HubSpot connected successfully!",
                crm = "hubspot",
                accessToken = tokenData.AccessToken,
                refreshToken = tokenData.RefreshToken
            });
        }

        [HttpGet("salesforce/callback")]
        public async Task<IActionResult> SalesforceCallback(
            [FromQuery] string? code,
            [FromQuery] string? state)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                var rawQuery = HttpContext.Request.QueryString.Value;
                return BadRequest(new
                {
                    message = "Salesforce callback received NO code/state.",
                    rawQuery
                });
            }

            string userId = state;
            var connection = await _salesforceAuthService.ExchangeCodeAsync(code, userId);

            return Ok(new
            {
                message = "Salesforce connected successfully!",
                crm = "salesforce",
                userId = userId,
                accessToken = connection.AccessToken,
                refreshToken = connection.RefreshToken,
                instanceUrl = connection.InstanceUrl
            });
        }
    }
}
