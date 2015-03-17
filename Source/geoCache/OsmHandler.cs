using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class OsmHandler : IHttpHandler
    {
        static OsmHandler()
        {
            if (Resolver.Current == null)
                Resolver.Current = new UnityAddInExtensionLoader();
        }

        #region IHttpHandler Members
        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new WebHttpContext(context));
        }

        public bool IsReusable
        {
            get { return false; }
        }
        #endregion

        public void ProcessRequest(IHttpContext context)
        {
            try
            {
                IService service = ObjectManager.GetService("tms", null);
                service.ProcessRequest(context);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("Exception occured:\n " + ex);
                Trace.TraceError("Exception:\r\n{0}", ex);
            }
        }
    }
}
