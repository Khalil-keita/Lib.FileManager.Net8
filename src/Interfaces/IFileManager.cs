using Lib.FileManager.Net8.Src.Models;

namespace Lib.FileManager.Net8.Src.Interfaces
{
    public interface IFileManager
    {
        Task<FileResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);

        Task<FileResult> MoveAsync(FileMoveRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(FileDeleteRequest request, CancellationToken cancellationToken = default);

        Task<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);

        Task<FileMetadata> GetMetadataAsync(string filePath, CancellationToken cancellationToken = default);

        Task<IEnumerable<FileMetadata>> ListAsync(string directoryPath, CancellationToken cancellationToken = default);
    }
}