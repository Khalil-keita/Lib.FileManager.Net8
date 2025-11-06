namespace Lib.FileManager.Net8.Src.Models
{
    public record FileUploadRequest
    {
        public Stream Content { get; init; } = Stream.Null;
        public string FileName { get; init; } = string.Empty;
        public string DestinationPath { get; init; } = string.Empty;
        public bool OverwriteIfExists { get; init; } = false;
        public long? MaxFileSize { get; init; }
        public IReadOnlyList<string> AllowedExtensions { get; init; } = Array.Empty<string>();
        public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
    }

    public record FileMoveRequest
    {
        public string SourcePath { get; init; } = string.Empty;
        public string DestinationPath { get; init; } = string.Empty;
        public bool OverwriteIfExists { get; init; } = false;
    }

    public record FileDeleteRequest
    {
        public string FilePath { get; init; } = string.Empty;
        public bool Permanent { get; init; } = true;
    }

    public record FileResult
    {
        public bool Success { get; init; }
        public string FilePath { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public long FileSize { get; init; }
        public string ContentType { get; init; } = string.Empty;
        public string? ErrorMessage { get; init; }
        public DateTime UploadedAt { get; init; } = DateTime.UtcNow;

        public static FileResult AsFailure(string errorMessage) => new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };

        public static FileResult AsSuccess(string filePath, string fileName, long fileSize, string contentType) => new()
        {
            Success = true,
            FilePath = filePath,
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
            UploadedAt = DateTime.UtcNow
        };
    }

    public record FileMetadata
    {
        public string FilePath { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public long FileSize { get; init; }
        public string ContentType { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime ModifiedAt { get; init; }
        public DateTime? LastAccessedAt { get; init; }
        public IDictionary<string, object> CustomMetadata { get; init; } = new Dictionary<string, object>();
    }

    public record FileValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }

        public static FileValidationResult Valid() => new() { IsValid = true };
        public static FileValidationResult Invalid(string errorMessage) => new()
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}