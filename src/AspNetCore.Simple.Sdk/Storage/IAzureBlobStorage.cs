using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Extensions;
using Azure.Storage.Blobs;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Storage
{
    public record StorageSettings
    {
        public string ConnectionString { get; init; } = string.Empty;
    }

    public static class AddAzureBlobStorageExtension
    {
        public static void AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.TryGetSettings<StorageSettings>(out var storageSettings))
            {
                services.AddSingletonIfNotExists(storageSettings);
                services.AddSingletonIfNotExists<IAzureBlobStorage, AzureBlobStorage>();
            }
        }
    }

    public interface IAzureBlobStorage
    {
        Task<BlobClient> AddOrUpdateBlobAsync(string containerName, string fileName, string content);

        Task<BlobClient> AddOrUpdateBlobAsync(string containerName, InMemoryFileAsByteArray inMemoryFileAsByteArray);

        IAsyncEnumerable<BlobContainerClient> GetAll();

        Task<BlobContainerClient> GetOrAddContainerAsync(string containerName);

        Task<BlobContainerClient?> FirstOrDefaultAsync(string containerName);

        Task DeleteAsync(string containerName);

        Task DeleteAsync(BlobContainerClient container);
    }

    internal class AzureBlobStorage : IAzureBlobStorage
    {
        private readonly StorageSettings _storageSettings;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorage(StorageSettings storageSettings)
        {
            _storageSettings = storageSettings;
            _blobServiceClient = new BlobServiceClient(storageSettings.ConnectionString);
        }

        public Task<BlobClient> AddOrUpdateBlobAsync(string containerName, string fileName, string content)
        {
            var inMemoryFile = new InMemoryFileAsByteArray(Encoding.UTF8.GetBytes(content), fileName);
            return AddOrUpdateBlobAsync(containerName, inMemoryFile);
        }

        public async Task<BlobClient> AddOrUpdateBlobAsync(string containerName, InMemoryFileAsByteArray inMemoryFileAsByteArray)
        {
            var container = await GetOrAddContainerAsync(containerName).ConfigureAwait(false);
            return await container.AddOrUpdateAsync(inMemoryFileAsByteArray).ConfigureAwait(false);
        }

        public IAsyncEnumerable<BlobContainerClient> GetAll()
        {
            return GetAllAsync();
        }

        public async Task<BlobContainerClient> GetOrAddContainerAsync(string containerName)
        {
            var container = await FirstOrDefaultAsync(containerName).ConfigureAwait(false) ??
                            await _blobServiceClient.CreateBlobContainerAsync(containerName).ConfigureAwait(false);

            return container;
        }

        public async IAsyncEnumerable<BlobContainerClient> GetAllAsync()
        {
            var blobServiceClient = new BlobServiceClient(_storageSettings.ConnectionString);
            var containers = blobServiceClient.GetBlobContainersAsync();
            var asyncEnumerator = containers.GetAsyncEnumerator();
            try
            {
                while (await asyncEnumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    var container = asyncEnumerator.Current;
                    yield return new BlobContainerClient(_storageSettings.ConnectionString, container.Name);
                }
            }
            finally
            {
                await asyncEnumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<BlobContainerClient?> FirstOrDefaultAsync(string containerName)
        {
            var blobServiceClient = new BlobServiceClient(_storageSettings.ConnectionString);
            var containers = blobServiceClient.GetBlobContainersAsync();
            var asyncEnumerator = containers.GetAsyncEnumerator();
            try
            {
                while (await asyncEnumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    var container = asyncEnumerator.Current;
                    if (container.Name == containerName)
                    {
                        return new BlobContainerClient(_storageSettings.ConnectionString, containerName);
                    }
                }
            }
            finally
            {
                await asyncEnumerator.DisposeAsync().ConfigureAwait(false);
            }

            return null;
        }

        public Task DeleteAsync(string containerName)
        {
            return _blobServiceClient.DeleteBlobContainerAsync(containerName);
        }

        public Task DeleteAsync(BlobContainerClient container)
        {
            return _blobServiceClient.DeleteBlobContainerAsync(container.Name);
        }
    }
}
