using DataScribeCSP.Options;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace DataScribeCSP.Services
{
    public class GoogleCloudStorageService : IGoogleCloudStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly ILogger logger;
        private string credentialsPath = @"C:\Users\roman\Downloads\google\elevated-style-430619-r8-f2d6fc0d1537.json";
        public GoogleCloudStorageService(IOptions<GoogleOptions> opt)
        {
            _bucketName = opt.Value.BucketName;
            if (string.IsNullOrEmpty(_bucketName))
            {
                throw new ArgumentNullException(nameof(_bucketName), "Bucket name must be provided in the configuration.");
            }

            GoogleCredential credential = GoogleCredential.FromFile(credentialsPath);
            _storageClient = StorageClient.Create(credential);

        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var objectName = Path.GetFileName(fileName);
            await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);
            return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var objName = Path.GetFileName(fileName);
            await _storageClient.DeleteObjectAsync(_bucketName, objName);
        }

        public async Task<List<string>> ShowFiles()
        {
            var fileNames = new List<string>();

            try
            {
                var blobs = _storageClient.ListObjects(_bucketName);

                foreach (var blob in blobs)
                {
                    try
                    {
                        var segments = blob.Name.Split('/');
                        var segment = segments[segments.Length - 1];

                        if (!string.IsNullOrEmpty(segment))
                        {
                            fileNames.Add(segment);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"An exception occurred while processing blob '{blob.Name}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while listing blobs: {ex.Message}");
            }

            return fileNames;
        }

        public async Task DownloadFile(string fileName, string localPath)
        {
            var objName = Path.GetFileName(fileName);
            using var outputFile = File.OpenWrite(localPath);
            var obj = await _storageClient.DownloadObjectAsync(_bucketName, fileName, outputFile);
        }

    }

    public interface IGoogleCloudStorageService
    {
        Task DownloadFile(string fileName, string localPath);
        Task<List<string>> ShowFiles();
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task DeleteFileAsync(string fileName);
    }
}