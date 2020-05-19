//
// File: IBBox.cs
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


namespace GeoCache.Core
{
	public interface IBBox
	{
		bool Contains(double x, double y);
		int GetAspect();
        double Height { get; }
        double MaxX { get; set; }
		double MaxY { get; set; }
		double MinX { get; set; }
		double MinY { get; set; }
        double Width { get; }
    }
}
