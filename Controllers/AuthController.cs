using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practiseproject.DTO;
using Practiseproject.Services.Interface;

namespace Practiseproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            var result=_authService.Login(loginModel);
            return Ok(result);
        }
        [HttpPost("adduser")]
        public IActionResult AddUser([FromBody] AddUser addUser)
        {
            addUser=_authService.Add(addUser);
            return Ok(addUser);
        }
    }
}
