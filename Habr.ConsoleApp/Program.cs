using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Habr.Services;

namespace Habr.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DataContext>();
                await context.Database.MigrateAsync();

                Console.WriteLine("Database created and migration applied.");

                var userService = services.GetRequiredService<UserService>();
                var postService = services.GetRequiredService<PostService>();
                var commentService = services.GetRequiredService<CommentService>();

                await MainMenuAsync(userService, postService, commentService);
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
                    services.AddTransient<UserService>();
                    services.AddTransient<PostService>();
                    services.AddTransient<CommentService>();
                });

        static async Task MainMenuAsync(UserService userService, PostService postService, CommentService commentService)
        {
            while (true)
            {
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await userService.RegisterAsync();
                        break;
                    case "2":
                        var user = await userService.LoginAsync();
                        if (user != null)
                        {
                            await UserMenuAsync(postService, commentService, user);
                        }
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please select again.");
                        break;
                }
            }
        }

        static async Task UserMenuAsync(PostService postService, CommentService commentService, User user)
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
                        await postService.ViewAllPostsAsync();
                        break;
                    case "2":
                        Console.Write("Do you want to publish the post? (yes/no): ");
                        var publishResponse = Console.ReadLine()?.ToLower();
                        var isPublished = publishResponse == "yes";
                        await postService.CreatePostAsync(user, isPublished);
                        break;
                    case "3":
                        await postService.EditPostAsync(user);
                        break;
                    case "4":
                        await postService.DeletePostAsync(user);
                        break;
                    case "5":
                        await commentService.CommentOnPostAsync(user);
                        break;
                    case "6":
                        await commentService.ReplyToCommentAsync(user);
                        break;
                    case "7":
                        await commentService.DeleteCommentAsync(user);
                        break;
                    case "8":
                        await postService.ViewAllDraftsAsync(user);
                        break;
                    case "9":
                        await postService.PublishPostAsync(user);
                        break;
                    case "10":
                        await postService.MoveToDraftsAsync(user);
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
