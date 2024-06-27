using Habr.DataAccess.Entities;

namespace Habr.DataAccess.Interfaces
{
    public interface IPostService
    {
        Task ViewAllPostsAsync();
        Task ViewAllDraftsAsync(User user);
        Task CreatePostAsync(User user, bool isPublished);
        Task EditPostAsync(User user);
        Task DeletePostAsync(User user);
        Task PublishPostAsync(User user);
        Task MoveToDraftsAsync(User user);
    }
}
