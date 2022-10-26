using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ErrorHandling;
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

    public static class AddAzureBlobStorageFactoryExtension
    {
        public static void AddAzureBlobStorageFactory(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IAzureBlobStorageFactory, AzureBlobStorageFactory>();
        }
    }

    public interface IAzureBlobStorageFactory
    {
        IAzureBlobStorage CreateFrom(string connectionString);
    }

    internal class AzureBlobStorageFactory : IAzureBlobStorageFactory
    {
        private readonly ConcurrentDictionary<string, IAzureBlobStorage> _bloStorageClients = new();

        public IAzureBlobStorage CreateFrom(string connectionString)
        {
            return _bloStorageClients.GetOrAdd(connectionString, key => new AzureBlobStorage(new StorageSettings { ConnectionString = key }));
        }
    }


    public interface IAzureBlobStorage
    {
        Task<BlobClient> AddOrUpdateBlobAsync(string containerName, string fileName, string content);

        Task<BlobClient> AddOrUpdateBlobAsync(string containerName, InMemoryFileAsByteArray inMemoryFileAsByteArray);

        Task<BlobClient> GetBlobAsync(string containerName, string fileName);

        Task DeleteBlobAsync(string containerName, string fileName);

        IAsyncEnumerable<BlobContainerClient> GetAllContainerAsync();

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

        public async Task<BlobClient?> GetBlobAsync(string containerName, string fileName)
        {
            var container = await FirstOrDefaultAsync(containerName).ConfigureAwait(false);
            if (container.IsNull())
            {
                return null;
            }

            return container.GetBlobClient(fileName);
        }

        public async Task DeleteBlobAsync(string containerName, string fileName)
        {
            var container = await FirstOrDefaultAsync(containerName).ConfigureAwait(false);
            if (container.IsNull())
            {
                var allContainers = await GetAllContainerAsync().ToListAsync().ConfigureAwait(false);
                throw new ProblemDetailsException("Could not delete expected file because the storage container for does not exists",
                                                  $"The container: '{containerName}' which should contains the file: '{fileName}' does not exists",
                                                  ("Available Containers", allContainers.Select(c => c.Name).ToJson()));
            }
        }

        public IAsyncEnumerable<BlobContainerClient> GetAllContainerAsync()
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
