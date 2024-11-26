using Habr.DataAccess;
using Habr.DataAccess.ApplicationConstants;
using Habr.DataAccess.Entities;
using Habr.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habr.Services
{
    public class PostService : IPostService
    {
        private readonly DataContext _context;

        public PostService(DataContext context)
        {
            _context = context;
        }

        public async Task ViewPublishedPostsAsync()
        {
            var posts = await _context.Posts.Include(p => p.User)
                                            .Include(p => p.Comments)
                                            .ThenInclude(c => c.User)
                                            .Where(p => p.IsPublished && !p.IsDeleted)
                                            .OrderByDescending(p => p.PublishedDate)
                                            .AsNoTracking()
                                            .ToListAsync();

            if (posts == null || posts.Count == 0)
            {
                Console.WriteLine("No published posts found.");
                return;
            }

            Console.WriteLine("\n====================");
            Console.WriteLine("Published Posts:");
            Console.WriteLine("====================");

            foreach (var post in posts)
            {
                Console.WriteLine($"\nPost ID: {post.Id}");
                Console.WriteLine($"Title: {post.Title}");
                Console.WriteLine($"Author: {post.User?.Email}");
                Console.WriteLine($"Published: {post.PublishedDate}");

                if (post.Comments != null && post.Comments.Count > 0)
                {
                    Console.WriteLine("\nComments:");
                    foreach (var comment in post.Comments)
                    {
                        Console.WriteLine($"\tComment ID: {comment.Id}");
                        Console.WriteLine($"\tComment by: {comment.User?.Email}");
                        Console.WriteLine($"\tCreated: {comment.Created}");
                        Console.WriteLine($"\tContent: {comment.Content}\n");
                    }
                }
                Console.WriteLine("--------------------");
            }
        }

        public async Task ViewDraftsByUserAsync(int userId)
        {
            var drafts = await _context.Posts.Include(p => p.User)
                                             .Where(p => !p.IsPublished && p.UserId == userId && !p.IsDeleted)
                                             .OrderByDescending(p => p.UpdatedAt)
                                             .AsNoTracking()
                                             .ToListAsync();

            if (drafts == null || drafts.Count == 0)
            {
                Console.WriteLine("No drafts found.");
                return;
            }

            Console.WriteLine("\n====================");
            Console.WriteLine("Draft Posts:");
            Console.WriteLine("====================");

            foreach (var draft in drafts)
            {
                Console.WriteLine($"\nDraft ID: {draft.Id}");
                Console.WriteLine($"Title: {draft.Title}");
                Console.WriteLine($"Created: {draft.CreatedAt}");
                Console.WriteLine($"Updated: {draft.UpdatedAt}");
                Console.WriteLine("--------------------");
            }
        }

        public async Task CreatePostAsync(int userId, bool isPublished)
        {
            Console.Write("Enter the title of your post: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine("The title is required.");
                return;
            }

            if (title.Length > Constants.MaxTitleLength)
            {
                Console.WriteLine($"The title must be less than {Constants.MaxTitleLength} symbols.");
                return;
            }

            Console.Write("Enter the text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("The text is required.");
                return;
            }

            if (text.Length > Constants.MaxTextLength)
            {
                Console.WriteLine($"The text must be less than {Constants.MaxTextLength} symbols.");
                return;
            }

            var post = new Post
            {
                Title = title,
                Text = text,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedDate = isPublished ? (DateTime?)DateTime.UtcNow : null,
                UserId = userId,
                IsPublished = isPublished
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            Console.WriteLine(isPublished ? "Post created and published successfully." : "Post created as draft successfully.");
        }

        public async Task EditPostAsync(int userId)
        {
            Console.Write("Enter the ID of the post you want to edit: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts
                .SingleOrDefaultAsync(p => p.Id == postId && p.UserId == userId);

            if (post == null)
            {
                Console.WriteLine("The post does not exist.");
                return;
            }

            if (post.IsDeleted)
            {
                Console.WriteLine("The post has been deleted and cannot be edited.");
                return;
            }

            if (post.IsPublished)
            {
                Console.WriteLine("A published post cannot be edited. It must be moved to drafts first.");
                return;
            }

            Console.Write("Enter the new title of your post: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine("The title is required.");
                return;
            }

            if (title.Length > Constants.MaxTitleLength)
            {
                Console.WriteLine($"The title must be less than {Constants.MaxTitleLength} symbols.");
                return;
            }

            Console.Write("Enter the new text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("The text is required.");
                return;
            }

            if (text.Length > Constants.MaxTextLength)
            {
                Console.WriteLine($"The text must be less than {Constants.MaxTextLength} symbols.");
                return;
            }

            post.Title = title;
            post.Text = text;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post updated successfully.");
        }

        public async Task DeletePostAsync(int userId)
        {
            Console.Write("Enter the ID of the post you want to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts
                .SingleOrDefaultAsync(p => p.Id == postId && p.UserId == userId);

            if (post == null || post.IsDeleted)
            {
                Console.WriteLine("The post does not exist.");
                return;
            }

            post.IsDeleted = true;
            post.PublishedDate = null;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post marked as deleted successfully.");
        }

        public async Task PublishPostAsync(int userId)
        {
            Console.Write("Enter the ID of the post you want to publish: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts
                .SingleOrDefaultAsync(p => p.Id == postId && p.UserId == userId && !p.IsPublished);

            if (post == null)
            {
                Console.WriteLine("Draft not found or you don't have permission to publish it.");
                return;
            }

            post.IsPublished = true;
            post.PublishedDate = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post published successfully.");
        }

        public async Task MoveToDraftsAsync(int userId)
        {
            Console.Write("Enter the ID of the post you want to move to drafts: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts
                .Include(p => p.Comments)
                .SingleOrDefaultAsync(p => p.Id == postId && p.UserId == userId);

            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to move it to drafts.");
                return;
            }

            if (!post.IsPublished)
            {
                Console.WriteLine("The post is already in drafts.");
                return;
            }

            if (post.Comments != null && post.Comments.Any())
            {
                Console.WriteLine("A published post cannot be moved to drafts if there are comments attached to it.");
                return;
            }

            post.IsPublished = false;
            post.PublishedDate = null;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post moved to drafts successfully.");
        }
    }
}