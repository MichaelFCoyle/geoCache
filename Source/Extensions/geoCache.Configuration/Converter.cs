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

		public static bool TryConvert<T>(object from, out T result) => new ConvertHelper<T>().TryConvert(from, out result);

		//This method is used by [public static bool TryConvert(Type toType, object from, out object result)]
		//ReSharper disable UnusedPrivateMember
		private static bool MyTryConvert<T>(object from, out T result) => TryConvert(from, out result);
		
		// ReSharper restore UnusedPrivateMember

		#region Nested type: ConvertHelper
		private class ConvertHelper<T>
		{
			private static Type m_resolvedToType;
			private static readonly Type m_toType;

			static ConvertHelper()
			{
				m_toType = typeof (T);
				if (m_toType.IsGenericType && m_toType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					var nc = new NullableConverter(m_toType);
					m_toType = nc.UnderlyingType;
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
					if (m_resolvedToType != null)
					{
						return TryConvertResolvedTo(from, out result);
					}

					TypeConverter fromConverter = TypeDescriptor.GetConverter(from);
					if (fromConverter.CanConvertTo(m_toType))
					{
						result = (T)fromConverter.ConvertTo(from, m_toType);
						return true;
					}

					//Use TryConvert for misc types when fromType is string:
					if (from is string)
					{
						if (m_toType == typeof(double))
						{
							var numberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
							if (double.TryParse((string)from, numberStyle, CultureInfo.InvariantCulture, out double d))
							{
								result = (T)(object)d;
								return true;
							}
						}
					}

					Type fromType = from.GetType();
					TypeConverter toConverter = TypeDescriptor.GetConverter(m_toType);
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
						m_resolvedToType = o.GetType();
						return TryConvertResolvedTo(from, out result);
					}
					return true;
				}
#if DEBUG
				catch (Exception ex)
				{
					Console.WriteLine("DEBUG: TryConvert failed to convert from {0} to {1}. Exception: {2}", from.GetType(), m_toType, ex);
				}
#else
				catch {}
#endif
				result = default(T);
				return false;
			}

			private static bool TryConvertResolvedTo(object from, out T result)
			{
				if (m_resolvedToType == null)
				{
					result = default(T);
					return false;
				}

				object o;
				if (Converter.TryConvert(m_resolvedToType, from, out o))
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
				if (m_toType.IsEnum) // if enum use parse
					return (T)Enum.Parse(m_toType, value.ToString(), false);

				// if we have a custom type converter then use it
				TypeConverter td = TypeDescriptor.GetConverter(m_toType);
				if (td.CanConvertFrom(value.GetType()))
				{
					return (T)td.ConvertFrom(value);
				}

				// otherwise use the changetype
				return (T)Convert.ChangeType(value, m_toType);
			}
		}
		#endregion
	}
}