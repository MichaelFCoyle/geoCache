using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GeoCache.Core;
using GeoCache.Extensions.Base;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Seed
{
    class Program
    {
        static OsmLayer osmLayer;

        /// <summary>
        /// Seed the tile cache
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            if (args.Length > 0 && args[0].ToLower().StartsWith("-h"))
            {
                Console.Write(@"
BlueToque TileCache Seed version {0}
Usage: seed.exe [-start=#] [-end=#] [-xstart=#] [-parallel=#] [-reverse=true|false]
where
    -start:    start zoom level
    -end:      end zoom level
    -xstart:   the x tile to start at
    -ystart:   the y stile to start at 
    -parallel: the maximum degree of parallelism
    -reverse:  reverse the iteration on zoom levels
");
                return;
            }

            GetParameters(args);

            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=tntilecache;AccountKey=vskhSwFKN1z4hZ+LOowQbtruPLeaUZoRMoYfPYJstty+mnNbwZwEbi5T97jRUnWRFufPVE3GZPHsZaRFzUgooQ==";

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


            if (m_reverse)
            {
                for (int zoom = m_zoomEnd; zoom >= m_zoomStart; zoom--)
                    DoZoom(m_xStart, zoom);
            }
            else
            {
                for (int zoom = m_zoomStart; zoom <= m_zoomEnd; zoom++)
                    DoZoom(m_xStart, zoom);
            }
            Console.WriteLine("Completed");
        }

        static int m_zoomStart = 1;
        static int m_zoomEnd = 18;
        static int m_xStart = 0;
        static bool m_reverse = false;
        static int m_parallelism = 10;

        private static void GetParameters(string[] args)
        {
            GetParam(args, "-start", out m_zoomStart, 1);
            GetParam(args, "-end", out m_zoomEnd, 18);
            GetParam(args, "-xstart", out m_xStart, 0);
            GetParam(args, "-parallel", out m_parallelism, 10);

            string arf = args.FirstOrDefault(x => x.StartsWith("-reverse"));
            if (!string.IsNullOrEmpty(arf))
            {
                string[] v = arf.Split('=');
                Boolean.TryParse(v[1], out m_reverse);
            }
        }

        private static void GetParam(string[] args, string tag, out int val, int def)
        {
            string arf = args.FirstOrDefault(x => x.ToLower().StartsWith(tag));
            if (string.IsNullOrEmpty(arf))
            {
                val = def;
                return;
            }
            string[] v = arf.Split('=');
            Int32.TryParse(v[1], out val);
        }

        private static void DoZoom(int xStart, int zoom)
        {
            int numTiles = (int)Math.Pow(2, zoom) - 1;
            Console.WriteLine("Zoom {0}, {1} tiles", zoom, numTiles);

            for (int x = xStart; x < numTiles; x++)
            {
                Parallel.For(
                    0,
                    numTiles,
                    new ParallelOptions { MaxDegreeOfParallelism = m_parallelism },
                    y =>
                    {
                        DoWork(new Tile(osmLayer, x, y, zoom));
                    });

            }
        }

        static int m_threads = 0;
        public static void DoWork(ITile tile)
        {
            try
            {
                Interlocked.Increment(ref m_threads);
                if (!osmLayer.MapBBox.Contains(tile.Bounds.MinX, tile.Bounds.MinY))
                    return;
                if (Exists(tile))
                    return;

                Console.WriteLine("Threads {0}: Retrieve {1}", m_threads, tile.ToString());
                Uri uri = new Uri(string.Format("{0}/{1}/{2}/{3}/{4}.png", osmLayer.Url, osmLayer.Name, tile.Z, tile.X, tile.Y));
                int retryCount = 3;
                while (retryCount-- > 0)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                            wc.DownloadData(uri);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error downloading {0} on try {1}\r\n{2}", uri, 3 - retryCount, ex.Message);
                    }
                }
                if (retryCount == 0)
                    Console.WriteLine("Failed {0}", tile.ToString());
                else
                    Console.WriteLine("Done {0}", tile.ToString());
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error on {0}:\r\n{1}", tile, ex);
                Console.WriteLine("Error on {0}:\r\n{1}", tile, ex);
            }
            finally
            {
                Interlocked.Decrement(ref m_threads);
            }
        }

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
