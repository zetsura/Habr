using Habr.DataAccess;
using Habr.DataAccess.ApplicationConstants;
using Habr.DataAccess.Entities;
using Habr.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habr.Services
{
    public class CommentService : ICommentService
    {
        private readonly DataContext _context;

        public CommentService(DataContext context)
        {
            _context = context;
        }

        public async Task CommentOnPostAsync(int userId)
        {
            Console.Write("Enter the ID of the post you want to comment on: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == postId && p.IsPublished);

            if (post == null)
            {
                Console.WriteLine("Post not found or is not published.");
                return;
            }

            Console.Write("Enter your comment: ");
            var content = Console.ReadLine();
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("Content is required.");
                return;
            }

            if (content.Length > Constants.MaxCommentLength)
            {
                Console.WriteLine($"Content must be less than {Constants.MaxCommentLength} characters.");
                return;
            }

            var comment = new Comment
            {
                Content = content,
                Created = DateTime.UtcNow,
                UserId = userId,
                PostId = post.Id
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            Console.WriteLine("Comment added successfully.");
        }

        public async Task ReplyToCommentAsync(int userId)
        {
            Console.Write("Enter the ID of the comment you want to reply to: ");
            if (!int.TryParse(Console.ReadLine(), out var commentId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var comment = await _context.Comments
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                Console.WriteLine("Comment not found.");
                return;
            }

            Console.Write("Enter your reply: ");
            var content = Console.ReadLine();
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("Content is required.");
                return;
            }

            if (content.Length > Constants.MaxCommentLength)
            {
                Console.WriteLine($"Content must be less than {Constants.MaxCommentLength} characters.");
                return;
            }

            var reply = new Comment
            {
                Content = content,
                Created = DateTime.UtcNow,
                UserId = userId,
                PostId = comment.PostId,
                ParentCommentId = comment.Id
            };
            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();
            Console.WriteLine("Reply added successfully.");
        }

        public async Task DeleteCommentAsync(int userId)
        {
            Console.Write("Enter the ID of the comment you want to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var commentId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var comment = await _context.Comments
                .Include(c => c.Replies)
                .SingleOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

            if (comment == null)
            {
                Console.WriteLine("Comment not found or you don't have permission to delete it.");
                return;
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            Console.WriteLine("Comment deleted successfully.");
        }
    }
}
