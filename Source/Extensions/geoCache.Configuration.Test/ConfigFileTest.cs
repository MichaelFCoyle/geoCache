//
// File: ConfigFileTest.cs
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
using GeoCache.Configuration;
using GeoCache.Core;
using GeoCache.Layers.Wms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoCache.Configuration.Test
{
	[TestClass]
	class ConfigFileTest : TestBase
	{
		[TestMethod]
		public void LayerExtensions_ToConfigTextShouldNotBeStringEmpty()
		{
			var layer = ObjectManager.GetLayer("wms", null);
			Assert.IsNotNull(layer, "layer");
			Assert.IsFalse(string.IsNullOrEmpty(ConfigFileHelper.ToConfigText(layer)), "layer.ToConfigText");
		}

		[TestMethod]
		public void LayerExtensions_ShouldHandleMultipleSections()
		{
			var config = @"
			[s0]
			p1=v1
			# This is a comment...
			paa=paa

			[s1]
			p2=v2
			p3=v3
			[s2]
			p4=v4
			p5=v5
			";
			var sections = new List<ConfigSection>(ConfigFileHelper.GetConfigSections(config));
			Assert.AreEqual(3, sections.Count);
			Assert.AreEqual("s0", sections[0].Name);
			Assert.AreEqual("s1", sections[1].Name);
			Assert.AreEqual("s2", sections[2].Name);
			var p0 = sections[0].Properties;
			var p1 = sections[1].Properties;
			var p2 = sections[2].Properties;
			Assert.IsFalse(p1.ContainsKey("p1"), "s1 should not contain p1");
			Assert.IsTrue(p1.ContainsKey("p2"), "s1 should contain p2");
			Assert.IsTrue(p1.ContainsKey("p3"), "s1 should contain p3");
			Assert.IsFalse(p1.ContainsKey("p4"), "s1 should not contain p4");
			Assert.IsFalse(p1.ContainsKey("p5"), "s1 should not contain p5");
			Assert.AreEqual("paa", p0["paa"]);
			Assert.AreEqual("v1", p0["p1"], "p1");
			Assert.AreEqual("v2", p1["p2"], "p2");
			Assert.AreEqual("v3", p1["p3"], "p3");
			Assert.AreEqual("v4", p2["p4"], "p4");
			Assert.AreEqual("v5", p2["p5"], "p5");

		}

		[TestMethod]
		public void LayerExtensions_LoadConfigTextShouldBeCaseInsensitive()
		{
			var layer = ObjectManager.GetLayer("wms", null);
			var config = "[test-name]\nbbox=-570684.15,6424344.35,1596363.87,7964447.65";
			ConfigFileHelper.LoadConfigText(layer, config);
			Assert.AreEqual(1596363.87, layer.BBox.MaxX);
		}

		[TestMethod]
		public void LayerExtensions_LoadConfigTextShouldHandleMultipleSections()
		{
			var layer = ObjectManager.GetLayer("wms", null);
			var config = @"
			[test-name]
			ExtentType=Loose
			# This is a comment... Override ExtentType to Strict
			[new-section]
			ExtentType=Strict
			";
			ConfigFileHelper.LoadConfigText(layer, config);
			var wmsLayer = layer as WmsLayer;
			Assert.IsNotNull(wmsLayer);
			// ReSharper disable PossibleNullReferenceException
			//Assert.AreEqual(ExtentType.Loose, wmsLayer.ExtentType);
			// ReSharper restore PossibleNullReferenceException
		}

		[TestMethod]
		public void LayerExtensions_LoadConfigTextShouldHandleDuplicateValuesAndComments()
		{
			var layer = ObjectManager.GetLayer("wms", null);
			var config = @"
			[test-name]
			ExtentType=Loose
			# This is a comment... Override ExtentType to Strict
			ExtentType=Strict
			";
			ConfigFileHelper.LoadConfigText(layer, config);
			var wmsLayer = layer as WmsLayer;
			Assert.IsNotNull(wmsLayer);
			// ReSharper disable PossibleNullReferenceException
			Assert.AreEqual(ExtentType.Strict, wmsLayer.ExtentType);
			// ReSharper restore PossibleNullReferenceException
		}

		[TestMethod]
		public void LayerExtensions_LoadConfigTextShouldLoadBasicValues()
		{
			var layer = ObjectManager.GetLayer("wms", null);
			var config = @"
			[test-name]
			BBox=-570684.15,6424344.35,1596363.87,7964447.65
			Extension=abc
			ContentType=image/test
			Resolutions=2,1.234
			Url=http://test.url/test
			MaxResolution=1.2
			Units=m
			Layers=layer1,layer2
			ExtentType=Strict
			";
			ConfigFileHelper.LoadConfigText(layer, config);
			Assert.AreEqual("abc", layer.Extension);
			Assert.AreEqual(1596363.87, layer.BBox.MaxX);
			Assert.AreEqual("image/test", layer.ContentType);
			Assert.IsTrue(layer.Resolutions.Contains(2), "layer.Resolutions should contain '2'");
			Assert.IsTrue(layer.Resolutions.Contains(1.234), "layer.Resolutions should contain '1.234'");
			Assert.IsFalse(layer.Resolutions.Contains(1), "layer.Resolutions should not contain '1'");
			var wmsLayer = layer as WmsLayer;
			Assert.IsNotNull(wmsLayer);
			// ReSharper disable PossibleNullReferenceException
			Assert.AreEqual(1.2, wmsLayer.MaxResolution);
			Assert.AreEqual(new Uri("http://test.url/test"), wmsLayer.Url);
			Assert.AreEqual("m", wmsLayer.Units);
			Assert.AreEqual("layer1,layer2", wmsLayer.Layers);
			// ReSharper restore PossibleNullReferenceException
			Assert.AreEqual(ExtentType.Strict, wmsLayer.ExtentType);
			Console.WriteLine(ConfigFileHelper.ToConfigText(layer));
		}
	}
}
