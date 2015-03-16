//
// File: UnityAddInExtensionLoader.cs
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
using System.Diagnostics;
using System.IO;
using GeoCache.Configuration;
using GeoCache.Core;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace GeoCache
{
    public class UnityAddInExtensionLoader : IResolver
    {
        private static readonly object _containerLock = new object();
        private static string _exeConfigFileName;
        private static IUnityContainer _unityContainer;

        public UnityAddInExtensionLoader()
        {
            _exeConfigFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "geoCache.Unity.config");
        }

        public UnityAddInExtensionLoader(string exeConfigFileName)
        {
            _exeConfigFileName = exeConfigFileName;
        }

        internal static IUnityContainer UnityContainer
        {
            get
            {
                if (_unityContainer == null)
                {
                    lock (_containerLock)
                    {
                        if (_unityContainer != null)
                            return _unityContainer;
                        var map = new ExeConfigurationFileMap
                        {
                            ExeConfigFilename = _exeConfigFileName
                        };
                        var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                        var section = (UnityConfigurationSection)config.GetSection("unity");
                        var container = new UnityContainer();
                        section.Containers["geoCacheContainer"].Configure(container);
                        _unityContainer = container;
                        return container;
                    }
                }
                return _unityContainer;
            }
        }

        #region IResolver Members
        public T Resolve<T>(string id, IDictionary<string, object> config)
        {
            try
            {
                {

                    T extension = string.IsNullOrEmpty(id)
                                    ? UnityContainer.Resolve<T>()
                                    : UnityContainer.Resolve<T>(id);
                    if ((object)extension == null)
                        return default(T);

                    if (config != null)
                    {
                        var helper = new PropertyHelper(extension);
                        helper.SetProperties(config);
                    }
                    return extension;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to load {0}:\r\n{1}", typeof(T).FullName, ex);
            }
            return default(T);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return UnityContainer.ResolveAll<T>();
        }
        #endregion
    }
}