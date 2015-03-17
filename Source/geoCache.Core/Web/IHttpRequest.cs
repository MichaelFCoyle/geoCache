using System;
using System.Collections.Specialized;

namespace GeoCache.Core.Web
{
	public interface IHttpRequest
	{
		NameValueCollection Params { get; }

        string FilePath { get; }

		Uri Url { get; }
	}
}
