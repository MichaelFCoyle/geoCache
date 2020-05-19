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

using System.Web;
using GeoCache.Core.Web;

namespace GeoCache.Web
{
    public class WebHttpContext : IHttpContext
    {
        public WebHttpContext(HttpContext context) => m_context = context;

        readonly HttpContext m_context;

        public IHttpRequest Request => new WebHttpRequest(m_context.Request);

        public IHttpResponse Response => new WebHttpResponse(m_context.Response);
    }
}
