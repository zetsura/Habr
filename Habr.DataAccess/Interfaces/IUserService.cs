using Habr.DataAccess.Entities;

namespace Habr.DataAccess.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync();
        Task<User> LoginAsync();
        Task ConfirmEmailAsync(string email);
    }
}
