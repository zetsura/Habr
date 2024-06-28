using Habr.DataAccess.Entities;

namespace Habr.DataAccess.Interfaces
{
    public interface ICommentService
    {
        Task CommentOnPostAsync(int userId);
        Task ReplyToCommentAsync(int userId);
        Task DeleteCommentAsync(int userId);
    }
}
