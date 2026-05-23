using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using LinkMeIn.Api.Options;

namespace LinkMeIn.Api.Services
{
    public class LocalMediaStorageService : IMediaStorageService
    {
        private readonly MediaStorageOptions _options;

        public LocalMediaStorageService(IOptions<MediaStorageOptions> options)
        {
            _options = options.Value;
        }

        public async Task<string> SaveFileAsync(string postId, string fileName, Stream fileStream, string contentType)
        {
            var safeFileName = Path.GetRandomFileName() + Path.GetExtension(fileName);
            var postFolder = Path.Combine(_options.RootPath, postId);
            Directory.CreateDirectory(postFolder);
            var filePath = Path.Combine(postFolder, safeFileName);
            using (var outStream = File.Create(filePath))
            {
                await fileStream.CopyToAsync(outStream);
            }
            return filePath;
        }

        public Task DeleteFileAsync(string storagePath)
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
            return Task.CompletedTask;
        }
    }
}
