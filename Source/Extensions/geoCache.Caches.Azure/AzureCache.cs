using System;
using System.Diagnostics;
using GeoCache.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GeoCache.Caches.Azure
{
    public class AzureCache : ICache
    {
        public AzureCache() : this("") { }

        private AzureCache(string connectionString) => ConnectionString = connectionString;

        public string ConnectionString { get; set; }

        CloudStorageAccount StorageAccount { get; set; }

        CloudBlobClient Client { get; set; }

        public byte[] Set(ITile tile, byte[] data)
        {
            CloudBlobContainer container = GetContainer(tile.Layer.Name);
            string name = GetFileName(tile);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
            blockBlob.UploadFromByteArray(data, 0, data.Length);
            return data;
        }

        public void Lock(ITile tile) { }

        public bool Lock(ITile tile, bool blocking) => true;

        public byte[] Get(ITile tile)
        {
            try
            {
                CloudBlobContainer container = GetContainer(tile.Layer.Name);
                string name = GetFileName(tile);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
                if (blockBlob.Exists())
                    return new System.Net.WebClient().DownloadData(blockBlob.Uri.ToString());
                else
                    return null;
            }
            catch(Exception ex)
            {
                Trace.TraceError("AzureCache.Get: Error\r\n{0}", ex);
                return null;
            }
        }

        public void Unlock(ITile tile) { }

        private string GetFileName(ITile tile) => new XYFileName().GetFileNames(tile);

        private CloudBlobContainer GetContainer(string containerName)
        {
            if (StorageAccount == null)
                StorageAccount = CloudStorageAccount.Parse(ConnectionString);

            if (Client == null)
                Client = StorageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = Client.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            if (container.CreateIfNotExists())
            {
                container.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Blob
                    });
            }
            return container;
        }
    }
}
