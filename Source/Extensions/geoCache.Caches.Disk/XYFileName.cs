//
// File: XYFileName.cs
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

namespace GeoCache.Caches.Disk
{
	internal class XYFileName
	{
        public virtual IEnumerable<string> GetFileNames(ITile tile) => new[] { GetTileCacheFileName(tile) };

        #region python - getKey
        /*
    def getKey (self, tile):
        components = ( self.basedir,
                       tile.layer.name,
                       "%02d" % tile.z,
                       "%03d" % int(tile.x / 1000000),
                       "%03d" % (int(tile.x / 1000) % 1000),
                       "%03d" % (int(tile.x) % 1000),
                       "%03d" % int(tile.y / 1000000),
                       "%03d" % (int(tile.y / 1000) % 1000),
                       "%03d.%s" % (int(tile.y) % 1000, tile.layer.extension)
                    )
        filename = os.path.join( *components )
        return filename
        */
        #endregion

        private static string GetTileCacheFileName(ITile tile)
		{
			return string.Join("/", new[]
        	{
        		tile.Layer.Name,
        		string.Format("{0:00}", tile.Z),
        		string.Format("{0:000}", Convert.ToInt32(tile.X / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(tile.X) / 1000) % 1000),
        		string.Format("{0:000}", (Convert.ToInt32(tile.X) % 1000)),
        		string.Format("{0:000}", Convert.ToInt32(tile.Y / 1000000)),
        		string.Format("{0:000}", (Convert.ToInt32(tile.Y / 1000) % 1000)),
        		string.Format("{0:000}.{1}", (Convert.ToInt32(tile.Y) % 1000), tile.Layer.Extension)
        	});
		}
	}
}
