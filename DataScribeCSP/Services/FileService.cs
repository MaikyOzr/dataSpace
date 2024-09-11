

using DataScribeCSP.Data;
using DataScribeCSP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DataScribeCSP.Services
{
    public class FileService : IFileService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;
        private readonly GoogleCloudStorageService _storageService;
        private readonly IHttpContextAccessor _contextAccessor;
        public FileService(AppDbContext dbContext, UserManager<User> manager,
            GoogleCloudStorageService storageService, IHttpContextAccessor contextAccessor)
        {
            _appDbContext = dbContext;
            _userManager = manager;
            _storageService = storageService;
            _contextAccessor = contextAccessor;
        }

        public async Task DeleteFile(int fileId)
        {
            var userId = _contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var file = await _appDbContext.Files.FirstOrDefaultAsync(x => x.FileId == fileId);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (file != null)
            {
                user.Files.Remove(file);
                await _storageService.DeleteFileAsync(file.FileName);
                _appDbContext.Remove(file);
                await _appDbContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("FILE NOT FOUND");
            }
        }

        public async Task UploadFile(string fileName)
        {
            var userId = _contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            if (userId == null)
            {
                throw new UnauthorizedAccessException();
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var fileLink = $"{user.Id}/{fileName}";

            var newFile = new Files
            {
                FileName = fileLink,
                UserId = user.Id,
                User = user
            };

            user.Files.Add(newFile);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task ClearFileList()
        {
            var userId = _contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new Exception("user not found");
            }
            user!.Files.Clear();
            await _appDbContext.SaveChangesAsync();
        }
    }
    public interface IFileService
    {
        Task UploadFile(string fileName);
        Task DeleteFile(int fileId);
        Task ClearFileList();
    }
}
