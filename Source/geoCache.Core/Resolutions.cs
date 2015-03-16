//
// File: Resolutions.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace GeoCache.Core
{
	[TypeConverter(typeof(ResolutionsConverter))]
	public class Resolutions : Collection<double>
	{
		public override string ToString()
		{
			return (string)new ResolutionsConverter().ConvertTo(this, typeof(string));
		}

		public static Resolutions Get(int levels, double maxResolution)
		{
			var r = new Resolutions();
			r.Add(maxResolution);
			for (int i = 0; i < levels - 1; i++)
				r.Add(maxResolution / (2 << i));
			return r;
		}

		class ResolutionsConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return (sourceType == typeof(string) || base.CanConvertFrom(context, sourceType));
			}

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType == typeof(string))
				{
					var resolutions = (Resolutions)value;
					return string.Join(",", new List<double>(resolutions).ConvertAll((double item) => item.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)).ToArray());
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value is string)
				{
					var r = new Resolutions();
					foreach (var v in ((string)value).Split(','))
						r.Add(double.Parse(v, System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
					return r;
				}
				return base.ConvertFrom(context, culture, value);
			}

		}
	}

}
