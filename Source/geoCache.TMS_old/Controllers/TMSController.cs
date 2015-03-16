using System;
using System.IO;
using System.Web.Mvc;
using geoCache.TMS.Properties;

namespace geoCache.TMS.Controllers
{
    public class TMSController : Controller
    {
        //
        // GET: /Home/x/y/z
        public ActionResult Tile(string layer, int x, int y, int z)
        {
            string path = Path.Combine(Settings.Default.Root, layer);
            path = GetTileCacheFileName(layer, x, y, z, "png");
            return File(path,"image/png");

            if (System.IO.File.Exists(path))
                return File(path, "image/png");
            else
                Redirect("");
        }

        private static string GetTileCacheFileName(string layer, int x, int y, int z, string ext)
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

    }
}
