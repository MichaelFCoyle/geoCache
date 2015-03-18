using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using GeoCache.Core;
using GeoCache.Extensions.Base;

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

            //if (args.Count() != 3)
            //    return;
            int threads, iocp = 0;
            ThreadPool.GetMaxThreads(out threads, out iocp);

            s = new Semaphore(iocp, iocp);  

            osmLayer = new OsmLayer(20)
            {
                Name = "geobc",
                BBox = new BBox("-20037508.3428,-20037508.3428,20037508.3428,20037508.3428"),
                Srs = "EPSG:3857",
                Size = new Size(256, 256),
                ExtentType = ExtentType.Strict,
                MapBBox = new BBox("-15691595.4222,5802989.4975,-12160095.8963,8880086.82382"),
                Url =new Uri("http://tncache.azurewebsites.net/Tiles")
            };

            osmLayer.Resolutions =osmLayer.BBox.GetResolutions(20,osmLayer.Size);

            for (int zoom = 1; zoom < 18; zoom++)
            {
                int numTiles = (int)Math.Pow(2, zoom) - 1;

                for (int x = 0; x < numTiles; x++)
                {
                    for (int y = 0; y < numTiles; y++)
                    {
                        Tile t = new Tile(osmLayer, x,y,zoom);
                        if (y == 700)
                            Debugger.Break();
                        if (!osmLayer.MapBBox.Contains(t.Bounds.MinX, t.Bounds.MinY))
                            continue;
                        s.WaitOne();
                        ThreadPool.QueueUserWorkItem(delegate { DoWork(t); });
                    }
                }
            }

            Console.WriteLine("Completed {0} tiles", tileCount);
        }

        static int tileCount = 0;
        
        static void DoWork(ITile tile)
        {
            Interlocked.Increment(ref tileCount);
            Console.WriteLine("Retrieving {0}", tile.ToString());
            osmLayer.RenderTile(tile);
            s.Release();
            Console.WriteLine("Done  {0}", tile.ToString());
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
