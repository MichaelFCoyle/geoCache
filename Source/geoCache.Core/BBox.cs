//
// File: BBox.cs
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
using System.ComponentModel;
using System.Globalization;

namespace GeoCache.Core
{
	[TypeConverter(typeof (BBoxConverter))]
	public class BBox : IBBox
	{
		public BBox() { }

		public BBox(string parseFrom)
		{
			if (string.IsNullOrEmpty(parseFrom))
				throw new ArgumentNullException("parseFrom");

			string[] items = parseFrom.Split(',');
			MinX = double.Parse(items[0], CultureInfo.InvariantCulture.NumberFormat);
			MinY = double.Parse(items[1], CultureInfo.InvariantCulture.NumberFormat);
			MaxX = double.Parse(items[2], CultureInfo.InvariantCulture.NumberFormat);
			MaxY = double.Parse(items[3], CultureInfo.InvariantCulture.NumberFormat);
		}

#if true
		/// <summary>
		/// Creates a new instance of <see cref="BBox"/>
		/// </summary>
		/// <param name="minX">West / MinX</param>
		/// <param name="minY">South / MinY</param>
		/// <param name="maxX">East / MaxX</param>
		/// <param name="maxY">North / MaxY</param>
		public BBox(double minX, double minY, double maxX, double maxY)
		{
			MinX = minX;
			MinY = minY;
			MaxX = maxX;
			MaxY = maxY;
		}
#endif
#if PYTHON_HELPER
	/// <summary>
	/// The bbox-coordinate:
	///0: West / MinX;
	///1: South / MinY;
	///2: East / MaxX;
	///3: North / MaxY;
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
		[Obsolete("0 = MinX | 1 = MinY | 2 = MaxX | 3 = MaxY", true)]
		public double this[int index]
		{
			get 
			{
				switch (index)
				{
					case 0: return MinX; //West
					case 1: return MinY; //South
					case 2: return MaxX; //East
					case 3: return MaxY; //North
					default: throw new ArgumentOutOfRangeException("index");
				}
			}
		}
#endif

		#region IBBox Members
		/// <summary>MinX = West</summary>
		public double MinX { get; set; }

		/// <summary>MaxX = East</summary>
		public double MaxX { get; set; }

		/// <summary>MinY = South</summary>
		public double MinY { get; set; }

		/// <summary>MaxY = North</summary>
		public double MaxY { get; set; }

		public double Width => MaxX - MinX;

		public double Height => MaxY - MinY;

		public int GetAspect()
		{
			double width = /*Math.Abs*/ (Width);
			double height = /*Math.Abs*/ (Height);
			return (width >= height)
			       	? (int) ((width / height) + .5) //Round up
			       	: (int) ((height / width) + .5); //Round up
		}

		public bool Contains(double x, double y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
		#endregion

		public override string ToString()
		{
			NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;
			return string.Format("{0},{1},{2},{3}", MinX.ToString(format), MinY.ToString(format), MaxX.ToString(format),
			                     MaxY.ToString(format));
		}

		#region Nested type: BBoxConverter
		public class BBoxConverter : TypeConverter
		{
			// Overrides the CanConvertFrom method of TypeConverter.
			// The ITypeDescriptorContext interface provides the context for the
			// conversion. Typically, this interface is used at design time to 
			// provide information about the design-time container.
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return (sourceType == typeof (string) || base.CanConvertFrom(context, sourceType));
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value is string)
				{
					string[] v = ((string) value).Split(',');
					return new BBox
					       	{
					       		MinX = double.Parse(v[0], CultureInfo.InvariantCulture.NumberFormat),
					       		MinY = double.Parse(v[1], CultureInfo.InvariantCulture.NumberFormat),
					       		MaxX = double.Parse(v[2], CultureInfo.InvariantCulture.NumberFormat),
					       		MaxY = double.Parse(v[3], CultureInfo.InvariantCulture.NumberFormat),
					       	};
				}
				return base.ConvertFrom(context, culture, value);
			}

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
			                                 Type destinationType)
			{
				if (destinationType == typeof (string))
				{
					var bbox = (BBox) value;
					NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;
					return string.Format("{0},{1},{2},{3}", bbox.MinX.ToString(format), bbox.MinY.ToString(format),
					                     bbox.MaxX.ToString(format), bbox.MaxY.ToString(format));
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
		#endregion
	}
}