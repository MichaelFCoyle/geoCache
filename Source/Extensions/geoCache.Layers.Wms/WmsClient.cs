//
// File: WmsClient.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2006-2007 MetaCarta, Inc.
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using GeoCache.Extensions.Base;

namespace GeoCache.Layers.Wms
{
    class WmsClient
    {

        static string[] _fields = new string[] 
		{ 
            "bbox", 
            "srs", 
            "width", 
            "height", 
            "format", 
            "layers", 
            "styles",
            "transparent"
        };

        static Dictionary<string, string> _defaultParams = new Dictionary<string, string>
		{
			{"EXCEPTIONS", "application/vnd.ogc.se_xml"},
			{"version", "1.1.1"},
			{"request", "GetMap"},
			{"service", "WMS"},
		};

        Uri _uri;

        #region python
        /*
		def __init__ (self, base, params):
			self.base    = base
			if self.base[-1] not in "?&":
				if "?" in self.base:
					self.base += "&"
				else:
					self.base += "?"

			self.params  = {}
			self.client  = urllib2.build_opener()
			for key, val in self.defaultParams.items():
				if self.base.lower().rfind("%s=" % key.lower()) == -1:
					self.params[key] = val
			for key in self.fields:
				if params.has_key(key):
					self.params[key] = params[key]
				elif self.base.lower().rfind("%s=" % key.lower()) == -1:
					self.params[key] = ""
		 */
        #endregion
        public WmsClient(Uri uri, IDictionary<string, string> @params)
        {
            var newParams = new Dictionary<string, string>(_defaultParams);
            foreach (var key in _fields)
            {
                if (@params.ContainsKey(key))
                {
                    if (newParams.ContainsKey(key))
                        newParams[key] = @params[key];
                    else
                        newParams.Add(key, @params[key]);
                }
            }

            //Reset params given in uriBase:
            var uriBuilder = new UriBuilder(uri);
            var queryParams = uriBuilder.GetQueryParams();
            foreach (var p in queryParams)
            {
                if (newParams.ContainsKey(p.Key))
                    newParams[p.Key] = p.Value;
                else
                    newParams.Add(p.Key, p.Value);
            }

            //UrlEncode params:
            uriBuilder.SetQuery(newParams, true);
            foreach (var interceptor in ObjectManager.GetUriInterceptors())
                interceptor.Intercept(uriBuilder);

            _uri = uriBuilder.Uri;
        }

        public byte[] Fetch()
        {
            int retryCount = 3;
            while (retryCount-- > 0)
            {
                var dataOk = true;
                try
                {
                    Trace.TraceInformation("WmsClient.Fetch: Fetching data from {0} ", _uri);
                    using (var client = new WebClient())
                    {
                        client.Headers.Add("user-agent", "GeoCache");
                        var data = client.DownloadData(_uri);

                        foreach (var validator in ObjectManager.GetResponseValidators())
                        {
                            if (!validator.Validate(data))
                                dataOk = false;
                        }

                        if (dataOk)
                            return data;
                        else
                            Trace.TraceWarning("Data failed to validate. Will retry {0} times.",retryCount );
                    }
                }
                catch(Exception ex)
                {
                    Trace.TraceError("Error retrieving data from url {0}:\r\n{1}", _uri, ex);
                }
            }
            throw new Exception("Data failed to validate");
        }
    }
}
