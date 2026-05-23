using System;
using System.IO;
using System.Threading;
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

        public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            var filePath = ResolveSafeStoragePath(storagePath);
            if (filePath == null || !File.Exists(filePath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            return Task.FromResult<Stream?>(stream);
        }

        public Task DeleteFileAsync(string storagePath)
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
            return Task.CompletedTask;
        }

        private string? ResolveSafeStoragePath(string storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
            {
                return null;
            }

            var rootPath = Path.GetFullPath(_options.RootPath);
            var candidatePath = Path.IsPathRooted(storagePath)
                ? Path.GetFullPath(storagePath)
                : ResolveRelativeStoragePath(rootPath, storagePath);

            var normalizedRoot = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return candidatePath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)
                ? candidatePath
                : null;
        }

        private string ResolveRelativeStoragePath(string rootPath, string storagePath)
        {
            var relativePath = Path.GetFullPath(storagePath);
            var normalizedRoot = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (relativePath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                return relativePath;
            }

            return Path.GetFullPath(Path.Combine(rootPath, storagePath));
        }
    }
}
