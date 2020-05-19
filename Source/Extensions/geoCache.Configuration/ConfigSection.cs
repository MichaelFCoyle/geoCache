//
// File: ConfigSection.cs
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

namespace GeoCache.Configuration
{
	public class ConfigSection
	{
		private readonly IDictionary<string, object> m_values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		public string Name { get; set; }

		public IDictionary<string, object> Properties => m_values;
	}
}
