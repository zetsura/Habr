using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Habr.DataAccess;
using Habr.DataAccess.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Habr.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DataContext>();
                context.Database.Migrate();

                Console.WriteLine("Database created and migration applied.");

                while (true)
                {
                    Console.WriteLine("1. Register");
                    Console.WriteLine("2. Login");
                    Console.WriteLine("3. Exit");
                    Console.Write("Select an option: ");
                    var option = Console.ReadLine();

                    if (option == "1")
                    {
                        Register(context);
                    }
                    else if (option == "2")
                    {
                        var user = Login(context);
                        if (user != null)
                        {
                            UserMenu(context, user);
                        }
                    }
                    else if (option == "3")
                    {
                        break;
                    }
                }
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("myCon");
                    services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
                });

        static void Register(DataContext context)
        {
            Console.Write("Enter your name: ");
            var name = Console.ReadLine();
            Console.Write("Enter your email: ");
            var email = Console.ReadLine();

            var existingUser = context.Users.SingleOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                Console.WriteLine("An account with this email already exists.");
                return;
            }

            Console.Write("Enter your password: ");
            var password = ReadPassword();

            var hashedPassword = HashPassword(password);
            var user = new User { Name = name, Email = email, Password = hashedPassword };
            context.Users.Add(user);
            context.SaveChanges();
            Console.WriteLine("User registered successfully.");
        }

        static User Login(DataContext context)
        {
            Console.Write("Enter your email: ");
            var email = Console.ReadLine();
            Console.Write("Enter your password: ");
            var password = ReadPassword();

            var hashedPassword = HashPassword(password);
            var user = context.Users.SingleOrDefault(u => u.Email == email && u.Password == hashedPassword);
            if (user == null)
            {
                Console.WriteLine("Invalid email or password");
            }
            return user;
        }

        static void UserMenu(DataContext context, User user)
        {
            while (true)
            {
                Console.WriteLine("1. View all posts");
                Console.WriteLine("2. Create a post");
                Console.WriteLine("3. Edit a post");
                Console.WriteLine("4. Delete a post");
                Console.WriteLine("5. Comment on a post");
                Console.WriteLine("6. Reply to a comment");
                Console.WriteLine("7. Delete a comment");
                Console.WriteLine("8. Logout");
                Console.Write("Select an option: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        ViewAllPosts(context);
                        break;
                    case "2":
                        CreatePost(context, user);
                        break;
                    case "3":
                        EditPost(context, user);
                        break;
                    case "4":
                        DeletePost(context, user);
                        break;
                    case "5":
                        CommentOnPost(context, user);
                        break;
                    case "6":
                        ReplyToComment(context, user);
                        break;
                    case "7":
                        DeleteComment(context, user);
                        break;
                    case "8":
                        return;
                }
            }
        }

        static void ViewAllPosts(DataContext context)
        {
            var posts = context.Posts.Include(p => p.User).Include(p => p.Comments).ToList();
            foreach (var post in posts)
            {
                Console.WriteLine($"{post.Id}. {post.Title} by {post.User.Name} at {post.Created}");
                Console.WriteLine(post.Text);
                foreach (var comment in post.Comments)
                {
                    Console.WriteLine($"  {comment.Id}. {comment.Content} by {comment.User.Name} at {comment.Created}");
                }
            }
        }

        static void CreatePost(DataContext context, User user)
        {
            Console.Write("Enter the title of your post: ");
            var title = Console.ReadLine();
            Console.Write("Enter the text of your post: ");
            var text = Console.ReadLine();

            var post = new Post { Title = title, Text = text, Created = DateTime.Now, UserId = user.Id };
            context.Posts.Add(post);
            context.SaveChanges();
            Console.WriteLine("Post created successfully.");
        }

        static void EditPost(DataContext context, User user)
        {
            Console.Write("Enter the ID of the post you want to edit: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = context.Posts.SingleOrDefault(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to edit it.");
                return;
            }

            Console.Write("Enter the new title of your post: ");
            post.Title = Console.ReadLine();
            Console.Write("Enter the new text of your post: ");
            post.Text = Console.ReadLine();
            context.SaveChanges();
            Console.WriteLine("Post updated successfully.");
        }

        static void DeletePost(DataContext context, User user)
        {
            Console.Write("Enter the ID of the post you want to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = context.Posts.SingleOrDefault(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                Console.WriteLine("Post not found or you don't have permission to delete it.");
                return;
            }

            context.Posts.Remove(post);
            context.SaveChanges();
            Console.WriteLine("Post deleted successfully.");
        }

        static void CommentOnPost(DataContext context, User user)
        {
            Console.Write("Enter the ID of the post you want to comment on: ");
            if (!int.TryParse(Console.ReadLine(), out var postId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var post = context.Posts.SingleOrDefault(p => p.Id == postId);
            if (post == null)
            {
                Console.WriteLine("Post not found.");
                return;
            }

            Console.Write("Enter your comment: ");
            var content = Console.ReadLine();
            var comment = new Comment { Content = content, Created = DateTime.Now, UserId = user.Id, PostId = post.Id };
            context.Comments.Add(comment);
            context.SaveChanges();
            Console.WriteLine("Comment added successfully.");
        }

        static void ReplyToComment(DataContext context, User user)
        {
            Console.Write("Enter the ID of the comment you want to reply to: ");
            if (!int.TryParse(Console.ReadLine(), out var commentId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var comment = context.Comments.SingleOrDefault(c => c.Id == commentId);
            if (comment == null)
            {
                Console.WriteLine("Comment not found.");
                return;
            }

            Console.Write("Enter your reply: ");
            var content = Console.ReadLine();
            var reply = new Comment { Content = content, Created = DateTime.Now, UserId = user.Id, PostId = comment.PostId, ParentCommentId = comment.Id };
            context.Comments.Add(reply);
            context.SaveChanges();
            Console.WriteLine("Reply added successfully.");
        }

        static void DeleteComment(DataContext context, User user)
        {
            Console.Write("Enter the ID of the comment you want to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var commentId))
            {
                Console.WriteLine("Invalid ID format");
                return;
            }

            var comment = context.Comments.Include(c => c.Replies).SingleOrDefault(c => c.Id == commentId && c.UserId == user.Id);
            if (comment == null)
            {
                Console.WriteLine("Comment not found or you don't have permission to delete it.");
                return;
            }

            context.Comments.Remove(comment);
            context.SaveChanges();
            Console.WriteLine("Comment deleted successfully.");
        }

        static string ReadPassword()
        {
            StringBuilder password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            return password.ToString();
        }

        static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (var t in bytes)
            {
                builder.Append(t.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
