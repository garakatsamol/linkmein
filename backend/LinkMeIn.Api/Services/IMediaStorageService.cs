using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LinkMeIn.Api.Services
{
    public interface IMediaStorageService
    {
        Task<string> SaveFileAsync(string postId, string fileName, Stream fileStream, string contentType);
        Task<Stream?> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string storagePath);
    }
}
