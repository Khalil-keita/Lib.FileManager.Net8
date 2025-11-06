using Lib.FileManager.Net8.Src.Configs;
using Lib.FileManager.Net8.Src.Interfaces;
using Lib.FileManager.Net8.Src.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lib.FileManager.Net8.Src.Services;

public class LocalFileSystemProvider(
    IOptions<LocalFileSystemOptions> options,
    IHostEnvironment environment) : IStorageProvider
{
    private readonly LocalFileSystemOptions _options = options.Value;
    private readonly IHostEnvironment _environment = environment;

    public async Task<FileResult> SaveAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(request.DestinationPath);
        var directory = Path.GetDirectoryName(fullPath)!;

        // Création du répertoire si nécessaire
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Vérification si le fichier existe déjà
        if (File.Exists(fullPath) && !request.OverwriteIfExists)
        {
            return FileResult.AsFailure("File already exists and overwrite is disabled.");
        }

        // Sauvegarde du fichier
        await using var fileStream = File.Create(fullPath);
        await request.Content.CopyToAsync(fileStream, cancellationToken);

        var fileInfo = new FileInfo(fullPath);

        return FileResult.AsSuccess(
            request.DestinationPath,
            request.FileName,
            fileInfo.Length,
            GetContentType(request.FileName));
    }

    public FileResult Move(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        var sourceFullPath = GetFullPath(sourcePath);
        var destinationFullPath = GetFullPath(destinationPath);

        if (!File.Exists(sourceFullPath))
        {
            return FileResult.AsFailure("Source file does not exist.");
        }

        var destinationDirectory = Path.GetDirectoryName(destinationFullPath)!;
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Move(sourceFullPath, destinationFullPath, overwrite: true);

        var fileInfo = new FileInfo(destinationFullPath);

        return FileResult.AsSuccess(
            destinationPath,
            Path.GetFileName(destinationPath),
            fileInfo.Length,
            GetContentType(destinationPath));
    }

    public Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);

        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public Task<Stream> GetAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream>(stream);
    }

    public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<FileMetadata> GetMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(filePath);
        var fileInfo = new FileInfo(fullPath);

        if (!fileInfo.Exists)
            throw new FileNotFoundException($"File not found: {filePath}");

        var metadata = new FileMetadata
        {
            FilePath = filePath,
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            ContentType = GetContentType(fileInfo.Name),
            CreatedAt = fileInfo.CreationTimeUtc,
            ModifiedAt = fileInfo.LastWriteTimeUtc,
            LastAccessedAt = fileInfo.LastAccessTimeUtc
        };

        return Task.FromResult(metadata);
    }

    private string GetFullPath(string relativePath)
    {
        var rootPath = _options.UseWebRoot
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.ContentRootPath;

        return Path.Combine(rootPath, _options.RootPath, relativePath);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".html" => "text/html",
            _ => "application/octet-stream"
        };
    }
}