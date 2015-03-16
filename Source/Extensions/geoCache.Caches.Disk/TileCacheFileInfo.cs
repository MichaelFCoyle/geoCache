//
// File: TileCacheFileInfo.cs
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
using System.IO;

namespace GeoCache.Caches.Disk
{
	public class TileCacheFileInfo
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public static TileCacheFileInfo Get(string partialFileName)
		{

			var a = partialFileName.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				var filenameWithoutExtension = Path.GetFileNameWithoutExtension(partialFileName);
				return new TileCacheFileInfo
				{
					Z = int.Parse(a[0]),
					X = int.Parse(a[1] + a[2] + a[3]),
					Y = int.Parse(a[4] + a[5] + filenameWithoutExtension),
				};
			}
			catch { }
			return null;
		}

		public string QuadKey
		{
			get { return Microsoft.MapPoint.VirtualEarthTileSystem.TileXYToQuadKey(X, Y, Z); }
		}
	}
}
