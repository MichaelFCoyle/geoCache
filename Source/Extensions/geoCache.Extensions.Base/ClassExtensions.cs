//
// File: ClassExtensions.cs
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

namespace GeoCache.Extensions.Base
{
	public static class ClassExtensions
	{
		public static bool Contains<T>(this IEnumerable<T> self, IEnumerable<T> items)
		{
			Collection<T> collectionOfT = new Collection<T>();
			foreach (var item in self)
				collectionOfT.Add(item);
			return Contains(collectionOfT, items);
		}

		public static bool Contains<T>(this ICollection<T> self, IEnumerable<T> items)
		{
			if (self == null)
				throw new ArgumentNullException("self");
			if (items == null)
				throw new ArgumentNullException("items");

			foreach (var item in items)
				if (self.Contains(item))
					return true;
			return false;
		}

		#region python conversion-helpers
#if PYTHON_HELPER
		[Obsolete("Use .Width")]
		public static double _0(this SizeD self)
		{
			return self.Width;
		}

		[Obsolete("Use .Height")]
		public static double _1(this SizeD self)
		{
			return self.Height;
		}

		[Obsolete("Use .Width")]
		public static int _0(this Size self)
		{
			return self.Width;
		}

		[Obsolete("Use .Height")]
		public static int _1(this Size self)
		{
			return self.Height;
		}
#endif
		#endregion

		public static IDictionary<string, string> GetQueryParams(this UriBuilder self)
		{
			var result = new Dictionary<string, string>();
			var query = self.Query;
			
			if(!string.IsNullOrEmpty(query) && query.StartsWith("?"))
				query = query.TrimStart('?');

			if (!string.IsNullOrEmpty(query))
			{
				foreach (var p in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
				{
					
					var param = p.Split(new char[] { '=' });
					var key = param[0];
					var value = param[1];

					if (!result.ContainsKey(key))
						result.Add(key, value);
				}
			}
			return result;
		}

		public static void SetQuery(this UriBuilder self, IDictionary<string, string> queryParams, bool urlEncodeValues)
		{
			var paramList = new List<string>();
			if (queryParams == null)
			{
				self.Query = string.Empty;
				return;
			}
			foreach (var p in queryParams)
			{
				paramList.Add(p.Key + "=" + (urlEncodeValues
					? System.Web.HttpUtility.UrlEncode(p.Value)
					: p.Value));
			}
			self.Query = string.Join("&", paramList.ToArray());
		}

#if DEBUG
		
#endif
		public static void Remove<TKey, TValue>(this IDictionary<TKey, TValue> self, ICollection<TKey> itemsToRemove)
		{
			if(itemsToRemove == null)
				return;

			foreach (var key in itemsToRemove)
				self.Remove(key);
		}

	}
}
