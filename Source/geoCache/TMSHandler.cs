using System;
using System.IO;
using System.Web;
using GeoCache.Core;

namespace GeoCache
{
    public class TmsHandler : IHttpHandler
    {
        static TmsHandler()
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

                    string fileName = Helpers.GetTileCacheFileName(layer, x, y, z, "png");
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

    
 

    }
}
