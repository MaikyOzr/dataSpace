using DataScribeCSP.Data;
using DataScribeCSP.Models;
using DataScribeCSP.Repository;
using DataScribeCSP.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataScribeCSP.Controllers
{
    [ApiController]
    public class RegistrationController : Controller
    {
        private readonly DbRepository _dbRepo;
        private readonly UserManager<User> _userManager;
        private readonly PasswordService _passwordService;
        private readonly UserService _userService;

        public RegistrationController(UserManager<User> userManager,
            PasswordService passwordService, UserService userService)
        {
            _dbRepo = new DbRepository(new AppDbContext());
            _userManager = userManager;
            _passwordService = passwordService;
            _userService = userService;
        }

        [HttpGet("/users")]
        public async Task<List<User>> GetUsers()
        {
            return await _userService.GetUsersAsync();
        }
        [HttpPost("/sign-in")]
        public async Task<IActionResult> SignIn([FromBody] LoginUserRequest loginUser)
        {
            var user = await _userService.LoginAsync(loginUser);
            return Ok(user);
        }

        [HttpPost("/sign-up")]
        public async Task<IActionResult> SignUp([FromBody] UserDTO userDTO)
        {
            var regUser = await _userService.RegisterAsync(userDTO);
            return Ok(regUser);
        }

        [HttpDelete("/users")]
        public async Task DeleteUser()
        {
            await _userService.DeleteAsync();
        }

        [HttpPatch("/users/{id}")]
        public async Task<IActionResult> PatchUser([FromBody] UpdateUserRequest userRequest)
        {
            var editUser = await _userService.EditAsync(userRequest);
            return Ok(editUser);
        }
    }
}
