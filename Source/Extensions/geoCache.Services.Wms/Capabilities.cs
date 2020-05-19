//
// File: Capabilities.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2006-2007 MetaCarta, Inc.
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)


namespace GeoCache.Services.Wms
{
    class Capabilities
    {
        public string Format { get; private set; }
       
        public object Data { get; private set; }

        #region python
        /*
		def __init__ (self, format, data):
			self.format = format
			self.data   = data
		 */
        #endregion
        public Capabilities(string format, string data) : this(format) => Data = data;

        public Capabilities(string format, byte[] data)
        {
            Format = format;
            Data = data;
        }

        private Capabilities(string format) => Format = format;

    }
}
