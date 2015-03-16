//
// File: ConverterTest.cs
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoCache.Configuration.Test
{
	[TestClass]
	class ConverterTest : TestBase
	{
		private const string _parseBBoxFrom = "-570684.15,6424344.35,1596363.87,7964447.65";

		[TestMethod]
		public void Converter_TryConvertMustParseStringToDouble()
		{
			VerifyConvert("1", 1.0);
			VerifyConvert("1,1", 1.1);
			VerifyConvert("1.1", 1.1);
		}

		[TestMethod]
		public void Converter_TryConvertMustParseStringToInteger()
		{
			int i;
			if (!Converter.TryConvert("1", out i))
				Assert.Fail("Unable to convert '1' -> integer");
			Assert.AreEqual(1, i);
		}

		[TestMethod]
		public void Converter_TryConvertShouldBeAbleToParseBBox()
		{
			Converter_TryConvertShouldBeAbleToParseIBBox<BBox>();
		}

		[TestMethod]
		public void Converter_TryConvertShouldBeAbleToParseIBBox()
		{
			Converter_TryConvertShouldBeAbleToParseIBBox<IBBox>();
		}

		private static void Converter_TryConvertShouldBeAbleToParseIBBox<T>() where T : IBBox
		{
			T result1;
			object o;
			if(!Converter.TryConvert(_parseBBoxFrom, out result1))
				Assert.Fail("Unable to convert {0} to {1}", _parseBBoxFrom, typeof(T));
			FailIfInvalidBBox(result1);

			if (!Converter.TryConvert(typeof(T), _parseBBoxFrom, out o))
				Assert.Fail("Unable to convert {0} to {1}", _parseBBoxFrom, typeof(T));

			T i2 = o is T ? (T) o : default(T);
			FailIfInvalidBBox(i2);
		}

		private static void FailIfInvalidBBox(IBBox bbox)
		{
			Assert.IsNotNull(bbox, "bbox should not be null");
			Assert.AreEqual(1596363.87, bbox.MaxX);
			Assert.AreEqual(7964447.65, bbox.MaxY);
			Assert.AreEqual(-570684.15, bbox.MinX);
			Assert.AreEqual(6424344.35, bbox.MinY);
		}

		private static void VerifyConvert<T>(string text, T expected)
		{
			T actual;
			if (!Converter.TryConvert(text, out actual))
			{
				Assert.Fail(string.Format("Unable to convert '{0}' -> {1}", text, typeof(T)));
			}
			Assert.AreEqual(expected, actual);
		}



	}
}
