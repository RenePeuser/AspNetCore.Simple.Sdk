using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ApplicationInsight;
using AspNetCore.Simple.Sdk.ErrorHandling;
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
        public static void AddAzureBlobStorageFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTelemetryClientAdapter(configuration);

            services.AddSingletonIfNotExists<IAzureBlobStorageFactory, AzureBlobStorageFactory>();
        }
    }

    public interface IAzureBlobStorageFactory
    {
        IAzureBlobStorage CreateFrom(string connectionString);
    }

    internal sealed class AzureBlobStorageFactory : IAzureBlobStorageFactory
    {
        private readonly ITelemetryClientAdapter _telemetryClientAdapter;
        private readonly ConcurrentDictionary<string, IAzureBlobStorage> _bloStorageClients = new();

        public AzureBlobStorageFactory(ITelemetryClientAdapter telemetryClientAdapter)
        {
            _telemetryClientAdapter = telemetryClientAdapter;
        }

        public IAzureBlobStorage CreateFrom(string connectionString)
        {
            return _bloStorageClients.GetOrAdd(connectionString, key => new AzureBlobStorage(new StorageSettings { ConnectionString = key }, _telemetryClientAdapter));
        }
    }


    public interface IAzureBlobStorage
    {
        Task<BlobClient> AddOrUpdateBlobAsync(string containerName,
                                              string fileName,
                                              string content,
                                              CancellationToken cancellationToken = default);

        Task<BlobClient> AddOrUpdateBlobAsync(string containerName,
                                              InMemoryFileAsByteArray inMemoryFileAsByteArray,
                                              CancellationToken cancellationToken = default);

        Task<BlobClient?> GetBlobClientAsync(string containerName,
                                             string fileName,
                                             CancellationToken cancellationToken = default);

        Task<T?> GetFromJsonAsync<T>(string containerName, string fileName, CancellationToken cancellationToken = default);

        Task DeleteBlobAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<BlobContainerClient> GetAllContainerAsync(CancellationToken cancellationToken = default);

        Task<BlobContainerClient> GetOrAddContainerAsync(string containerName, CancellationToken cancellationToken = default);

        Task<BlobContainerClient?> FirstOrDefaultAsync(string containerName, CancellationToken cancellationToken = default);

        Task DeleteAsync(string containerName, CancellationToken cancellationToken = default);

        Task DeleteAsync(BlobContainerClient container, CancellationToken cancellationToken = default);
    }

    internal sealed class AzureBlobStorage : IAzureBlobStorage
    {
        private readonly StorageSettings _storageSettings;
        private readonly ITelemetryClientAdapter _telemetryClientAdapter;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorage(StorageSettings storageSettings,
                                ITelemetryClientAdapter telemetryClientAdapter)
        {
            _storageSettings = storageSettings;
            _telemetryClientAdapter = telemetryClientAdapter;
            _blobServiceClient = new BlobServiceClient(storageSettings.ConnectionString);
        }

        public Task<BlobClient> AddOrUpdateBlobAsync(string containerName,
                                                     string fileName,
                                                     string content,
                                                     CancellationToken cancellationToken = default)
        {
            var inMemoryFile = new InMemoryFileAsByteArray(Encoding.UTF8.GetBytes(content), fileName);
            return AddOrUpdateBlobAsync(containerName, inMemoryFile, cancellationToken);
        }

        public async Task<BlobClient> AddOrUpdateBlobAsync(string containerName, InMemoryFileAsByteArray inMemoryFileAsByteArray, CancellationToken cancellationToken = default)
        {
            var container = await GetOrAddContainerAsync(containerName, cancellationToken).ConfigureAwait(false);
            return await container.AddOrUpdateAsync(inMemoryFileAsByteArray).ConfigureAwait(false);
        }

        public async Task<BlobClient?> GetBlobClientAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
        {
            var container = await FirstOrDefaultAsync(containerName, cancellationToken).ConfigureAwait(false);
            if (container.IsNull())
            {
                return null;
            }

            return container.GetBlobClient(fileName);
        }

        public async Task<T?> GetFromJsonAsync<T>(string containerName, string fileName, CancellationToken cancellationToken = default)
        {
            var container = await FirstOrDefaultAsync(containerName, cancellationToken).ConfigureAwait(false);
            if (container.IsNull())
            {
                return default;
            }

            var blobClient = container.GetBlobClient(fileName);
            var exists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
            if (exists.HasValue && exists.Value.IsFalse())
            {
                return default;
            }

            var fileContent = await blobClient.DownloadContentAsync(cancellationToken).ConfigureAwait(false);
            if (fileContent.HasValue.IsFalse())
            {
                return default;
            }

            try
            {
                return fileContent.Value.Content.ToObjectFromJson<T>();
            }
            catch (Exception e)
            {
                _telemetryClientAdapter.TrackException(e,
                    ("Container", containerName),
                    ("File", fileName),
                    ("Json", fileContent.Value.Content.ToString()));
                return default;
            }
        }

        public async Task DeleteBlobAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
        {
            var container = await FirstOrDefaultAsync(containerName, cancellationToken).ConfigureAwait(false);
            if (container.IsNull())
            {
                var allContainers = await GetAllContainerAsync(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
                throw new ProblemDetailsException("Could not delete expected file because the storage container for does not exists",
                    $"The container: '{containerName}' which should contains the file: '{fileName}' does not exists",
                    ("Available Containers", allContainers.Select(c => c.Name).ToJson()));
            }
        }

        public IAsyncEnumerable<BlobContainerClient> GetAllContainerAsync(CancellationToken cancellationToken = default)
        {
            return GetAllAsync(cancellationToken);
        }

        public async Task<BlobContainerClient> GetOrAddContainerAsync(string containerName, CancellationToken cancellationToken = default)
        {
            var container = await FirstOrDefaultAsync(containerName, cancellationToken).ConfigureAwait(false) ??
                            await _blobServiceClient.CreateBlobContainerAsync(containerName, cancellationToken: cancellationToken).ConfigureAwait(false);

            return container;
        }

        public async IAsyncEnumerable<BlobContainerClient> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var blobServiceClient = new BlobServiceClient(_storageSettings.ConnectionString);
            var containers = blobServiceClient.GetBlobContainersAsync(cancellationToken: cancellationToken);
            var asyncEnumerator = containers.GetAsyncEnumerator(cancellationToken);
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

        public async Task<BlobContainerClient?> FirstOrDefaultAsync(string containerName, CancellationToken cancellationToken = default)
        {
            var blobServiceClient = new BlobServiceClient(_storageSettings.ConnectionString);
            var containers = blobServiceClient.GetBlobContainersAsync(cancellationToken: cancellationToken);
            var asyncEnumerator = containers.GetAsyncEnumerator(cancellationToken);
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

        public Task DeleteAsync(string containerName, CancellationToken cancellationToken = default)
        {
            return _blobServiceClient.DeleteBlobContainerAsync(containerName, cancellationToken: cancellationToken);
        }

        public Task DeleteAsync(BlobContainerClient container, CancellationToken cancellationToken = default)
        {
            return _blobServiceClient.DeleteBlobContainerAsync(container.Name, cancellationToken: cancellationToken);
        }
    }
}
