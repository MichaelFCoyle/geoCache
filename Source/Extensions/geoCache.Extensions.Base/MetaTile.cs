//
// File: MetaTile.cs
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

using System.Drawing;
using GeoCache.Core;

namespace GeoCache.Extensions.Base
{
	public class MetaTile : Tile
	{
		public MetaTile(Layer layer, int x, int y, int z)
			: base(layer, x, y, z)
		{
		}

		#region python
		/*
    def actualSize (self):
        metaCols, metaRows = self.layer.getMetaSize(self.z)
        return ( self.layer.size[0] * metaCols,
                 self.layer.size[1] * metaRows )
		 */
		#endregion

		public Size ActualSize
		{
			get
			{
				Size mt = Layer.GetMetaSize(Z);
				return new Size
					(Layer.Size.Width /*[0]*/* mt.Width
					 , Layer.Size.Height /*[1]*/* mt.Height);
			}
		}

		#region python
		/*
    def size (self):
        actual = self.actualSize()
        return ( actual[0] + self.layer.metaBuffer[0] * 2, 
                 actual[1] + self.layer.metaBuffer[1] * 2 )
		 */
		#endregion
		public override Size Size
		{
			get
			{
				Size actual = ActualSize;
				return new Size
					(actual.Width + Layer.MetaBuffer.Width * 2
					 , actual.Height + Layer.MetaBuffer.Height * 2);
			}
		}

		#region python
		/*
    def bounds (self):
        tilesize   = self.actualSize()
        res        = self.layer.resolutions[self.z]
        buffer     = (res * self.layer.metaBuffer[0], res * self.layer.metaBuffer[1])
        metaWidth  = res * tilesize[0]
        metaHeight = res * tilesize[1]
        minx = self.layer.bbox[0] + self.x * metaWidth  - buffer[0]
        miny = self.layer.bbox[1] + self.y * metaHeight - buffer[1]
        maxx = minx + metaWidth  + 2 * buffer[0]
        maxy = miny + metaHeight + 2 * buffer[1]
        return (minx, miny, maxx, maxy)
		*/
		#endregion
		public override IBBox Bounds
		{
			get
			{
				Size tilesize = ActualSize;
				double res = Layer.Resolutions[Z];
				var buffer = new SizeD(res * Layer.MetaBuffer.Width, res * Layer.MetaBuffer.Height);
				double metaWidth = res * tilesize.Width;
				double metaHeight = res * tilesize.Height;

				double minX = Layer.BBox.MinX + X * metaWidth - buffer.Width;
				double minY = Layer.BBox.MinY + Y * metaWidth - buffer.Height;

				double maxX = minX + metaWidth + 2 * buffer.Width;
				double maxY = minY + metaHeight + 2 * buffer.Height;

				return new BBox(minX, minY, maxX, maxY);
			}
		}


	}
}