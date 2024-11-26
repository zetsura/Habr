using Habr.DataAccess.Entities;

namespace Habr.DataAccess.Interfaces
{
    public interface IPostService
    {
        Task ViewPublishedPostsAsync();
        Task ViewDraftsByUserAsync(int userId);
        Task CreatePostAsync(int userId, bool isPublished);
        Task EditPostAsync(int userId);
        Task DeletePostAsync(int userId);
        Task PublishPostAsync(int userId);
        Task MoveToDraftsAsync(int userId);
    }
}
