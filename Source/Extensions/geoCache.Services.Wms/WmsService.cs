//
// File: WmsService.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2006-2007 MetaCarta, Inc.
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using GeoCache.Core;
using GeoCache.Core.Web;

namespace GeoCache.Services.Wms
{
	class WmsService : ServiceRequest, IService
	{
		public WmsService() { }

		public WmsService(ITileRenderer tileRenderer, ILayerContainer layerContainer)
			: base(tileRenderer, layerContainer)
		{
		}

		#region python
		/*
		def parse (self, fields, path, host):
			param = {}
			for key in ['bbox', 'layers', 'request', 'version']: 
				if fields.has_key(key.upper()):
					param[key] = fields[key.upper()] 
				elif fields.has_key(key):
					param[key] = fields[key]
				else:
					param[key] = ""
			if param["request"] == "GetCapabilities":
				return self.getCapabilities(host + path, param)
			else:
				return self.getMap(param)
		 */
		#endregion
		public void ProcessRequest(IHttpContext context)
		{
			var requestParams = context.Request.Params;
			//NameValueCollection requestParams, string pathInfo, string host
			if ("GetCapabilities".Equals(requestParams["request"], StringComparison.OrdinalIgnoreCase))
			{
				//TODO: Get host and pathInfo
				var host = "dummy-host";
				var pathInfo = "dummy-path-info";
				var capabilities = GetCapabilities(host + pathInfo);
				context.Response.ContentType = capabilities.Format;
				context.Response.Write(capabilities.Data);
				return;
			}

			var forceParam = requestParams["force"];
			bool force = !string.IsNullOrEmpty(forceParam) && forceParam == "true";

			TileRenderer.RenderTile(context.Response, GetMap(requestParams), force);
			//return this.GetMap(requestParams);
		}

		#region python
		/*
    def getMap (self, param):
        bbox  = map(float, param["bbox"].split(","))
        layer = self.getLayer(param["layers"])
        tile  = layer.getTile(bbox)
        if not tile:
            raise Exception(
                "couldn't calculate tile index for layer %s from (%s)"
                % (layer.name, bbox))
        return tile
		 */
		#endregion
		ITile GetMap(NameValueCollection param)
		{
			var bbox = new BBox(param["bbox"]);
			var layer = GetLayer(param["layers"]);
			var tile = layer.GetTile(bbox);
			if (tile == null)
				throw new Exception(string.Format("couldn't calculate tile index for layer {0} from ({1})", layer.Name, bbox));
			return tile;
		}
		#region python
		/*
		 */
		#endregion
		internal static string GetHost(string host)
		{
			if (string.IsNullOrEmpty(host))
				throw new ArgumentNullException("host");

			if(!host.EndsWith("&") && !host.EndsWith("?"))
				return host;

			if(host.Contains("?"))
				return host + "&";
			return host + "?";

			#region Original python-code
			//if host[-1] not in "?&":
			//   if "?" in host:
			//       host += "&"
			//   else:
			//       host += "?"
			#endregion
		}

		#region python
		/*
		 */
		#endregion
		internal Capabilities GetCapabilities(string host)
		{
			host = GetHost(host);

			var formats = new List<string>();
			foreach (var layer in LayerContainer.Layers)
			{
				var format = layer.Value.Format;
				if (!formats.Contains(format))
					formats.Add(format);
			}
			//base.Response.ContentType = "text/xml";
			return GetCapabilities(host, Description, formats);
		}

		public string Description { get; set; }

		#region python
		/*
		 */
		#endregion
		internal static Capabilities GetCapabilities(string host, string description, IEnumerable<string> formats)
		{
			var xml = string.Format(
			@"<?xml version='1.0' encoding=""ISO-8859-1"" standalone=""no"" ?>
			<!DOCTYPE WMT_MS_Capabilities SYSTEM 
				""http://schemas.opengeospatial.net/wms/1.1.1/WMS_MS_Capabilities.dtd"" [
				  <!ELEMENT VendorSpecificCapabilities (TileSet*) >
				  <!ELEMENT TileSet (SRS, BoundingBox?, Resolutions,
									 Width, Height, Format, Layers*, Styles*) >
				  <!ELEMENT Resolutions (#PCDATA) >
				  <!ELEMENT Width (#PCDATA) >
				  <!ELEMENT Height (#PCDATA) >
				  <!ELEMENT Layers (#PCDATA) >
				  <!ELEMENT Styles (#PCDATA) >
			]> 
			<WMT_MS_Capabilities version=""1.1.1"">
			  <Service>
				<Name>OGC:WMS</Name>
				<Title>{0}</Title>
				<OnlineResource xmlns:xlink=""http://www.w3.org/1999/xlink"" xlink:href=""{1}""/>
			  </Service>
			", description, host);

			xml += string.Format(@"
			  <Capability>
				<Request>
				  <GetCapabilities>
					<Format>application/vnd.ogc.wms_xml</Format>
					<DCPType>
					  <HTTP>
						<Get><OnlineResource xmlns:xlink=""http://www.w3.org/1999/xlink"" xlink:href=""{0}""/></Get>
					  </HTTP>
					</DCPType>
				  </GetCapabilities>", host);

			xml += @"
              <GetMap>";
			
			foreach (var format in formats)
			{
				xml += string.Format("\n<Format>{0}</Format>\n", format);
			}
			xml += string.Format(@"
                <DCPType>
                  <HTTP>
                    <Get><OnlineResource xmlns:xlink=""http://www.w3.org/1999/xlink"" xlink:href=""{0}""/></Get>
                  </HTTP>
                </DCPType>
              </GetMap>
            </Request>""", host);
			
			xml += @"
            <Exception><Format>text/plain</Format></Exception>";
			/*
            xml += "<VendorSpecificCapabilities>";
			 
			for name, layer in self.service.layers.items():
				resolutions = " ".join(["%.9f" % r for r in layer.resolutions])
				xml += """
				  <TileSet>
					<SRS>%s</SRS>
					<BoundingBox SRS="%s" minx="%f" miny="%f"
										  maxx="%f" maxy="%f" />
					<Resolutions>%s</Resolutions>
					<Width>%d</Width>
					<Height>%d</Height>
					<Format>%s</Format>
					<Layers>%s</Layers>
					<Styles></Styles>
				  </TileSet>""" % (
					layer.srs, layer.srs, layer.bbox[0], layer.bbox[1],
					layer.bbox[2], layer.bbox[3], resolutions, layer.size[0],
					layer.size[1], layer.format(), name )
			 
			xml += "</VendorSpecificCapabilities>"
			*/
			
			xml += @"
				<UserDefinedSymbolization SupportSLD=""0"" UserLayer=""0""
                                      UserStyle=""0"" RemoteWFS=""0""/>";
			/*
			xml += @"
			<Layer>";
			for name, layer in self.service.layers.items():
				xml += """
				<Layer queryable="0" opaque="0" cascaded="1">
				  <Name>%s</Name>
				  <Title>%s</Title>
				  <SRS>%s</SRS>
				  <BoundingBox srs="%s" minx="%f" miny="%f"
										maxx="%f" maxy="%f" />
				</Layer>""" % (
					name, layer.name, layer.srs, layer.srs,
					layer.bbox[0], layer.bbox[1], layer.bbox[2], layer.bbox[3])

			xml += @"
			</Layer>";
			 */
			xml += @"
			  </Capability>
			</WMT_MS_Capabilities>";
			
			return new Capabilities("text/xml", xml);
		}
	}
}
