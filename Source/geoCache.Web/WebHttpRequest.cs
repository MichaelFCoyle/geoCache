//
// File: WebHttpRequest.cs
//
// Authors:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;
using System.Collections.Specialized;
using System.Web;
using GeoCache.Core.Web;

namespace GeoCache.Web
{
	public class WebHttpRequest : IHttpRequest
	{
		public WebHttpRequest(HttpRequest request)
		{
			_request = request;
		}

        readonly HttpRequest _request;
        
        public NameValueCollection Params { get { return _request.Params; } }

        public string FilePath { get { return _request.FilePath; } }
		
        public Uri Url { get { return _request.Url; } }
	}
}
