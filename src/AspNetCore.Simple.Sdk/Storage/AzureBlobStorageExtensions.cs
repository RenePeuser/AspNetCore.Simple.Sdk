using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Extensions.Pack;

namespace AspNetCore.Simple.Sdk.Storage
{
    public static class AzureBlobStorageExtensions
    {
        public static Task AddOrUpdateAsync(this BlobContainerClient blobContainerClient, string fileName, string content)
        {
            var inMemoryFile = new InMemoryFileAsByteArray(Encoding.UTF8.GetBytes(content), fileName);
            return blobContainerClient.AddOrUpdateAsync(inMemoryFile);
        }

        public static async Task<BlobClient> AddOrUpdateAsync(this BlobContainerClient blobContainerClient, InMemoryFileAsByteArray content)
        {
            var blob = blobContainerClient.GetBlobClient(content.Name);
            using var result = await blob.OpenWriteAsync(true).ConfigureAwait(false);
            await result.WriteAsync(content.FileContent).ConfigureAwait(false);
            await result.DisposeAsync().ConfigureAwait(false);
            return blob;
        }
    }
}
