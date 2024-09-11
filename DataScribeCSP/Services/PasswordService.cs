using DataScribeCSP.Models;
using Microsoft.AspNetCore.Identity;

namespace DataScribeCSP.Services
{
    public class PasswordService
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public PasswordService(IPasswordHasher<User> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string HashPassword(User user, string password)
            => _passwordHasher.HashPassword(user, password);

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
            => _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }
}
