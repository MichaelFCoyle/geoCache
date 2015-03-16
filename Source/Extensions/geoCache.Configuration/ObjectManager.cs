//
// File: ObjectManager.cs
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
using System.Collections.Generic;
using GeoCache.Core;

namespace GeoCache.Configuration
{
	internal static class ObjectManager
	{
		public static ILayer GetLayer(string id, IDictionary<string, object> config)
		{
			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			var loader = Resolver.Current;
			ThrowIfExtensionLoaderIsNull(loader);
			var layer = loader.Resolve<ILayer>(id, config);
			if (layer == null)
				throw new Exception("Failed to get layer of type " + id);
			return layer;
		}

		public static ICache GetCache(string id, IDictionary<string, object> config)		
		{
			var loader = Resolver.Current;
			ThrowIfExtensionLoaderIsNull(loader);
			return loader.Resolve<ICache>(id, config);
		}

		private static void ThrowIfExtensionLoaderIsNull(IResolver loader)
		{
			if (loader == null)
				throw new NotSupportedException("ExtensionLoader can not be null");
		}

	}
}
