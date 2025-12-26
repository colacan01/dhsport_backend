using Microsoft.AspNetCore.Http;

namespace DhSport.Infrastructure.Services;

public interface IFileService
{
    Task<(string FilePath, string FileName, long FileSize)> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default);
    bool FileExists(string filePath);
}
