using Lib.FileManager.Net8.src.Services;
using Lib.FileManager.Net8.Src.Configs;
using Lib.FileManager.Net8.Src.Exceptions;
using Lib.FileManager.Net8.Src.Interfaces;
using Lib.FileManager.Net8.Src.Models;
using Microsoft.Extensions.Options;

namespace Lib.FileManager.Net8.Src.Services;

public class FileManagerService(
    IStorageProvider storageProvider,
    IFileValidator fileValidator,
    IFileNameGenerator fileNameGenerator,
    IOptions<FileManagerOptions> options) : IFileManager
{
    private readonly IStorageProvider _storageProvider = storageProvider;
    private readonly IFileValidator _fileValidator = fileValidator;
    private readonly IFileNameGenerator _fileNameGenerator = fileNameGenerator;
    private readonly FileManagerOptions _options = options.Value;

    public async Task<FileResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation du fichier
            var validationResult = await _fileValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return FileResult.Failure(validationResult.ErrorMessage!);
            }

            // Génération du nom de fichier sécurisé
            var fileName = _options.UseSecureNaming
                ? _fileNameGenerator.GenerateSecureName(request.FileName)
                : request.FileName;

            var finalPath = Path.Combine(request.DestinationPath, fileName).Replace('\\', '/');

            // Création de la requête finale
            var uploadRequest = request with
            {
                FileName = fileName,
                DestinationPath = finalPath
            };

            // Sauvegarde du fichier
            var result = await _storageProvider.SaveAsync(uploadRequest, cancellationToken);

            return result;
        }
        catch (FileManagerException ex)
        {
            Console.WriteLine(ex.ToString(), $"File upload failed for {request.FileName}");
            return FileResult.AsFailure(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString(), $"File upload failed for {request.FileName}");
            return FileResult.AsFailure("An unexpected error occurred during file upload.");
        }
    }

    public async Task<FileResult> MoveAsync(FileMoveRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérification de l'existence du fichier source
            if (!await _storageProvider.ExistsAsync(request.SourcePath, cancellationToken))
            {
                return FileResult.AsFailure("Source file does not exist.");
            }

            var result = await _storageProvider.MoveAsync(request.SourcePath, request.DestinationPath, cancellationToken);

            Console.WriteLine($"File moved from {request.SourcePath} to {request.DestinationPath}");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString(), $"Error moving file from {request.SourcePath} to {request.DestinationPath}");
            return FileResult.AsFailure($"Error moving file: {ex.Message}");
        }
    }

    public async Task<bool> DeleteAsync(FileDeleteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _storageProvider.DeleteAsync(request.FilePath, cancellationToken);

            if (result)
            {
                Console.WriteLine($"File deleted successfully: {request.FilePath}");
            }
            else
            {
                Console.WriteLine($"File deletion failed: {request.FilePath}");
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString(), $"Error deleting file: {request.FilePath}");
            return false;
        }
    }

    public Task<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return _storageProvider.GetAsync(filePath, cancellationToken);
    }

    public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return _storageProvider.ExistsAsync(filePath, cancellationToken);
    }

    public Task<FileMetadata> GetMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return _storageProvider.GetMetadataAsync(filePath, cancellationToken);
    }
}

public static class FileResultFluentExtensions
{
    public static FileResult AsFailure(this string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };

    public static FileResult AsSuccess(this (string path, string name, long size, string type) fileInfo) => new()
    {
        Success = true,
        FilePath = fileInfo.path,
        FileName = fileInfo.name,
        FileSize = fileInfo.size,
        ContentType = fileInfo.type,
        UploadedAt = DateTime.UtcNow
    };
}