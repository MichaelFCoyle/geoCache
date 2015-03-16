//
// File: Converter.cs
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
using System.Reflection;
using GeoCache.Core;

namespace GeoCache.Configuration
{
	public static class Converter
	{
		public static bool TryConvert(Type toType, object from, out object result)
		{
			const BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.NonPublic;
			//var method = typeof(PropertyHelper).GetMethod("TryConvert", bindingAttr, null,  new[] { typeof(object), toType }, null);
			//var method = typeof(PropertyHelper).GetMethod("TryConvert", new[] { typeof(object), typeof(object).MakeByRefType() });
			MethodInfo method = typeof(Converter).GetMethod("MyTryConvert", bindingAttr);

			//Console.WriteLine("Method: " + method);
			var methodParams = new[] { from, null };
			object r = method.MakeGenericMethod(toType).Invoke(null, methodParams);
			result = methodParams[1];
			//Console.WriteLine("TryConvert: r: {0}", r);
			//Console.WriteLine("TryConvert: Result: {0}", result);
			return (bool)r;
		}

		public static bool TryConvert<T>(object from, out T result)
		{
			return new ConvertHelper<T>().TryConvert(from, out result);
		}

		//This method is used by [public static bool TryConvert(Type toType, object from, out object result)]
		//ReSharper disable UnusedPrivateMember
		private static bool MyTryConvert<T>(object from, out T result)
		{
			return TryConvert(from, out result);
		}
		// ReSharper restore UnusedPrivateMember

		#region Nested type: ConvertHelper
		private class ConvertHelper<T>
		{
			private static Type _resolvedToType;
			private static readonly Type _toType;

			static ConvertHelper()
			{
				_toType = typeof (T);
				if (_toType.IsGenericType && _toType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					var nc = new NullableConverter(_toType);
					_toType = nc.UnderlyingType;
				}
			}

			public bool TryConvert(object from, out T result)
			{
				if (from == null)
				{
					result = default(T);
					return false;
				}

				try
				{
					if (_resolvedToType != null)
					{
						return TryConvertResolvedTo(from, out result);
					}

					TypeConverter fromConverter = TypeDescriptor.GetConverter(from);
					if (fromConverter.CanConvertTo(_toType))
					{
						result = (T)fromConverter.ConvertTo(from, _toType);
						return true;
					}

					//Use TryConvert for misc types when fromType is string:
					if (from is string)
					{
						if (_toType == typeof(double))
						{
							double d;
							var numberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite |
							                  NumberStyles.AllowTrailingWhite;
							if (double.TryParse((string)from, numberStyle, CultureInfo.InvariantCulture, out d))
							{
								result = (T)(object)d;
								return true;
							}
						}
					}

					Type fromType = from.GetType();
					TypeConverter toConverter = TypeDescriptor.GetConverter(_toType);
					if (toConverter.CanConvertFrom(fromType))
					{
						result = (T)toConverter.ConvertFrom(from);
						return true;
					}

					//Try to use ConvertTo. If that fails, resolve the object-type, and try to convert to that type.
					try
					{
						result = ConvertTo(from);
					}
					catch (InvalidCastException)
					{
						object o = Resolver.Resolve<T>(null, null);
						if (o == null)
							throw;
						_resolvedToType = o.GetType();
						return TryConvertResolvedTo(from, out result);
					}
					return true;
				}
#if DEBUG
				catch (Exception ex)
				{
					Console.WriteLine("DEBUG: TryConvert failed to convert from {0} to {1}. Exception: {2}", from.GetType(), _toType, ex);
				}
#else
				catch {Exception}
#endif
				result = default(T);
				return false;
			}

			private static bool TryConvertResolvedTo(object from, out T result)
			{
				if (_resolvedToType == null)
				{
					result = default(T);
					return false;
				}

				object o;
				if (Converter.TryConvert(_resolvedToType, from, out o))
				{
					result = (T)o;
					return true;
				}
				result = default(T);
				return false;
			}

			//Portions of this function is copied from http://blogs.msdn.com/jongallant/archive/2006/06/19/637023.aspx
			private static T ConvertTo(object value)
			{
				if (_toType.IsEnum) // if enum use parse
					return (T)Enum.Parse(_toType, value.ToString(), false);

				// if we have a custom type converter then use it
				TypeConverter td = TypeDescriptor.GetConverter(_toType);
				if (td.CanConvertFrom(value.GetType()))
				{
					return (T)td.ConvertFrom(value);
				}

				// otherwise use the changetype
				return (T)Convert.ChangeType(value, _toType);
			}
		}
		#endregion
	}
}