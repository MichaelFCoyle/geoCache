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
		public WebHttpRequest(HttpRequest request) => m_request = request;

		readonly HttpRequest m_request;

		public NameValueCollection Params => m_request.Params;

		public string FilePath => m_request.FilePath;

		public Uri Url => m_request.Url;
	}
}
