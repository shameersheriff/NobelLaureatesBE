using NobelLaureatesBE.BusinessLogic.Services;
using NobelLaureatesBE.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using NobelLaureatesBE.BusinessLogic.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace NobelLaureatesBE.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private IUserService _userService;
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var context = new ApplicationDbContext(_dbContextOptions);
            _userService = new UserService(context);
        }

        [Test]
        public async Task IsUniqueUser_ReturnsTrue_WhenUserDoesNotExist()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Act
                var result = await _userService.IsUniqueUser("nonexistentuser");

                // Assert
                Assert.IsTrue(result);
            }
        }

        [Test]
        public async Task IsUniqueUser_ReturnsFalse_WhenUserExists()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Arrange
                var refreshToken = _userService.GenerateRefreshToken();
                context.Users.Add(new User { Username = "existinguser", Password = "Password", FirstName = "FirstName", LastName = "LastName", RefreshToken = refreshToken });
                context.SaveChanges();

                // Act
                var result = await _userService.IsUniqueUser("existinguser");

                // Assert
                Assert.IsFalse(result);
            }
        }

        [Test]
        public async Task RegisterUser_CreatesUserSuccessfully()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {

                // Act
                var user = await _userService.RegisterUser("newuser", "password", "First", "Last");

                // Assert
                var createdUser = await context.Users.SingleOrDefaultAsync(u => u.Username == "newuser");
                Assert.IsNotNull(createdUser);
                Assert.AreEqual("newuser", createdUser.Username);
                Assert.IsTrue(BCrypt.Net.BCrypt.Verify("password", createdUser.Password));
                Assert.AreEqual("First", createdUser.FirstName);
                Assert.AreEqual("Last", createdUser.LastName);
            }
        }

        [Test]
        public async Task Authenticate_ReturnsUser_WhenCredentialsAreValid()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Arrange
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password");
                var refreshToken = _userService.GenerateRefreshToken();

                context.Users.Add(new User { Username = "validuser", Password = hashedPassword, FirstName = "FirstName", LastName = "LastName", RefreshToken = refreshToken });
                context.SaveChanges();

                // Act
                var user = await _userService.Authenticate("validuser", "password");

                Console.WriteLine("username : " + user.Username);

                // Assert
                Assert.IsNotNull(user);
                Assert.AreEqual("validuser", user.Username);
            }
        }

        [Test]
        public async Task Authenticate_ReturnsNull_WhenCredentialsAreInvalid()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Act
                var user = await _userService.Authenticate("invaliduser", "password");

                // Assert
                Assert.IsNull(user);
            }
        }

        [Test]
        public async Task SaveRefreshToken_UpdatesUserRefreshToken()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Arrange
                var refreshToken = _userService.GenerateRefreshToken();
                var user = new User { Username = "user1", Password = "Password", FirstName = "FirstName", LastName = "LastName", RefreshToken = refreshToken };
                context.Users.Add(user);
                context.SaveChanges();
                var newRefreshToken = _userService.GenerateRefreshToken();
                var newExpiryTime = DateTime.UtcNow.AddDays(2);

                // Act
                await _userService.SaveRefreshToken(user, newRefreshToken, newExpiryTime);

                // Assert
                var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "user1");
                Assert.IsNotNull(updatedUser);
                Assert.AreEqual(newRefreshToken, updatedUser.RefreshToken);
                Assert.AreEqual(newExpiryTime, updatedUser.RefreshTokenExpiryTime);
            }
        }

        [Test]
        public async Task GetUserByRefreshToken_ReturnsUser_WhenTokenIsValid()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Arrange
                var validToken = _userService.GenerateRefreshToken();
                var user = new User { Username = "user2", RefreshToken = validToken, Password = "Password", FirstName = "FirstName", LastName = "LastName" };
                context.Users.Add(user);
                context.SaveChanges();

                // Act
                var result = await _userService.GetUserByRefreshToken(validToken);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("user2", result.Username);
            }
        }

        [Test]
        public async Task GetUserByRefreshToken_ReturnsNull_WhenTokenIsInvalid()
        {
            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                // Act
                var result = await _userService.GetUserByRefreshToken("invalidToken");

                // Assert
                Assert.IsNull(result);
            }
        }

        [Test]
        public void GenerateJwtToken_ReturnsValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "user3",
                FirstName = "FirstName",
                LastName = "LastName"
            };
            var key = Encoding.ASCII.GetBytes("12341234512345612312341212341234561212345");
            var expiry = DateTime.UtcNow.AddHours(1);

            // Act
            var token = _userService.GenerateJwtToken(user, key, expiry);

            // Assert
            Assert.IsNotNull(token);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            Assert.IsNotNull(principal);
            Assert.IsNotNull(validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;
            Assert.IsNotNull(jwtToken);

            Assert.AreEqual(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == "Id").Value);
            Assert.AreEqual(user.Username, jwtToken.Claims.First(c => c.Type == "Email").Value);
            Assert.AreEqual(user.FirstName, jwtToken.Claims.First(c => c.Type == "FirstName").Value);
            Assert.AreEqual(user.LastName, jwtToken.Claims.First(c => c.Type == "LastName").Value);
        }
    }
}
