using System;
using System.Collections.Specialized;

namespace GeoCache.Core.Web
{
	public interface IHttpRequest
	{
		NameValueCollection Params { get; }
		Uri Url { get; }
	}
}
