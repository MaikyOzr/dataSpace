using DataScribeCSP.Data;
using DataScribeCSP.Models;
using DataScribeCSP.Options;
using DataScribeCSP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace DSTests
{
    public class FileServiceTests
    {
        private readonly Mock<AppDbContext> _dbContextMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly GoogleCloudStorageService _storageService;
        private readonly Mock<IHttpContextAccessor> _contextAccessorMock;
        private readonly FileService _fileService;
        private readonly IOptions<GoogleOptions> opt;

        public FileServiceTests()
        {
            _dbContextMock = new Mock<AppDbContext>();
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            var configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

            var googleOptions = configuration.GetSection("GoogleCloudStorage").Get<GoogleOptions>();

            var options = Options.Create(googleOptions);

            _storageService = new GoogleCloudStorageService(options);

            _contextAccessorMock = new Mock<IHttpContextAccessor>();

            _fileService = new FileService(_dbContextMock.Object, _userManagerMock.Object,
                _storageService, _contextAccessorMock.Object);
        }

        [Fact]
        public async Task UploadFile_ShouldThrowUnauthorizedAccessException()
        {
            _contextAccessorMock.Setup(x => x.HttpContext.User.FindFirst(It.IsAny<string>()))
                .Returns((Claim)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _fileService.UploadFile("testfile.txt"));
        }

        [Fact]
        public async Task UploadFile_ShouldAddFileToUserAndSaveChanges()
        {
            var userId = 4;
            var user = new User { Id = userId, Files = new List<Files>() };

            _contextAccessorMock.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            _dbContextMock.Setup(x => x.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            await _fileService.UploadFile("testfile.txt");

            Assert.Single(user.Files);
            Assert.Equal($"{userId}/testfile.txt", user.Files.First().FileName);
            _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}
