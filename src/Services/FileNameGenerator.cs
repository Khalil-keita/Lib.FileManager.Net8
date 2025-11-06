using Lib.FileManager.Net8.Src.Configs;
using Microsoft.Extensions.Options;

namespace Lib.FileManager.Net8.src.Services
{
    public interface IFileNameGenerator
    {
        string GenerateSecureName(string originalFileName);
    }

    public class FileNameGenerator(IOptions<FileManagerOptions> options) : IFileNameGenerator
    {
        private readonly FileManagerOptions _options = options.Value;

        public string GenerateSecureName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);

            // Génération d'un nom sécurisé
            var secureName = $"{GenerateRandomString(_options.FileNameLength)}{extension}";

            return secureName;
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
        }
    }
}