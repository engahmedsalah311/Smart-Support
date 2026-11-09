namespace SmartSupport.Api.Services.FileStorage
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
        Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    }
}
