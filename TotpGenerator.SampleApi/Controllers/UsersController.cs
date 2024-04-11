using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TotpGenerator.SampleApi.Contracts;

namespace TotpGenerator.SampleApi.Controllers
{

    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("api/[controller]/2fa/{email}")]
        public async ValueTask<IActionResult> GenerateTotpCode(string email)
        {
            return Ok(await _userService.GetTwoFactorTotpCode(email));
        }
        [HttpPost]
        [Route("api/[controller]/2fa-verfy/{email}/{totpCode}")]
        public async ValueTask<IActionResult> ValidateTotpCode(string email, int totpCode)
        {
            return Ok(await _userService.ValidateTwoFactorTotpCode(email, totpCode));
        }
    }
}
