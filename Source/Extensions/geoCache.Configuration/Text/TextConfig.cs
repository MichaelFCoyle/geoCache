//
// File: TextConfig.cs
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
using System.Configuration;
using System.IO;
using GeoCache.Core;

namespace GeoCache.Configuration.Text
{
	public class TextConfig : ILayerContainer, ICache
	{
		private string _configFile = "geoCache.cfg";
		private readonly object _loadLock = new object();

		public string ConfigFile
		{
			get { return _configFile; }
			set { _configFile = value; }
		}

		#region ICache Members
		public byte[] Set(ITile tile, byte[] data)
		{
			return Cache.Set(tile, data);
		}

		public void Lock(ITile tile)
		{
			Cache.Lock(tile);
		}

		public bool Lock(ITile tile, bool blocking)
		{
			return Cache.Lock(tile, blocking);
		}

		public byte[] Get(ITile tile)
		{
			return Cache.Get(tile);
		}

		public void Unlock(ITile tile)
		{
			Cache.Unlock(tile);
		}
		#endregion

		#region ICache helper-functions
		private ICache _cache;
		ICache Cache
		{
			get
			{
				if (_cache == null)
				{
					lock (_loadLock)
					{
						if (_cache != null)
							return _cache;

						var cacheSection = GetCacheSection();
						IDictionary<string, object> properties;
						string type;
						if (cacheSection == null)
						{
							//No cache-settings found in config: Load defaults from [app|web].config
							properties = new Dictionary<string, object>
							             	{{"BaseDir", GetAppSetting("geoCache.DiskCache.Primary", "/tmp/tilecache")}};
							var secondary = GetAppSetting("geoCache.DiskCache.Secondary", "");
							if (!string.IsNullOrEmpty(secondary))
								properties.Add("SecondaryDir", secondary);
							type = "disk";
						}
						else
						{
							type = cacheSection.Properties.ContainsKey("type") ? cacheSection.Properties["type"].ToString() : "disk";
							properties = cacheSection.Properties;
						}

						var cache = ObjectManager.GetCache(type, properties);
						if (cache == null)
							throw new NotSupportedException("Unable to load cache of type 'disk'");
						_cache = cache;
					}
				}
				return _cache;
			}
		}

		protected static string GetAppSetting(string setting, string defaultValue)
		{
			string value = ConfigurationManager.AppSettings[setting];
			return string.IsNullOrEmpty(value) ? defaultValue : value;
		}
		#endregion

		#region ILayerContainer Members
		private Dictionary<string, ILayer> _layers;
		public Dictionary<string, ILayer> Layers
		{
			get
			{
				if(_layers == null)
					lock (_loadLock)
					{
						if (_layers != null)
							return _layers;
						var layers = new Dictionary<string, ILayer>();
						foreach (var section in GetLayerSections())
						{
							var type = section.Properties.ContainsKey("type") ? section.Properties["type"].ToString() : "wms";
							var layer = GetLayer(type, section.Name, section.Properties);
							if (layer != null)
								layers.Add(layer.Name, layer);
						}
						if (layers.Count == 0)
							Console.WriteLine("Unable to read any config-sections from {0}", GetConfigFile());
						_layers = layers;
					}
				return _layers;
			}
		}
		#endregion

		private static ILayer GetLayer(string type, string name, IDictionary<string, object> args)
		{
			if (type != "wms")
			{
				Console.WriteLine("Layer-type {0} is not supported. Layer {1} not added.", type, name);
				return null;
			}

			if (!args.ContainsKey("name"))
				args.Add("name", name);
			return ObjectManager.GetLayer(type, args);
		}


		private IEnumerable<ConfigSection> GetLayerSections()
		{
			var configSections = GetConfigSections();
			if (configSections != null && configSections.Count != 0)
			{
				foreach (var section in configSections)
				{
					if ("cache".Equals(section.Name, StringComparison.OrdinalIgnoreCase))
						continue;

					string type = "wms"; //Default to wms-layer
					//Currently ONLY wms layers are supported

					if (!section.Properties.ContainsKey("type"))
					{
						section.Properties.Add("type", type);
					}
					else
					{
						type = section.Properties["type"].ToString().Replace("Layer", "");
						if ("wms".Equals(type, StringComparison.OrdinalIgnoreCase))
							type = "wms";
						section.Properties["type"] = type;
					}
					yield return section;
				}
			}
		}

		private ConfigSection GetCacheSection()
		{
			foreach (var section in GetConfigSections())
			{
				if ("cache".Equals(section.Name, StringComparison.OrdinalIgnoreCase))
				{
					var properties = section.Properties;
					if (properties.ContainsKey("type"))
						properties["type"] = properties["type"].ToString().ToLower();
					if (properties.ContainsKey("base"))
						properties.Add("BaseDir", properties["base"]);
					return section;
				}
			}
			return null;
		}

		private List<ConfigSection> GetConfigSections()
		{
			string configFile = GetConfigFile();
			return !File.Exists(configFile)
			       	? null
			       	: new List<ConfigSection>(ConfigFileHelper.GetConfigSections(File.ReadAllText(configFile)));
		}

		private string GetConfigFile()
		{
			return Path.IsPathRooted(ConfigFile)
			       	? ConfigFile
			       	: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
			//Path.Combine(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile).DirectoryName, ConfigFile);
		}
	}
}