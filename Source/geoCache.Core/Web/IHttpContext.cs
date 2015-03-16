using System;
using System.Collections.Generic;
using System.Text;

namespace GeoCache.Core.Web
{
	public interface IHttpContext
	{
		IHttpRequest Request { get; }
		IHttpResponse Response { get; }
	}
}
