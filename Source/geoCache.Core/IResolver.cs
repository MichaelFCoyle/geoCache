//
// File: IResolver.cs
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

using System.Collections.Generic;

namespace GeoCache.Core
{
	public interface IResolver
	{
		T Resolve<T>(string id, IDictionary<string, object> config);
		IEnumerable<T> ResolveAll<T>();
	}
}
