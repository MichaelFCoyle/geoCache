using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GeoCache.Core;
using GeoCache.Core.Web;
using GeoCache.Web;

namespace GeoCache
{
    public class TMSHandler : IHttpHandler
    {
        static TMSHandler()
        {
            Resolver.Current = new UnityAddInExtensionLoader();
        }

        #region IHttpHandler Members
        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            try
            {
                string[] parts = request.FilePath.Split('/');
                if (parts.Length != 5)
                {
                    response.Status = "Image Not Found";
                    response.StatusCode = 404;
                }
                else
                {
                    string layer = parts[1];
                    int x, y, z;
                    Int32.TryParse(parts[2], out x);
                    Int32.TryParse(parts[3], out y);
                    Int32.TryParse(parts[4], out z);

                    string fileName = GetTileCacheFileName(layer, x, y, z, "png");
                    fileName = Path.Combine("D://Temp//GeoCache//", fileName);

                    if (!File.Exists(fileName))
                    {
                        response.Status = "Image Not Found";
                        response.StatusCode = 404;
                    }
                    else
                    {
                        byte[] bytes = File.ReadAllBytes(fileName);
                        response.ContentType = "image/png";
                        response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = string.Format("Error retrieving tile:\r\n{0}", ex.Message);
                response.StatusCode = 500;
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
        #endregion

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
