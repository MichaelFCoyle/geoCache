﻿//
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
        public WebHttpContext(HttpContext context)
        {
            _context = context;
        }

        readonly HttpContext _context;

        public IHttpRequest Request { get { return new WebHttpRequest(_context.Request); } }

        public IHttpResponse Response { get { return new WebHttpResponse(_context.Response); } }
    }
}
