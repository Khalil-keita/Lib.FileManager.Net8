using Lib.FileManager.Net8.src.Services;
using Lib.FileManager.Net8.Src.Interfaces;
using Lib.FileManager.Net8.Src.Services;
using Microsoft.Extensions.DependencyInjection;  // Pour IServiceCollection
using Microsoft.Extensions.Configuration;

namespace Lib.FileManager.Net8.Src.Configs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileManager(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<FileManagerOptions>(configuration.GetSection(FileManagerOptions.SectionName));
        services.Configure<LocalFileSystemOptions>(configuration.GetSection(LocalFileSystemOptions.SectionName));

        // Services de base
        services.AddScoped<IFileValidator, FileValidator>();
        services.AddScoped<IFileNameGenerator, FileNameGenerator>();

        // Providers de stockage
        services.AddScoped<IStorageProvider, LocalFileSystemProvider>();

        // Service principal
        services.AddScoped<IFileManager, FileManagerService>();

        return services;
    }

    public static IServiceCollection AddFileManager<TProvider>(this IServiceCollection services, IConfiguration configuration)
        where TProvider : class, IStorageProvider
    {
        services.AddFileManager(configuration);
        services.AddScoped<IStorageProvider, TProvider>();
        return services;
    }
}