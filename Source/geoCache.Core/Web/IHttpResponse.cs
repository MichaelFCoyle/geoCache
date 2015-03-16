using System;
using System.IO;

namespace GeoCache.Core.Web
{
	public interface IHttpResponse
	{
		string ContentType { get; set; }
		
		TextWriter Output { get; }
		Stream OutputStream { get; }
		
		void Write(string s);
		void Write(object obj);
		
		void Redirect(string url);
		void Redirect(string url, bool endResponse);
	}
}
