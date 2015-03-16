//
// File: WebHttpResponse.cs
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

using GeoCache.Core.Web;
using System.Web;

namespace GeoCache.Web
{
	public class WebHttpResponse : IHttpResponse
	{
		readonly HttpResponse _response;
		public WebHttpResponse(HttpResponse response)
		{
			_response = response;
		}
		
		public string ContentType
		{
			get { return _response.ContentType; }
			set { _response.ContentType = value; }
		}

		public System.IO.TextWriter Output { get { return _response.Output; } }

		public System.IO.Stream OutputStream { get { return _response.OutputStream; } }

		public void Write(string s) { _response.Write(s); }

		public void Write(object obj) { _response.Write(obj); }
		
		public void Redirect(string url) { _response.Redirect(url); }
		
		public void Redirect(string url, bool endResponse) { _response.Redirect(url, endResponse); }
	}
}
