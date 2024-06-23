using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Habr.Services
{
    public class PostService
    {
        private readonly DataContext _context;
        private const int MaxTitleLength = 200;
        private const int MaxTextLength = 2000;

        public PostService(DataContext context)
        {
            _context = context;
        }

        public async Task ViewAllPostsAsync()
        {
            var posts = await _context.Posts.Include(p => p.User)
                                            .Include(p => p.Comments)
                                            .ThenInclude(c => c.User)
                                            .Where(p => p.IsPublished)
                                            .OrderByDescending(p => p.Created)
                                            .AsNoTracking()
                                            .ToListAsync();

            if (posts.Count == 0)
            {
                Console.WriteLine("No published posts found.");
                return;
            }

            Console.WriteLine("\nPosts:");
            foreach (var post in posts)
            {
                Console.WriteLine($"ID: {post.Id}, Title: {post.Title}, by {post.User.Email} at {post.Created}, Updated at {post.UpdatedDate}\n");
                if (post.Comments != null && post.Comments.Count > 0)
                {
                    Console.WriteLine("Comments:");
                    foreach (var comment in post.Comments)
                    {
                        Console.WriteLine($"ID: {comment.Id}, Comment by {comment.User.Email} at {comment.Created}: {comment.Content}\n");
                    }
                }
            }
        }


        public async Task CreatePostAsync(User user)
        {
            Console.Write("Enter the title of your post: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine("Title is required.");
                return;
            }

            if (title.Length > MaxTitleLength)
            {
                Console.WriteLine($"Title must be less than {MaxTitleLength} characters.");
                return;
            }

            Console.Write("Enter the text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Text is required.");
                return;
            }

            if (text.Length > MaxTextLength)
            {
                Console.WriteLine($"Text must be less than {MaxTextLength} characters.");
                return;
            }

            var post = new Post
            {
                Title = title,
                Text = text,
                Created = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                UserId = user.Id,
                IsPublished = true
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            Console.WriteLine("Post created successfully.");
        }


        public async Task EditPostAsync(User user)
        {
            Console.Write("Enter the ID of the post you want to edit: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to edit it.");
                return;
            }

            Console.Write("Enter the new title of your post: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine("Title is required.");
                return;
            }

            if (title.Length > MaxTitleLength)
            {
                Console.WriteLine($"Title must be less than {MaxTitleLength} characters.");
                return;
            }

            Console.Write("Enter the new text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Text is required.");
                return;
            }

            if (text.Length > MaxTextLength)
            {
                Console.WriteLine($"Text must be less than {MaxTextLength} characters.");
                return;
            }

            post.Title = title;
            post.Text = text;
            post.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post updated successfully.");
        }


        public async Task DeletePostAsync(User user)
        {
            Console.Write("Enter the ID of the post you want to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to delete it.");
                return;
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            Console.WriteLine("Post deleted successfully.");
        }


        public async Task PublishPostAsync(User user)
        {
            Console.Write("Enter the ID of the post you want to publish: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to publish it.");
                return;
            }

            post.IsPublished = true;
            post.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post published successfully.");
        }
    }
}
