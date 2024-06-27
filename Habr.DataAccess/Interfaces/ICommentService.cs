using Habr.DataAccess.Entities;

namespace Habr.DataAccess.Interfaces
{
    public interface ICommentService
    {
        Task CommentOnPostAsync(User user);
        Task ReplyToCommentAsync(User user);
        Task DeleteCommentAsync(User user);
    }
}
