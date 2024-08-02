using Microsoft.AspNetCore.Mvc;
using System.Text;
using NobelLaureatesBE.BusinessLogic.Interfaces;

namespace NobelLaureatesBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly byte[] _key;
        private readonly int _tokenDurationInMinutes;
        private readonly int _refreshTokenDurationInDays;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            _tokenDurationInMinutes = int.Parse(_configuration["Jwt:DurationInMinutes"] ?? throw new InvalidOperationException("Jwt:DurationInMinutes is not configured."));
            _refreshTokenDurationInDays = int.Parse(_configuration["Jwt:RefreshTokenDurationInDays"] ?? throw new InvalidOperationException("Jwt:RefreshTokenDurationInDays is not configured."));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null)
                return BadRequest("Invalid client request");

            if (!await _userService.IsUniqueUser(model.Username))
                return BadRequest("Username already exists");

            var user = await _userService.RegisterUser(model.Username, model.Password, model.FirstName, model.LastName);

            if (user == null)
                return BadRequest("User registration failed");

            var expiry = DateTime.UtcNow.AddMinutes(_tokenDurationInMinutes);
            var token = _userService.GenerateJwtToken(user, _key, expiry);

            return Ok(new { AccessToken = token, RefreshToken = user.RefreshToken, User = user });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model == null)
                return BadRequest("Invalid client request");

            var user = await _userService.Authenticate(model.Username, model.Password);
            if (user == null)
                return Unauthorized("Invalid username or password");

            var expiry = DateTime.UtcNow.AddMinutes(_tokenDurationInMinutes);
            var token = _userService.GenerateJwtToken(user, _key, expiry);
            var refreshToken = _userService.GenerateRefreshToken();
            await _userService.SaveRefreshToken(user, refreshToken, DateTime.UtcNow.AddDays(_refreshTokenDurationInDays));

            return Ok(new { AccessToken = token, RefreshToken = refreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshModel model)
        {
            if (model == null)
                return BadRequest("Invalid client request");

            var user = await _userService.GetUserByRefreshToken(model.RefreshToken);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            var expiry = DateTime.UtcNow.AddMinutes(_tokenDurationInMinutes);
            var token = _userService.GenerateJwtToken(user, _key, expiry);
            var refreshToken = _userService.GenerateRefreshToken();
            await _userService.SaveRefreshToken(user, refreshToken, DateTime.UtcNow.AddDays(_refreshTokenDurationInDays));

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
