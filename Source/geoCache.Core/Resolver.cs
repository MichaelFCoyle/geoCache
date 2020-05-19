//
// File: Resolver.cs
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
using System.Reflection;

namespace GeoCache.Core
{
	public static class Resolver
	{
		public static IResolver Current { get; set; }

		public static T Resolve<T>(string id, IDictionary<string, object> config) => Current.Resolve<T>(id, config);

		public static IEnumerable<T> ResolveAll<T>() => Current.ResolveAll<T>();

		private static readonly MethodInfo _resolveMethod 
			= typeof(Resolver).GetMethod("Resolve", new Type[] { typeof(string), typeof(IDictionary<string, object>) });

		private static readonly MethodInfo _resolveAllMethod
			= typeof(Resolver).GetMethod("ResolveAll", new Type[] { });

		public static object Resolve(Type type, string id, IDictionary<string, object> config)
		{
			var method = _resolveMethod.MakeGenericMethod(type);
			return method.Invoke(Current, new object[] { id, config });
		}

		public static object ResolveAll(Type type)
		{
			var method = _resolveAllMethod.MakeGenericMethod(type);
			return method.Invoke(Current, new object[] { });
		}
	}
}
