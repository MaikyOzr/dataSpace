using DataScribeCSP.Models;
using DataScribeCSP.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataScribeCSP.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly PasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly DbRepository _dbRepo;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(UserManager<User> userManager, PasswordService passwordService,
            IJwtService jwtService, DbRepository dbRepo, IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _dbRepo = dbRepo;
            _contextAccessor = contextAccessor;
        }

        public async Task<AuthModel> RegisterAsync(UserDTO userDTO)
        {
            var user = new User
            {
                Email = userDTO.Email,
                Name = userDTO.Name,
                UserName = userDTO.UserName,
                PasswordHash = userDTO.Password
            };

            var passwordHash = _passwordService.HashPassword(user, userDTO.Password);
            user.PasswordHash = passwordHash;

            var res = await _userManager.CreateAsync(user, user.PasswordHash);

            if (!res.Succeeded)
            {
                var errors = string.Join(", ", res.Errors.Select(e => e.Description));
                throw new Exception($"Registration Problem: {errors}");
            }

            return new AuthModel
            {
                AccessToken = _jwtService.GenerateToken(user)
            };
        }

        public async Task<AuthModel> LoginAsync(LoginUserRequest loginUser)
        {
            var user = await _userManager.FindByEmailAsync(loginUser.Email);
            if (user == null) throw new Exception("user doesnt exist");

            var verificationResult = _passwordService.VerifyHashedPassword(user, user.PasswordHash!, loginUser.Password);
            if (verificationResult == null) throw new Exception("Invalid password.");
            var token = _jwtService.GenerateToken(user);
            return new AuthModel
            {
                AccessToken = token
            }; ;
        }

        public async Task<AuthModel> EditAsync(UpdateUserRequest userRequest)
        {
            var userId = _contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) { throw new Exception("USER DOES`NT EXIST!!"); }
            user.Name = userRequest.Name;
            user.Email = userRequest.Email;
            user.UserName = userRequest.UserName;
            await _userManager.UpdateAsync(user);
            return new AuthModel { AccessToken = _jwtService.GenerateToken(user) };
        }

        public async Task DeleteAsync()
        {
            var userId = _contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) { throw new Exception("USER DOES`NT EXIST!!"); }
            await _userManager.DeleteAsync(user);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) { throw new Exception("USER DOES`NT EXIST!!"); }
            return user;
        }

        public async Task ClearList(User user)
        {
            user.Files.Clear();
        }
    }

    public interface IUserService
    {
        Task<AuthModel> RegisterAsync(UserDTO userDTO);
        Task<AuthModel> EditAsync(UpdateUserRequest userRequest);
        Task DeleteAsync();
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<AuthModel> LoginAsync(LoginUserRequest loginUser);
    }
}
