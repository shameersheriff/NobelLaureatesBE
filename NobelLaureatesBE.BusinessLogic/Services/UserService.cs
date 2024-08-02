using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NobelLaureatesBE.BusinessLogic.Interfaces;
using NobelLaureatesBE.Repositories.Models;

namespace NobelLaureatesBE.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> IsUniqueUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User> RegisterUser(string username, string password, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be null or whitespace.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be null or whitespace.", nameof(lastName));

            var user = new User
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(2)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }
            return user;
        }

        public async Task SaveRefreshToken(User user, string refreshToken, DateTime refreshTokenExpiryTime)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentException("Refresh token cannot be null or whitespace.", nameof(refreshToken));

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentException("Refresh token cannot be null or whitespace.", nameof(refreshToken));
            return await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public string GenerateJwtToken(User user, byte[] key, DateTime expiry)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (key == null || key.Length == 0) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim("Email", user.Username),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
