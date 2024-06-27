using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Habr.Services;
using Habr.DataAccess.Interfaces;

namespace Habr.ConsoleApp
{
    class Program
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;

        public Program(IUserService userService, IPostService postService, ICommentService commentService)
        {
            _userService = userService;
            _postService = postService;
            _commentService = commentService;
        }

        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DataContext>();
                await context.Database.MigrateAsync();

                Console.WriteLine("Database created and migration applied.");

                var program = services.GetRequiredService<Program>();
                await program.MainMenuAsync();
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
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<IPostService, PostService>();
                    services.AddTransient<ICommentService, CommentService>();
                    services.AddTransient<Program>();
                });

        public async Task MainMenuAsync()
        {
            while (true)
            {
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Confirm Email");
                Console.WriteLine("4. Exit");
                Console.Write("Select an option: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await _userService.RegisterAsync();
                        break;
                    case "2":
                        var user = await _userService.LoginAsync();
                        if (user != null && user.EmailConfirmed)
                        {
                            await UserMenuAsync(user);
                        }
                        else if (user != null && !user.EmailConfirmed)
                        {
                            Console.WriteLine("Please confirm your email before logging in.");
                        }
                        break;
                    case "3":
                        Console.Write("Enter your email to confirm: ");
                        var email = Console.ReadLine();
                        await _userService.ConfirmEmailAsync(email);
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please select again.");
                        break;
                }
            }
        }

        public async Task UserMenuAsync(User user)
        {
            while (true)
            {
                Console.WriteLine("\n====================");
                Console.WriteLine("User Menu:");
                Console.WriteLine("====================");
                Console.WriteLine("1. View all posts");
                Console.WriteLine("2. Create a post");
                Console.WriteLine("3. Edit a post");
                Console.WriteLine("4. Delete a post");
                Console.WriteLine("5. Comment on a post");
                Console.WriteLine("6. Reply to a comment");
                Console.WriteLine("7. Delete a comment");
                Console.WriteLine("8. View all drafts");
                Console.WriteLine("9. Publish a draft post");
                Console.WriteLine("10. Move published post to drafts");
                Console.WriteLine("11. Logout");
                Console.Write("Select an option: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await _postService.ViewAllPostsAsync();
                        break;
                    case "2":
                        Console.Write("Do you want to publish the post? (yes/no): ");
                        var publishResponse = Console.ReadLine()?.ToLower();
                        var isPublished = publishResponse == "yes";
                        await _postService.CreatePostAsync(user, isPublished);
                        break;
                    case "3":
                        await _postService.EditPostAsync(user);
                        break;
                    case "4":
                        await _postService.DeletePostAsync(user);
                        break;
                    case "5":
                        await _commentService.CommentOnPostAsync(user);
                        break;
                    case "6":
                        await _commentService.ReplyToCommentAsync(user);
                        break;
                    case "7":
                        await _commentService.DeleteCommentAsync(user);
                        break;
                    case "8":
                        await _postService.ViewAllDraftsAsync(user);
                        break;
                    case "9":
                        await _postService.PublishPostAsync(user);
                        break;
                    case "10":
                        await _postService.MoveToDraftsAsync(user);
                        break;
                    case "11":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please select again.");
                        break;
                }
            }
        }
    }
}
