namespace BvadGroupApi.Services
{
    public interface IFileStorageService
    {
        Task<StoredFile> SaveAsync(IFormFile file, string subFolder);
        Task<byte[]?> ReadAsync(string relativePath);
        Task<bool> DeleteAsync(string relativePath);
        string GetFullPath(string relativePath);
    }

    public record StoredFile(
        string RelativePath,
        string FileName,
        long Size,
        string ContentType
    );

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileStorageService> _logger;
        private const string RootFolder = "Uploads";

        public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<StoredFile> SaveAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Fichier vide");

            var folder = Path.Combine(_env.ContentRootPath, RootFolder, subFolder);
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var safeName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, safeName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine(RootFolder, subFolder, safeName)
                .Replace('\\', '/');

            _logger.LogInformation("✅ Fichier stocké : {Path} ({Size} KB)",
                relativePath, file.Length / 1024);

            return new StoredFile(
                relativePath,
                file.FileName,
                file.Length,
                file.ContentType
            );
        }

        public async Task<byte[]?> ReadAsync(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);
            if (!File.Exists(fullPath)) return null;
            return await File.ReadAllBytesAsync(fullPath);
        }

        public Task<bool> DeleteAsync(string relativePath)
        {
            var fullPath = GetFullPath(relativePath);
            if (!File.Exists(fullPath)) return Task.FromResult(false);

            try
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur suppression fichier {Path}", relativePath);
                return Task.FromResult(false);
            }
        }

        public string GetFullPath(string relativePath)
        {
            return Path.Combine(_env.ContentRootPath, relativePath);
        }
    }
}