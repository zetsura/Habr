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

            Console.WriteLine("\n====================");
            Console.WriteLine("Published Posts:");
            Console.WriteLine("====================");

            foreach (var post in posts)
            {
                Console.WriteLine($"\nPost ID: {post.Id}");
                Console.WriteLine($"Title: {post.Title}");
                Console.WriteLine($"Text: {post.Text}");
                Console.WriteLine($"Author: {post.User.Email}");
                Console.WriteLine($"Created: {post.Created}");
                Console.WriteLine($"Updated: {post.UpdatedDate}");

                if (post.Comments != null && post.Comments.Count > 0)
                {
                    Console.WriteLine("\nComments:");
                    foreach (var comment in post.Comments)
                    {
                        Console.WriteLine($"\tComment ID: {comment.Id}");
                        Console.WriteLine($"\tComment by: {comment.User.Email}");
                        Console.WriteLine($"\tCreated: {comment.Created}");
                        Console.WriteLine($"\tContent: {comment.Content}\n");
                    }
                }
                Console.WriteLine("--------------------");
            }
        }

        public async Task ViewAllDraftsAsync(User user)
        {
            var drafts = await _context.Posts.Include(p => p.User)
                                             .Where(p => !p.IsPublished && p.UserId == user.Id)
                                             .OrderByDescending(p => p.Created)
                                             .AsNoTracking()
                                             .ToListAsync();

            if (drafts.Count == 0)
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
                Console.WriteLine($"Text: {draft.Text}");
                Console.WriteLine($"Author: {draft.User.Email}");
                Console.WriteLine($"Created: {draft.Created}");
                Console.WriteLine($"Updated: {draft.UpdatedDate}");
                Console.WriteLine("--------------------");
            }
        }

        public async Task CreatePostAsync(User user, bool isPublished)
        {
            Console.Write("Enter the title of your post: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine("The title is required.");
                return;
            }

            if (title.Length > MaxTitleLength)
            {
                Console.WriteLine($"The title must be less than {MaxTitleLength} symbols.");
                return;
            }

            Console.Write("Enter the text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("The text is required.");
                return;
            }

            if (text.Length > MaxTextLength)
            {
                Console.WriteLine($"The text must be less than {MaxTextLength} symbols.");
                return;
            }

            var post = new Post
            {
                Title = title,
                Text = text,
                Created = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                UserId = user.Id,
                IsPublished = isPublished
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            Console.WriteLine(isPublished ? "Post created and published successfully." : "Post created as draft successfully.");
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

            if (title.Length > MaxTitleLength)
            {
                Console.WriteLine($"The title must be less than {MaxTitleLength} symbols.");
                return;
            }

            Console.Write("Enter the new text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("The text is required.");
                return;
            }

            if (text.Length > MaxTextLength)
            {
                Console.WriteLine($"The text must be less than {MaxTextLength} symbols.");
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
                Console.WriteLine("The post does not exist.");
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

            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id && !p.IsPublished);
            if (post == null)
            {
                Console.WriteLine("Draft not found or you don't have permission to publish it.");
                return;
            }

            post.IsPublished = true;
            post.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post published successfully.");
        }


        public async Task MoveToDraftsAsync(User user)
        {
            Console.Write("Enter the ID of the post you want to move to drafts: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = await _context.Posts.Include(p => p.Comments).SingleOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);
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

            if (post.Comments.Any())
            {
                Console.WriteLine("A published post cannot be moved to drafts if there are comments attached to it.");
                return;
            }

            post.IsPublished = false;
            post.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine("Post moved to drafts successfully.");
        }
    }
}