//
// File: ConfigFileHelper.cs
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
using System.IO;
using System.Text;
using GeoCache.Core;

namespace GeoCache.Configuration
{
	public static class ConfigFileHelper
	{
		public static string ToConfigText(ILayer self)
		{
			var sb = new StringBuilder();
			var writer = new StringWriter(sb);
			TileCacheConfigHelper.ToConfigText(writer, self);
			return sb.ToString();
		}

		public static void LoadConfigText(ILayer self, string config)
		{
			if(self == null)
				throw new ArgumentNullException("self");
			if(string.IsNullOrEmpty(config))
				throw new ArgumentOutOfRangeException("config");

			var cs = GetConfigSection(new StringReader(config));
			if (cs == null)
				return;
			self.Name = cs.Name;
			TileCacheConfigHelper.ConfigureLayer(self, cs.Properties);
		}

		public static IEnumerable<ConfigSection> GetConfigSections(string text)
		{
			if(text == null)
				throw new ArgumentNullException("text");
			var reader = new StringReader(text);
			while (true)
			{
				var section = GetConfigSection(reader);
				if (section == null)
					yield break;
				yield return section;

			}
		}
		private static ConfigSection GetConfigSection(TextReader reader)
		{
			var configSection = new ConfigSection();
			while(true)
			{
				//Break when no more characters
				if (reader.Peek() == -1)
					break;
				
				SwallowWhitespace(reader);


				bool isStartOfSection = reader.Peek() == '[';
				if (configSection.Name != null && isStartOfSection)
					break;
				if (//Ignore all lines starting with #
					(reader.Peek() == '#') ||
					//Ignore all lines not starting with '[' until name is found
					(configSection.Name == null && !isStartOfSection))
				{
					reader.ReadLine();
					continue;
				}
				
				var s = reader.ReadLine();
				
				//Stop reading values when all lines are read
				if (s == null)
					break;

				//Set newName FIRST TIME line starts with [ - Second time found: Abort further reading.
				if (isStartOfSection)
				{
					configSection.Name = s.TrimStart('[', ' ').TrimEnd(']', ' ');
					continue;
				}

				//If line does not contain '=' at position > 0, - ignore line
				var indexOfEqualsSign = s.IndexOf('=');
				if (indexOfEqualsSign < 1) 
					continue;
				//this is a "normal" property. - Add to the properties collection
				var key = s.Substring(0, indexOfEqualsSign).Trim();
				if (configSection.Properties.ContainsKey(key))
					configSection.Properties.Remove(key);
				configSection.Properties.Add(key, s.Substring(indexOfEqualsSign + 1).Trim());
			}
			if (configSection.Name == null)
				return null;
			return configSection;
		}

		private static void SwallowWhitespace(TextReader reader)
		{
			while (IsWhiteSpace(reader.Peek()))
				reader.Read();
		}

		static bool IsWhiteSpace(int character) => character == '\t' || character == ' ';

		#region Nested type: TileCacheConfigHelper
		private class TileCacheConfigHelper
		{
			private static readonly string[] _commonProperties = new[]
         		{
         			"Name", 
                    "MaxResolution", 
                    "Srs", 
                    "BBox", 
                    "ExtentType", 
                    "Layers",
         			"Units", 
                    "Url", 
                    "Extension", 
                    "DelayedLoading",
                    "Transparent"
         		};

			public static void ToConfigText(TextWriter writer, ILayer layer)
			{
				writer.WriteLine("[" + layer.Name + "]");
				var propertyHelper = new PropertyHelper(layer);
				Array.ForEach(_commonProperties, property => propertyHelper.WriteProperty(property, writer));
				writer.WriteLine();
			}

			public static void ConfigureLayer(ILayer layer, IDictionary<string, object> properties)
			{
				if (layer == null) throw new ArgumentNullException("layer");
				if (properties == null) throw new ArgumentNullException("properties");
				new PropertyHelper(layer).SetProperties(properties);
			}
		}
		#endregion
	}
}