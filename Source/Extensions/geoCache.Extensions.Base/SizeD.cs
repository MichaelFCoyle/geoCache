//
// File: SizeD.cs
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

namespace GeoCache.Extensions.Base
{
	public class Size<T>
	{
		public T Width { get; set; }
		public T Height { get; set; }
	}

	public class SizeD : Size<double>
	{
		public SizeD(double width, double height)
		{
			Width = width;
			Height = height;
		}
	}
}
