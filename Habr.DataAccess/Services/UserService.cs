using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Habr.DataAccess.Servicec;

namespace Habr.Services
{
    public class UserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public void Register()
        {
            Console.Write("Enter your name: ");
            var name = Console.ReadLine();
            if (string.IsNullOrEmpty(name) || name.Length > 100)
            {
                Console.WriteLine("Name is required and must be less than 100 characters.");
                return;
            }

            Console.Write("Enter your email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email) || email.Length > 200)
            {
                Console.WriteLine("Email is required and must be less than 200 characters.");
                return;
            }

            var existingUser = _context.Users.SingleOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                Console.WriteLine("Email address is already taken.");
                return;
            }

            Console.Write("Enter your password: ");
            var password = PasswordService.ReadPassword();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Password is required.");
                return;
            }

            var hashedPassword = PasswordService.HashPassword(password);
            var user = new User { Name = name, Email = email, Password = hashedPassword };
            _context.Users.Add(user);
            _context.SaveChanges();
            Console.WriteLine("User registered successfully.");
        }

        public User Login()
        {
            Console.Write("Enter your email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Email is required.");
                return null;
            }

            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine("Email address is incorrect.");
                return null;
            }

            Console.Write("Enter your password: ");
            var password = PasswordService.ReadPassword();

            var hashedPassword = PasswordService.HashPassword(password);

            if (user.Password != hashedPassword)
            {
                Console.WriteLine("Invalid email or password");
                return null;
            }
            return user;
        }
    }
}
