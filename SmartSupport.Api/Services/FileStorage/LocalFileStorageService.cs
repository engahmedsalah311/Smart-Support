using Microsoft.Extensions.Options;
using SmartSupport.Api.Options;

namespace SmartSupport.Api.Services.FileStorage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _documentsPath;

        public LocalFileStorageService(IWebHostEnvironment environment,IOptions<StorageOptions> options)
        {
            _documentsPath = Path.Combine(environment.ContentRootPath,options.Value.DocumentsPath);
        }

        public async Task<string> SaveAsync(Stream content,string fileName,CancellationToken cancellationToken = default)
        {
            if (Path.GetFileName(fileName) != fileName)
                throw new ArgumentException("Invalid file name.", nameof(fileName));

            Directory.CreateDirectory(_documentsPath);

            var filePath = Path.Combine(_documentsPath, fileName);

            await using var fileStream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await content.CopyToAsync(fileStream, cancellationToken);

            return fileName;
        }

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Path.GetFileName(storageKey) != storageKey)
                throw new ArgumentException("Invalid storage key.", nameof(storageKey));

            var filePath = Path.Combine(_documentsPath, storageKey);

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }


    }
}
