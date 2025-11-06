# Lib.FileManager.NET8

Une bibliothèque complète et robuste pour la gestion de fichiers en .NET 8, inspirée de Vitch Uploader pour PHP.

## Fonctionnalités

- ✅ Upload de fichiers avec validation
- ✅ Téléchargement de fichiers
- ✅ Déplacement et suppression de fichiers
- ✅ Gestion des métadonnées
- ✅ Validation de sécurité (taille, extension, type MIME)
- ✅ Génération de noms de fichiers sécurisés
- ✅ Support multiple des providers de stockage
- ✅ Configuration flexible via appsettings.json

## Configuration Avancée

### Options de FileManager

| Propriété | Description | Défaut |
|-----------|-------------|---------|
| `DefaultStorageProvider` | Provider de stockage par défaut | `LocalFileSystem` |
| `DefaultMaxFileSize` | Taille maximale des fichiers (bytes) | `10485760` (10MB) |
| `DefaultAllowedExtensions` | Extensions autorisées | `[".jpg", ".jpeg", ".png", ".gif", ".pdf"]` |
| `BasePath` | Chemin de base pour les uploads | `"uploads"` |
| `UseSecureNaming` | Générer des noms sécurisés | `true` |
| `FileNameLength` | Longueur des noms sécurisés | `32` |
| `CreateDirectoryIfNotExists` | Créer les répertoires automatiquement | `true` |

### Options LocalFileSystem

| Propriété | Description | Défaut |
|-----------|-------------|---------|
| `RootPath` | Chemin racine pour le stockage | `"wwwroot/uploads"` |
| `UseWebRoot` | Utiliser le wwwroot de l'application | `true` |
| `TempPath` | Dossier temporaire | `"temp"` |
 
## Configurer les services

```C#
var builder = WebApplication.CreateBuilder(args);

// Ajouter FileManager
builder.Services.AddFileManager(builder.Configuration);

var app = builder.Build();
```

## Configuration (appsettings.json)

```JSON
{
  "FileManager": {
    "DefaultStorageProvider": "LocalFileSystem",
    "DefaultMaxFileSize": 10485760,
    "DefaultAllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx"],
    "BasePath": "uploads",
    "UseSecureNaming": true,
    "FileNameLength": 32,
    "CreateDirectoryIfNotExists": true
  },
  "FileManager:LocalFileSystem": {
    "RootPath": "wwwroot/uploads",
    "UseWebRoot": true,
    "TempPath": "temp"
  }
}
```

## Utilisation Rapide

```C#
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileManager _fileManager;

    public FilesController(IFileManager fileManager)
    {
        _fileManager = fileManager;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string destinationPath = "uploads")
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        await using var stream = file.OpenReadStream();
        
        var request = new FileUploadRequest
        {
            Content = stream,
            FileName = file.FileName,
            DestinationPath = destinationPath,
            OverwriteIfExists = false,
            MaxFileSize = 10 * 1024 * 1024, // 10MB
            AllowedExtensions = new[] { ".jpg", ".png", ".pdf" }
        };

        var result = await _fileManager.UploadAsync(request);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(new { 
            success = true, 
            filePath = result.FilePath,
            fileName = result.FileName,
            fileSize = result.FileSize
        });
    }

    [HttpGet("download/{*filePath}")]
    public async Task<IActionResult> Download(string filePath)
    {
        try
        {
            var stream = await _fileManager.DownloadAsync(filePath);
            var metadata = await _fileManager.GetMetadataAsync(filePath);
            
            return File(stream, metadata.ContentType, metadata.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{*filePath}")]
    public async Task<IActionResult> Delete(string filePath)
    {
        var request = new FileDeleteRequest { FilePath = filePath };
        var result = await _fileManager.DeleteAsync(request);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
```

## Upload de fichier

```c#
var request = new FileUploadRequest
{
    Content = stream,
    FileName = "document.pdf",
    DestinationPath = "documents",
    OverwriteIfExists = false,
    MaxFileSize = 5 * 1024 * 1024, // 5MB
    AllowedExtensions = new[] { ".pdf", ".doc", ".docx" }
};

var result = await _fileManager.UploadAsync(request);
```

## Téléchargement de fichier

```c#
var stream = await _fileManager.DownloadAsync("documents/file.pdf");
var metadata = await _fileManager.GetMetadataAsync("documents/file.pdf");
```

## Déplacement & Suppression de fichier

```c#
//Déplacement
var moveRequest = new FileMoveRequest
{
    SourcePath = "temp/file.pdf",
    DestinationPath = "documents/file.pdf",
    OverwriteIfExists = true
};

var result = await _fileManager.MoveAsync(moveRequest);

//Suppression
var deleteRequest = new FileDeleteRequest 
{ 
    FilePath = "documents/file.pdf",
    Permanent = true
};

var result = await _fileManager.DeleteAsync(deleteRequest);
```