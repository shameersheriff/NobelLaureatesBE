using System;
using System.Threading.Tasks;
using NobelLaureatesBE.Repositories.Models;

namespace NobelLaureatesBE.BusinessLogic.Interfaces
{
    public interface IUserService
    {
        Task<bool> IsUniqueUser(string username);
        Task<User> RegisterUser(string username, string password);
        Task<User> Authenticate(string username, string password);
        Task SaveRefreshToken(User user, string refreshToken, DateTime refreshTokenExpiryTime);
        Task<User> GetUserByRefreshToken(string refreshToken);
    }
}
