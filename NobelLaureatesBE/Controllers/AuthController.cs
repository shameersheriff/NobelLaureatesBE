using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using NobelLaureatesBE.BusinessLogic.Interfaces;
using NobelLaureatesBE.Repositories.Models;

namespace NobelLaureatesBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private byte[] _key;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
            _key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!await _userService.IsUniqueUser(model.Username))
                return BadRequest("Username already exists");

            var user = await _userService.RegisterUser(model.Username, model.Password, model.FirstName, model.LastName);

            if (user == null)
                return BadRequest("User registration failed");

            var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:DurationInMinutes"]));
            var token = _userService.GenerateJwtToken(user, _key, expiry);

            return Ok(new { AccessToken = token, RefreshToken = user.RefreshToken, User = user });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userService.Authenticate(model.Username, model.Password);
            if (user == null)
                return Unauthorized();

            var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:DurationInMinutes"]));
            var token = _userService.GenerateJwtToken(user, _key, expiry);
            var refreshToken = _userService.GenerateRefreshToken();
            await _userService.SaveRefreshToken(user, refreshToken, DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDurationInDays"])));

            return Ok(new { AccessToken = token, RefreshToken = refreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshModel model)
        {
            var user = await _userService.GetUserByRefreshToken(model.RefreshToken);
            if (user == null)
                return Unauthorized();

            var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:DurationInMinutes"]));
            var token = _userService.GenerateJwtToken(user, _key, expiry);
            var refreshToken = _userService.GenerateRefreshToken();
            await _userService.SaveRefreshToken(user, refreshToken, DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDurationInDays"])));

            return Ok(new { AccessToken = token, RefreshToken = refreshToken });
        }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RefreshModel
    {
        public string RefreshToken { get; set; }
    }
}
