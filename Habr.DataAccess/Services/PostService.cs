using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Habr.Services
{
    public class PostService
    {
        private readonly DataContext _context;

        public PostService(DataContext context)
        {
            _context = context;
        }

        public void ViewAllPosts()
        {
            var posts = _context.Posts.Include(p => p.User).OrderByDescending(p => p.Created).ToList();
            foreach (var post in posts)
            {
                Console.WriteLine($"{post.Title} by {post.User.Email} at {post.Created}");
            }
        }

        public void CreatePost(User user)
        {
            Console.Write("Enter the title of your post: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title) || title.Length > 200)
            {
                Console.WriteLine("Title is required and must be less than 200 characters.");
                return;
            }

            Console.Write("Enter the text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text) || text.Length > 2000)
            {
                Console.WriteLine("Text is required and must be less than 2000 characters.");
                return;
            }

            var post = new Post { Title = title, Text = text, Created = DateTime.Now, UserId = user.Id };
            _context.Posts.Add(post);
            _context.SaveChanges();
            Console.WriteLine("Post created successfully.");
        }

        public void EditPost(User user)
        {
            Console.Write("Enter the ID of the post you want to edit: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = _context.Posts.SingleOrDefault(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to edit it.");
                return;
            }

            Console.Write("Enter the new title of your post: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title) || title.Length > 200)
            {
                Console.WriteLine("Title is required and must be less than 200 characters.");
                return;
            }

            Console.Write("Enter the new text of your post: ");
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text) || text.Length > 2000)
            {
                Console.WriteLine("Text is required and must be less than 2000 characters.");
                return;
            }

            post.Title = title;
            post.Text = text;
            _context.SaveChanges();
            Console.WriteLine("Post updated successfully.");
        }

        public void DeletePost(User user)
        {
            Console.Write("Enter the ID of the post you want to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = _context.Posts.SingleOrDefault(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to delete it.");
                return;
            }

            _context.Posts.Remove(post);
            _context.SaveChanges();
            Console.WriteLine("Post deleted successfully.");
        }
    }
}

