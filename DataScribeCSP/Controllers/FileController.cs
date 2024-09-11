using DataScribeCSP.Data;
using DataScribeCSP.Models;
using DataScribeCSP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataScribeCSP.Controllers
{
    [Authorize]
    [ApiController]
    public class FileController : Controller
    {
        private readonly GoogleCloudStorageService _storageService;
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;
        private readonly FileService _fileService;
        public FileController(GoogleCloudStorageService storageService,
            AppDbContext appDbContext, UserManager<User> userManager, FileService fileService)
        {
            _storageService = storageService;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _fileService = fileService;
        }

        [HttpGet]
        [Route("/files")]
        public async Task<List<string>> GetFiles()
        {
            return await _storageService.ShowFiles();
        }

        [HttpGet]
        [Route("/files-down/{fileName}/{localPath}")]
        public async Task DownloadFiles(string fileName, string localPath)
        {
            await _storageService.DownloadFile(fileName, localPath);
        }

        [HttpPost("/files-up")]
        public async Task<IActionResult> PostFile(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var url = await _storageService.UploadFileAsync(stream, file.FileName);
                await _fileService.UploadFile(file.FileName);
                return Ok(new { Url = url });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex);
            }
        }

        [HttpDelete("/files-delete/{fileId}")]
        public async Task DeleteFile([FromRoute] int fileId)
        {
            await _fileService.DeleteFile(fileId);
        }

        [HttpDelete("/clear-list")]
        public async Task ClearList()
        {
            await _fileService.ClearFileList();
        }
    }
}