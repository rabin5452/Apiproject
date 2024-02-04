using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Practiseproject.DTO;
using Practiseproject.Model;
using Practiseproject.Services.Interface;

namespace Practiseproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        public AuthController(IAuthService authService,UserManager<User> userManager)
        {
            _userManager = userManager;
            _authService = authService;
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var result=await _authService.Login(loginModel);
            return Ok(result);
        }
        [Authorize(Roles ="Admin")]
        [HttpPost("add-Admin")]
        public async Task<IActionResult> AddAdmin([FromBody] AddUser addUser)
        {
            addUser=await _authService.AddAdmin(addUser);
            return Ok(addUser);
        }
        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser([FromBody] AddUser addUser)
        {
            addUser=await _authService.Add(addUser);
            return Ok(addUser);
        }

        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Invalid user name");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return Ok("LoggedOut");
        }
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
           tokenModel=await _authService.RefreshToken(tokenModel);
            if (tokenModel == null) return BadRequest("Token Invalid");
            return Ok(tokenModel);
        }

    }
}
