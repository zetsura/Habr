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
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DataContext>();
                context.Database.Migrate();

                Console.WriteLine("Database created and migration applied.");

                var userService = services.GetRequiredService<UserService>();
                var postService = services.GetRequiredService<PostService>();
                var commentService = services.GetRequiredService<CommentService>();

                while (true)
                {
                    Console.WriteLine("1. Register");
                    Console.WriteLine("2. Login");
                    Console.WriteLine("3. Exit");
                    Console.Write("Select an option: ");
                    var option = Console.ReadLine();

                    if (option == "1")
                    {
                        userService.Register();
                    }
                    else if (option == "2")
                    {
                        var user = userService.Login();
                        if (user != null)
                        {
                            UserMenu(postService, commentService, user);
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
                    services.AddTransient<UserService>();
                    services.AddTransient<PostService>();
                    services.AddTransient<CommentService>();
                });

        static void UserMenu(PostService postService, CommentService commentService, User user)
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
                        postService.ViewAllPosts();
                        break;
                    case "2":
                        postService.CreatePost(user);
                        break;
                    case "3":
                        postService.EditPost(user);
                        break;
                    case "4":
                        postService.DeletePost(user);
                        break;
                    case "5":
                        commentService.CommentOnPost(user);
                        break;
                    case "6":
                        commentService.ReplyToComment(user);
                        break;
                    case "7":
                        commentService.DeleteComment(user);
                        break;
                    case "8":
                        return;
                }
            }
        }
    }
}
