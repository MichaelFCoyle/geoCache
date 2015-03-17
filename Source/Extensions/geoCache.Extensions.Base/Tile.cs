//
// File: Tile.cs
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
    //public struct Bounds { public double MinX, MinY, MaxX, MaxY;}

    public class Tile : ITile
    {
        #region python
        /*
		def __init__ (self, layer, x, y, z):
			self.layer = layer
			self.x = x
			self.y = y
			self.z = z
			self.data = None
		 */
        #endregion
        public Tile(ILayer layer, Cell cell)
        {
            Layer = layer;
            Cell = cell;
        }

        public Tile(ILayer layer, double x, double y, int z)
            : this(layer, new Cell(x, y, z)) { }


        #region properties

        public ILayer Layer { get; set; }

        public Cell Cell { get; set; }

        public double X { get { return Cell.X; } }

        public double Y { get { return Cell.Y; } }

        public int Z { get { return Cell.Z; } }

        #region python
        /*
		def size (self):
			return self.layer.size
		 */
        #endregion
        public virtual Size Size { get { return Layer.Size; } }

        #region python
        /*
		def bounds (self):
			res  = self.layer.resolutions[self.z]
			minx = self.layer.bbox[0] + (res * self.x * self.layer.size[0])
			miny = self.layer.bbox[1] + (res * self.y * self.layer.size[1])
			maxx = self.layer.bbox[0] + (res * (self.x + 1) * self.layer.size[0])
			maxy = self.layer.bbox[1] + (res * (self.y + 1) * self.layer.size[1])
			return (minx, miny, maxx, maxy)
		 */
        #endregion
        public virtual IBBox Bounds
        {
            get
            {
                var res = Layer.Resolutions[Z];
                return new BBox
                {
                    MinX = Layer.BBox.MinX + (res * Cell.X * Layer.Size.Width/*[0]*/),
                    MinY = Layer.BBox.MinY + (res * Cell.Y * Layer.Size.Height/*[1]*/),
                    MaxX = Layer.BBox.MinX + (res * (Cell.X + 1) * Layer.Size.Width/*[0]*/),
                    MaxY = Layer.BBox.MinY + (res * (Cell.Y + 1) * Layer.Size.Height/*[1]*/)
                };
            }
        }

        #region python
        /*
		def bbox (self):
			return ",".join(map(str, self.bounds()))
		 */
        #endregion
        public string BBox
        {
            get { return Bounds.ToString(); }
        }
        #endregion
    }
}
