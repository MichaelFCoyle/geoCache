//
// File: ResolverTest.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Generic;
using GeoCache.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoCache.Configuration.Test
{
	[TestClass]
	internal class ResolverTest : TestBase
	{
		private const string _parseBBoxFrom = "-570684.15,6424344.35,1596363.87,7964447.65";

		[TestMethod]
		public void Resolver_ShouldBeAbleToResolveIBBox()
		{
			object o1 = Resolver.Resolve(typeof(IBBox), null, null);
			Assert.IsNotNull(o1, "Should be able to resolve IBBox");
			Assert.IsNotNull(Resolver.Resolve<IBBox>(null, null), "Should be able to resolve IBBoxusing generic method");
			Assert.AreEqual(o1.GetType(), o1.GetType(), "Generic and non-generic method should resolve to the same type");
		}

		[TestMethod]
		public void Resolver_ShouldBeAbleToResolveITileRendererUsingTypeof()
		{
			object o1 = Resolver.Resolve(typeof(ITileRenderer), null, null);
			Assert.IsNotNull(o1, "Should be able to resolve ITileRenderer");
			Assert.IsNotNull(Resolver.Resolve<ITileRenderer>(null, null), "Should be able to resolve ITileRenderer using generic method");
			Assert.AreEqual(o1.GetType(), o1.GetType(), "Generic and non-generic method should resolve to the same type");
		}

		[TestMethod]
		public static void SimpleTest()
    {
		var properties = new Dictionary<string, object> { { "BBox", _parseBBoxFrom } };
        Assert.AreEqual(1596363.87, ObjectManager.GetLayer("wms", properties).BBox.MaxX);
    }

		[TestMethod]
		public void aaa()
		{
			ILayer layer = ObjectManager.GetLayer("wms", null);
			new PropertyHelper(layer).SetProperty("DelayedLoading", true);
			Console.WriteLine(layer.BBox);
		}
	}
}
