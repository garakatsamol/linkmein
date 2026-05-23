using System.IO;
using System.Threading.Tasks;

namespace LinkMeIn.Api.Services
{
    public interface IMediaStorageService
    {
        Task<string> SaveFileAsync(string postId, string fileName, Stream fileStream, string contentType);
        Task DeleteFileAsync(string storagePath);
    }
}
