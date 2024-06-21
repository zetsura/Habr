using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Habr.Services
{
    public class CommentService
    {
        private readonly DataContext _context;

        public CommentService(DataContext context)
        {
            _context = context;
        }

        public void CommentOnPost(User user)
        {
            Console.Write("Enter the ID of the post you want to comment on: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = _context.Posts.SingleOrDefault(p => p.Id == postId);
            if (post == null)
            {
                Console.WriteLine("Post not found.");
                return;
            }

            Console.Write("Enter your comment: ");
            var content = Console.ReadLine();
            if (string.IsNullOrEmpty(content) || content.Length > 300)
            {
                Console.WriteLine("Content is required and must be less than 300 characters.");
                return;
            }

            var comment = new Comment { Content = content, Created = DateTime.Now, UserId = user.Id, PostId = post.Id };
            _context.Comments.Add(comment);
            _context.SaveChanges();
            Console.WriteLine("Comment added successfully.");
        }

        public void ReplyToComment(User user)
        {
            Console.Write("Enter the ID of the comment you want to reply to: ");
            if (!int.TryParse(Console.ReadLine(), out var commentId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var comment = _context.Comments.SingleOrDefault(c => c.Id == commentId);
            if (comment == null)
            {
                Console.WriteLine("Comment not found.");
                return;
            }

            Console.Write("Enter your reply: ");
            var content = Console.ReadLine();
            if (string.IsNullOrEmpty(content) || content.Length > 300)
            {
                Console.WriteLine("Content is required and must be less than 300 characters.");
                return;
            }

            var reply = new Comment { Content = content, Created = DateTime.Now, UserId = user.Id, PostId = comment.PostId, ParentCommentId = comment.Id };
            _context.Comments.Add(reply);
            _context.SaveChanges();
            Console.WriteLine("Reply added successfully.");
        }

        public void DeleteComment(User user)
        {
            Console.Write("Enter the ID of the comment you want to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var commentId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var comment = _context.Comments.Include(c => c.Replies).SingleOrDefault(c => c.Id == commentId && c.UserId == user.Id);
            if (comment == null)
            {
                Console.WriteLine("Comment not found or you don't have permission to delete it.");
                return;
            }

            _context.Comments.Remove(comment);
            _context.SaveChanges();
            Console.WriteLine("Comment deleted successfully.");
        }
    }
}
