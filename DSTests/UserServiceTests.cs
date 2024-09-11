using DataScribeCSP.Models;
using DataScribeCSP.Repository;
using DataScribeCSP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DSTests
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<DbRepository> _dbRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _contextAccessorMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);

            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            _jwtServiceMock = new Mock<IJwtService>();
            _dbRepositoryMock = new Mock<DbRepository>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();

            var passwordService = new PasswordService(_passwordHasherMock.Object);

            _userService = new UserService(
                _userManagerMock.Object,
                passwordService,
                _jwtServiceMock.Object,
                _dbRepositoryMock.Object,
                _contextAccessorMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ValidUserAndPassword_ReturnsAuthModel()
        {
            var loginUser = new LoginUserRequest { Email = "roma@gmail.com", Password = "String123" };
            var user = new User { Email = loginUser.Email, PasswordHash = "hashedPassword" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginUser.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, loginUser.Password))
                .Returns(PasswordVerificationResult.Success);
            _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("jwt_token");

            var result = await _userService.LoginAsync(loginUser);

            Assert.NotNull(result);
            Assert.Equal("jwt_token", result.AccessToken);
        }

        [Fact]
        public async Task RegisterAsync_ReturnAuthModel()
        {
            var regUser = new UserDTO
            {
                Email = "romaoz@gmail.com",
                Name = "romaETH",
                Password = "String123",
                UserName = "ETHEREAL"
            };

            _passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<User>(),
                It.IsAny<string>())).Returns("hashed_password");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

            var result = await _userService.RegisterAsync(regUser);

            Assert.NotNull(result);
            Assert.Equal("jwt_token", result.AccessToken);
        }

    }
}