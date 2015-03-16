//
// File: TestBase.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using GeoCache.Core;

namespace GeoCache.Configuration.Test
{
	class TestBase
	{
		private const string CONFIG_FILE = @"E:\TFS\Map\Internet\geoCache\Source\geoCache.Demo\geoCache.Unity.config";

		static TestBase()
		{
			if (Resolver.Current == null)
				Resolver.Current = new UnityAddInExtensionLoader(CONFIG_FILE);
		}
	}
}
