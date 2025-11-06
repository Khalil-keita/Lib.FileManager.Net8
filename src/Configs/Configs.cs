namespace Lib.FileManager.Net8.Src.Configs;

public class FileManagerOptions
{
    public const string SectionName = "FileManager";

    public string DefaultStorageProvider { get; set; } = "LocalFileSystem";
    public long DefaultMaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB
    public IReadOnlyList<string> DefaultAllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
    public string BasePath { get; set; } = "uploads";
    public bool UseSecureNaming { get; set; } = true;
    public int FileNameLength { get; set; } = 32;
    public bool CreateDirectoryIfNotExists { get; set; } = true;
}

public class LocalFileSystemOptions
{
    public const string SectionName = "FileManager:LocalFileSystem";

    public string RootPath { get; set; } = "wwwroot/uploads";
    public bool UseWebRoot { get; set; } = true;
    public string TempPath { get; set; } = "temp";
}