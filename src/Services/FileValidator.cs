// FileManager.Core/Security/IFileValidator.cs
using Lib.FileManager.Net8.Src.Configs;
using Lib.FileManager.Net8.Src.Models;
using Microsoft.Extensions.Options;

namespace Lib.FileManager.Net8.src.Services;

public interface IFileValidator
{
    Task<FileValidationResult> ValidateAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
}

public class FileValidator(IOptions<FileManagerOptions> options) : IFileValidator
{
    private readonly FileManagerOptions _options = options.Value;

    public async Task<FileValidationResult> ValidateAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        // Vérification de la taille du fichier
        var maxFileSize = request.MaxFileSize ?? _options.DefaultMaxFileSize;
        if (request.Content.Length > maxFileSize)
        {
            return FileValidationResult.Invalid($"File size exceeds maximum allowed size of {maxFileSize} bytes.");
        }

        // Vérification de l'extension
        var allowedExtensions = request.AllowedExtensions.Any()
            ? request.AllowedExtensions
            : _options.DefaultAllowedExtensions;

        var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (allowedExtensions.Any() && !allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            return FileValidationResult.Invalid($"File extension '{fileExtension}' is not allowed.");
        }

        // Vérification du type MIME (basique)
        if (!await IsSafeContentTypeAsync(request.Content, fileExtension, cancellationToken))
        {
            return FileValidationResult.Invalid("File content type is not allowed.");
        }

        return FileValidationResult.Valid();
    }

    private static async Task<bool> IsSafeContentTypeAsync(Stream content, string fileExtension, CancellationToken cancellationToken)
    {
        // Implémentation basique - à étendre selon les besoins
        try
        {
            // Réinitialiser le stream pour la lecture
            if (content.CanSeek)
                content.Position = 0;

            // Vérification des magic numbers pour les types courants
            var buffer = new byte[8];
            var bytesRead = await content.ReadAsync(buffer, cancellationToken);

            if (bytesRead >= 2)
            {
                // Vérification PNG
                if (buffer[0] == 0x89 && buffer[1] == 0x50 && fileExtension == ".png")
                    return true;

                // Vérification JPEG
                if (buffer[0] == 0xFF && buffer[1] == 0xD8 && fileExtension == ".jpg")
                    return true;

                // Vérification PDF
                if (buffer[0] == 0x25 && buffer[1] == 0x50 && fileExtension == ".pdf")
                    return true;
            }

            // Réinitialiser à nouveau pour l'utilisation ultérieure
            if (content.CanSeek)
                content.Position = 0;

            return true; // Fallback - à renforcer en production
        }
        catch
        {
            return false;
        }
    }
}