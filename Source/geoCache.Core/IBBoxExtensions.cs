//
// File: IBBoxExtensions.cs
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
using System.Drawing;

namespace GeoCache.Core
{
    public static class IBBoxExtensions
    {
		#region python
		/*
		 * Note: Original python-implementation was defined in Layer.py
		 * 
    def getResolution (self, (minx, miny, maxx, maxy)):
        return max( (maxx - minx) / self.size[0],
                    (maxy - miny) / self.size[1] )
			 */
		#endregion

		public static double GetResolution(this IBBox self, Size size)
        {
			var resX = (self.MaxX - self.MinX) / size.Width;
			var resY = (self.MaxY - self.MinY) / size.Height;
			return Math.Max(resX, resY);
        }

        public static Resolutions GetResolutions(this IBBox self, int levels, Size size)
        {
            var width = self.Width;
            var height = self.Height;
            var aspect = self.GetAspect();

            double maxResolution = (width >= height)
                ? width / (size.Width/*[0]*/ * aspect)
                : width / (size.Height/*[1]*/ * aspect);

            return Resolutions.Get(levels, maxResolution);
        }
    }
}
