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

using System.IO;
using System.Web;
using GeoCache.Core.Web;

namespace GeoCache.Web
{
	public class WebHttpResponse : IHttpResponse
	{
		public WebHttpResponse(HttpResponse response) => m_response = response;

		readonly HttpResponse m_response;

		public string ContentType { get => m_response.ContentType; set => m_response.ContentType = value; }

		public TextWriter Output => m_response.Output;

		public Stream OutputStream => m_response.OutputStream;

		public void Write(string s) => m_response.Write(s);

		public void Write(object obj) => m_response.Write(obj);

		public void Redirect(string url) => m_response.Redirect(url);

		public void Redirect(string url, bool endResponse) => m_response.Redirect(url, endResponse);
	}
}
