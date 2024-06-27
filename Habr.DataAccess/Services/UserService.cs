using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Habr.DataAccess.Interfaces;
using Habr.DataAccess.Servicec;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Habr.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private const int MaxNameLength = 100;
        private const int MaxEmailLength = 200;
        private const int MaxPasswordLength = 100;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public async Task RegisterAsync()
        {
            Console.Write("Enter your name: ");
            var name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Name is required.");
                return;
            }

            if (name.Length > MaxNameLength)
            {
                Console.WriteLine($"Name must be less than {MaxNameLength} characters.");
                return;
            }

            Console.Write("Enter your email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return;
            }

            if (email.Length > MaxEmailLength)
            {
                Console.WriteLine($"Email must be less than {MaxEmailLength} characters.");
                return;
            }

            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                Console.WriteLine("The email is already taken.");
                return;
            }

            Console.Write("Enter your password: ");
            var password = PasswordService.ReadPassword();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Password is required.");
                return;
            }

            if (password.Length > MaxPasswordLength)
            {
                Console.WriteLine($"Password must be less than {MaxPasswordLength} characters.");
                return;
            }

            var hashedPassword = PasswordService.HashPassword(password);
            var user = new User
            {
                Name = name,
                Email = email,
                Password = hashedPassword,
                RegisteredDate = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            Console.WriteLine("User registered successfully.");
        }

        public async Task<User> LoginAsync()
        {
            Console.Write("Enter your email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Email is required.");
                return null;
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine("The email is incorrect.");
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

        public async Task ConfirmEmailAsync(string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine("User with the given email does not exist.");
                return;
            }

            user.EmailConfirmed = true;
            await _context.SaveChangesAsync();
            Console.WriteLine("Email confirmed successfully.");
        }

        private bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}
