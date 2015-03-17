//
// File: ILayer.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System.Drawing;

namespace GeoCache.Core
{
	public interface ILayer
	{
		IBBox BBox { get; }

        string Extension { get; }
		
        ExtentType ExtentType { get; }
		
        string Format { get; }
		
        Size GetMetaSize(int z);

        ITile GetTile(IBBox bbox);
        
        ITile GetTile(Cell cell);
		
        Size MetaBuffer { get; }
		
        string ContentType { get; }
		
        string Name { get; set; }
		
        byte[] Render(ITile tile);
		
        Resolutions Resolutions { get; }
		
        Size Size { get; }
		
        bool DelayedLoading { get; }

        /// <summary>
        /// The size of the map data
        /// </summary>
        IBBox MapBBox { get; }
	}
}
