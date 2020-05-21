using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GeoCache
{
    static class Helpers
    {
        public static byte[] ToBytes(this Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static byte[] Create(int width, int height)
        {
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage((Image)bitmap))
                {
                    g.Clear(Color.Transparent);
                    //g.FillRectangle(Brushes.Red, 0f, 0f, bitmap.Width, bitmap.Height);
                }

                using (MemoryStream mem = new MemoryStream())
                {
                    bitmap.Save(mem, ImageFormat.Png);
                    return mem.ToArray();
                }
            }
        }

        public static string GetTileCacheFileName(string layer, int x, int y, int z, string ext)
        {
            return string.Join("/", new[]
        	{
        		layer,
        		string.Format("{0:00}", z),
        		string.Format("{0:000}", Convert.ToInt32(x / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(x) / 1000) % 1000),
        		string.Format("{0:000}", (Convert.ToInt32(x) % 1000)),
        		string.Format("{0:000}", Convert.ToInt32(y / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(y/ 1000) % 1000)),
        		string.Format("{0:000}.{1}", (Convert.ToInt32(y) % 1000), ext)
        	});
        }

        /// <summary>
        /// http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static PointF WorldToTilePos(double lon, double lat, int zoom)
        {
            PointF p = new PointF
            {
                X = (float)((lon + 180.0) / 360.0 * (1 << zoom)),
                Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom))
            };

            return p;
        }

        /// <summary>
        /// http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        /// </summary>
        /// <param name="tile_x"></param>
        /// <param name="tile_y"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static PointF TileToWorldPos(double tile_x, double tile_y, int zoom)
        {
            PointF p = new Point();
            double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

            p.X = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return p;
        }
    }
}
