using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using GeoCache.Core;
using GeoCache.Extensions.Base;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Seed
{
    class Program
    {
        static Semaphore s;
        static OsmLayer osmLayer;

        /// <summary>
        /// Seed the tile cache
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=tntilecache;AccountKey=vskhSwFKN1z4hZ+LOowQbtruPLeaUZoRMoYfPYJstty+mnNbwZwEbi5T97jRUnWRFufPVE3GZPHsZaRFzUgooQ==";

            s = new Semaphore(40, 40);

            osmLayer = new OsmLayer(20)
            {
                Name = "geobc",
                BBox = new BBox("-20037508.3428,-20037508.3428,20037508.3428,20037508.3428"),
                Srs = "EPSG:3857",
                Size = new Size(256, 256),
                ExtentType = ExtentType.Strict,
                MapBBox = new BBox("-15691595.4222,5802989.4975,-12160095.8963,8880086.82382"),
                Url = new Uri("http://tncache.azurewebsites.net/Tiles")
            };

            osmLayer.Resolutions = osmLayer.BBox.GetResolutions(20, osmLayer.Size);

            int zoomStart = 1;
            int xStart = 0;

            for (int zoom = zoomStart; zoom < 18; zoom++)
            {
                int numTiles = (int)Math.Pow(2, zoom) - 1;

                for (int x = xStart; x < numTiles; x++)
                {
                    for (int y = 0; y < numTiles; y++)
                    {
                        s.WaitOne();
                        Tile t = new Tile(osmLayer, x, y, zoom);
                        ThreadPool.QueueUserWorkItem(delegate { DoWork(t); });
                    }
                }
            }

            Console.WriteLine("Completed {0} tiles", tileCount);
        }

        public static void DoWork(ITile tile)
        {
            try
            {
                if (!osmLayer.MapBBox.Contains(tile.Bounds.MinX, tile.Bounds.MinY))
                    return;
                Console.WriteLine("{0}", tile);
                if (Exists(tile))
                    return;

                Interlocked.Increment(ref tileCount);
                Console.WriteLine("Retrieving {0}", tile.ToString());
                
                Uri uri = new Uri(string.Format("{0}/{1}/{2}/{3}/{4}.png", osmLayer.Url, osmLayer.Name, tile.Z, tile.X, tile.Y));

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                    wc.DownloadDataAsync(uri, tile);
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Error on {0}:\r\n{1}", tile, ex);
                Console.WriteLine("Error on {0}:\r\n{1}", tile, ex);
            }
            finally
            {
                s.Release();
            }
        }

        static int tileCount = 0;

        static string ConnectionString { get; set; }

        static CloudStorageAccount StorageAccount { get; set; }

        static CloudBlobClient Client { get; set; }

        static CloudBlobContainer Container { get; set; }

        static CloudBlobContainer GetContainer(string containerName)
        {
            if (StorageAccount == null)
                StorageAccount = CloudStorageAccount.Parse(ConnectionString);

            if (Client == null)
                Client = StorageAccount.CreateCloudBlobClient();

            if (Container == null)
                Container = Client.GetContainerReference(containerName);

            return Container;
        }

        static bool Exists(ITile tile)
        {
            CloudBlobContainer container = GetContainer(tile.Layer.Name);
            string name = GetFileName(tile);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);
            return blockBlob.Exists();
        }

        static string GetFileName(ITile tile)
        {
            return string.Join("/", new[]
        	{
        		tile.Layer.Name,
        		string.Format("{0:00}", tile.Z),
        		string.Format("{0:000}", Convert.ToInt32(tile.X / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(tile.X) / 1000) % 1000),
        		string.Format("{0:000}", (Convert.ToInt32(tile.X) % 1000)),
        		string.Format("{0:000}", Convert.ToInt32(tile.Y / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(tile.Y / 1000) % 1000)),
        		string.Format("{0:000}.{1}", (Convert.ToInt32(tile.Y) % 1000), tile.Layer.Extension)
        	});
        }

        static void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
                Console.WriteLine("Error:\r\n{0}", e.Error);
            if (e.UserState == null)
                return;
            Tile t = e.UserState as Tile;
            if (t == null)
                return;
            Console.WriteLine("Done {0}", t.ToString());
        }

        static void Test()
        {
            Tile testTile = new Tile(osmLayer, 5182, 21567, 15);
            byte[] b = osmLayer.RenderTile(testTile);

            using (var ms = new MemoryStream(b))
            {
                Image testImage = Image.FromStream(ms);
                testImage.Save("c:\\Temp\\x.png");
            }
            if (!osmLayer.MapBBox.Contains(testTile.Bounds.MinX, testTile.Bounds.MinY))
                return;

        }
    }
}
