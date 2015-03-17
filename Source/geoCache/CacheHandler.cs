//
// File: CacheHandler.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;
using GeoCache.Core;
using GeoCache.Core.Web;
using GeoCache.Web;

namespace GeoCache
{
	/// <summary>
	/// Summary description for CacheHandler
	/// </summary>
	public class CacheHandler : IHttpHandler
	{
		static CacheHandler()
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

		#region python dispatchRequest
		/*
		 * ProcessRequest loosely based on this code from TileCache
    def dispatchRequest (self, params, path_info="/", req_method="GET", host="http://example.com/"):
        if self.metadata.has_key('exception'):
            raise TileCacheException("%s\n%s" % (self.metadata['exception'], self.metadata['traceback']))
        
        if path_info.split(".")[-1] == "kml":
            from TileCache.Services.KML import KML 
            return KML(self).parse(params, path_info, host)
            raise TileCacheException("What, you think we do KML?")
        
        if params.has_key("scale") or params.has_key("SCALE"): 
            from TileCache.Services.WMTS import WMTS
            tile = WMTS(self).parse(params, path_info, host)
        elif params.has_key("service") or params.has_key("SERVICE") or \
           params.has_key("REQUEST") and params['REQUEST'] == "GetMap" or \
           params.has_key("request") and params['request'] == "GetMap": 
            from TileCache.Services.WMS import WMS
            tile = WMS(self).parse(params, path_info, host)
        elif params.has_key("L") or params.has_key("l") or \
             params.has_key("request") and params['request'] == "metadata":
            from TileCache.Services.WorldWind import WorldWind
            tile = WorldWind(self).parse(params, path_info, host)
        elif params.has_key("interface"):
            from TileCache.Services.TileService import TileService
            tile = TileService(self).parse(params, path_info, host)
        elif params.has_key("v") and \
             (params['v'] == "mgm" or params['v'] == "mgmaps"):
            from TileCache.Services.MGMaps import MGMaps 
            tile = MGMaps(self).parse(params, path_info, host)
        else:
            from TileCache.Services.TMS import TMS
            tile = TMS(self).parse(params, path_info, host)
        
        if isinstance(tile, Layer.Tile):
            if req_method == 'DELETE':
                self.expireTile(tile)
                return ('text/plain', 'OK')
            else:
                return self.renderTile(tile, params.has_key('FORCE'))
        else:
            return (tile.format, tile.data)
		 */
		#endregion
		public void ProcessRequest(IHttpContext context)
		{
			NameValueCollection requestParams = context.Request.Params;

			try
			{
				string request = requestParams["request"];
				if (requestParams["service"] != null && request != null
				    && (request.Equals("GetMap") || request.Equals("GetCapabilities")))
				{
					IService service = ObjectManager.GetService("wms", null);
					service.ProcessRequest(context);
				}
			}
			catch (Exception ex)
			{
				HttpContext.Current.Response.Write("Exception occured:\n " + ex);
                Trace.TraceError("Exception:\r\n{0}", ex);
			}
		}
	}
}

//<%@ WebHandler Language="C#" CodeBehind="Handler1.ashx.cs" Class="GeoPort.Handler1" %>