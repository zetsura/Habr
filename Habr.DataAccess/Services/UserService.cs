using Habr.DataAccess;
using Habr.DataAccess.Entities;
using Habr.DataAccess.Interfaces;
using Habr.DataAccess.ApplicationConstants;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Habr.DataAccess.Servicec;

namespace Habr.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public async Task RegisterAsync()
        {
            Console.Write("Enter your email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return;
            }

            if (email.Length > Constants.MaxEmailLength)
            {
                Console.WriteLine($"Email must be less than {Constants.MaxEmailLength} characters.");
                return;
            }

            var isEmailTaken = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (isEmailTaken)
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

            if (password.Length > Constants.MaxPasswordLength)
            {
                Console.WriteLine($"Password must be less than {Constants.MaxPasswordLength} characters.");
                return;
            }

            var name = email.Split('@')[0];

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

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

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
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                Console.WriteLine("User with the given email does not exist.");
                return;
            }

            user.IsEmailConfirmed = true;
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