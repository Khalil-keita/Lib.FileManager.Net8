using Lib.FileManager.Net8.Src.Models;

namespace Lib.FileManager.Net8.Src.Interfaces
{
    public interface IStorageProvider
    {
        Task<FileResult> SaveAsync(FileUploadRequest request, CancellationToken cancellationToken = default);

        FileResult Move(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

        Task<Stream> GetAsync(string filePath, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);

        Task<FileMetadata> GetMetadataAsync(string filePath, CancellationToken cancellationToken = default);
    }
}