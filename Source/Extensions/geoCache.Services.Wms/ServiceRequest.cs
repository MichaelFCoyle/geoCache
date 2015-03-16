//
// File: ServiceRequest.cs
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
using GeoCache.Core;

namespace GeoCache.Services.Wms
{
	#region python
	/*
	class Request (object):
		def __init__ (self, service):
			self.service = service
		def getLayer(self, layername):    
			try:
				return self.service.layers[layername]
			except:
				raise TileCacheException("The requested layer (%s) does not exist. Available layers are: \n * %s" % (layername, "\n * ".join(self.service.layers.keys()))) 
	 */
	#endregion
	public class ServiceRequest
	{
		public ServiceRequest() { }

		public ServiceRequest(ITileRenderer tileRenderer, ILayerContainer layerContainer)
		{
			TileRenderer = tileRenderer;
			LayerContainer = layerContainer;
		}

		public ILayer GetLayer(string layerName)
		{
			if (string.IsNullOrEmpty(layerName))
				throw new ArgumentNullException("layerName");

			if(LayerContainer == null)
				throw new NotSupportedException("Unable to get layer when LayerContainer is null");

			if (!LayerContainer.Layers.ContainsKey(layerName))
				throw new Exception(string.Format("The requested layer ({0}) does not exist.", layerName)); //Available layers are: \n ", (layername, "\n * ".join(self.service.layers.keys()))) 

			return LayerContainer.Layers[layerName];
		}

		protected ITileRenderer TileRenderer { get; set; }
		protected ILayerContainer LayerContainer { get; set; }
	}
}
