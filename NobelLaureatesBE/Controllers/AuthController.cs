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

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!await _userService.IsUniqueUser(model.Username))
                return BadRequest("Username already exists");

            var user = await _userService.RegisterUser(model.Username, model.Password, model.FirstName, model.LastName);

            if (user == null)
                return BadRequest("User registration failed");


            var token = GenerateJwtToken(user);

            return Ok(new { AccessToken = token, RefreshToken = user.RefreshToken, User = user });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userService.Authenticate(model.Username, model.Password);
            if (user == null)
                return Unauthorized();

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            await _userService.SaveRefreshToken(user, refreshToken, DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDurationInDays"])));

            return Ok(new { AccessToken = token, RefreshToken = refreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshModel model)
        {
            var user = await _userService.GetUserByRefreshToken(model.RefreshToken);
            if (user == null)
                return Unauthorized();

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            await _userService.SaveRefreshToken(user, refreshToken, DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDurationInDays"])));

            return Ok(new { AccessToken = token, RefreshToken = refreshToken });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim("Email", user.Username.ToString()),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:DurationInMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
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
