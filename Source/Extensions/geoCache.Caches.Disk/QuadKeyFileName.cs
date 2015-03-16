//
// File: QuadKeyFileName.cs
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
using System.Text;
using System.IO;
using GeoCache.Core;

namespace GeoCache.Caches.Disk
{
	class QuadKeyFileName : XYFileName
	{
		public override IEnumerable<string> GetFileNames(ITile tile)
		{
			var names = new List<string>();
			var key = GetQuadKeyFileName(tile);
			if (!string.IsNullOrEmpty(key))
				names.Add(key);

			names.AddRange(base.GetFileNames(tile));
			return names;
		}
		private static string GetQuadKeyFileName(ITile tile)
		{
			try
			{
				var x = Convert.ToInt32(tile.X);
				var y = Convert.ToInt32(tile.Y);
				var quadKey = Microsoft.MapPoint.VirtualEarthTileSystem.TileXYToQuadKey(x, y, tile.Z);
				if (string.IsNullOrEmpty(quadKey))
					return string.Empty;
				return string.Join("/", new string[] {
					tile.Layer.Name,
					"QuadKey",
					tile.Z.ToString(),
					QuadKeyHelper.QuadKeyToFileName(quadKey, "." + tile.Layer.Extension)
				});
			}
			catch { return string.Empty; }
		}


		static class QuadKeyHelper
		{
			public static FileInfo GetQuadKeyFileInfo(string baseDir, string fileExtension, TileCacheFileInfo tileInfo)
			{
				var destFolder = Path.Combine(baseDir, tileInfo.Z.ToString());
				var quadKey = tileInfo.QuadKey;
				var destFileName = Path.Combine(destFolder, QuadKeyToFileName(quadKey, fileExtension));
				return new FileInfo(destFileName);
			}

			public static string QuadKeyToFileName(string quadKey, string fileExtension)
			{
				var sb = new StringBuilder();
				var quadKeyLength = quadKey.Length;
				int i = 0;
				while (true)
				{
					var length = Math.Min(3, quadKeyLength - i);
					sb.Append(quadKey.Substring(i, length));
					i += 3;
					if (i >= quadKeyLength)
						return sb + fileExtension;
					sb.Append("/");
				}
			}

#if DEBUG
			static void TestQuadKeyToFileName()
			{
				var s = "1234567890abcdefghijklmn";
				for (int i = 0; i < s.Length; i++)
				{
					string quadKey = s.Substring(i);
					string name = QuadKeyToFileName(quadKey, ".jpg");
					Console.WriteLine(name);

					string nameWithoutSeperators = name.Replace("\\", "").Replace("/", "");

					string expected = quadKey + ".jpg";
					if (nameWithoutSeperators != expected)
						throw new Exception("Expected " + expected + " Result: " + nameWithoutSeperators);
				}
			}
#endif
		}
	}
}
