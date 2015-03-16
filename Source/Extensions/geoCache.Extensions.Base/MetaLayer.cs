//
// File: MetaLayer.cs
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

using System;
using System.Drawing;
using GeoCache.Core;

namespace GeoCache.Extensions.Base
{
	public abstract class MetaLayer : Layer
	{
		#region python
		/*
    __slots__ = ('metaTile', 'metaSize', 'metaBuffer')
		 */
		#endregion

		private readonly bool MetaTile;
		private Size MetaSize;

		#region python
		/*
    def __init__ (self, name, metatile = "", metasize = (5,5),
                              metabuffer = (10,10), **kwargs):
        Layer.__init__(self, name, **kwargs)
        self.metaTile    = metatile.lower() in ("true", "yes", "1")
        if isinstance(metasize, str):
            metasize = map(int,metasize.split(","))
        if isinstance(metabuffer, str):
            metabuffer = map(int, metabuffer.split(","))
            if len(metabuffer) == 1:
                metabuffer = (metabuffer[0], metabuffer[0])
        self.metaSize    = metasize
        self.metaBuffer  = metabuffer
		*/
		#endregion

		protected MetaLayer() : this(string.Empty)
		{
		}

		protected MetaLayer(string name)
			: this(name, false, new Size(5, 5), new Size(10, 10))
		{
		}

		protected MetaLayer(string name, bool metaTile, Size metaSize, Size metaBuffer)
			: base(name)
		{
			MetaTile = metaTile;
			MetaSize = metaSize;
			MetaBuffer = metaBuffer;
		}

		#region python
		/*
    def getMetaSize (self, z):
        if not self.metaTile: return (1,1)
        maxcol, maxrow = self.grid(z)
        return ( min(self.metaSize[0], int(maxcol + 1)), 
                 min(self.metaSize[1], int(maxrow + 1)) )
		*/
		#endregion

		public override Size GetMetaSize(int z)
		{
			if (!MetaTile)
				return new Size(1, 1);
			SizeD size = Grid(z);
			return new Size
			       	{
			       		//TODO: Verify python-conversion...
			       		Width = Math.Min(MetaSize.Width, Convert.ToInt32(size.Width + 1)),
			       		Height = Math.Min(MetaSize.Height, Convert.ToInt32(size.Height + 1))
			       	};
		}

		public MetaTile GetMetaTile(ITile tile)
		{
			int x = Convert.ToInt32(tile.X / MetaSize.Width);
			int y = Convert.ToInt32(tile.X / MetaSize.Height);
			return new MetaTile(this, x, y, tile.Z);
		}

		public byte[] RenderMetaTile(MetaTile metatile, ITile tile)
		{
			byte[] data = RenderTile(metatile);
			Image image = ImageHelper.Open(data);
			Size metaSize = GetMetaSize(metatile.Z);
			int metaHeight = metaSize.Height * Size.Height + 2 * MetaBuffer.Height;
			for (int i = 0; i < metaSize.Width; i++)
				for (int j = 0; j < metaSize.Height; i++)
				{
					int minX = i * Size.Width + MetaBuffer.Width;
					int maxX = minX + Size.Width;
					// this next calculation is because image origin is (top, left)
					int maxY = metaHeight - (j * Size.Height + MetaBuffer.Height);
					int minY = maxY - Size.Height;
					Image subImage = image.Crop(minX, minY, maxX, maxY);
					byte[] subdata = subImage.GetBytes();
					double x = metatile.X * MetaSize.Width + i;
					double y = metatile.Y * MetaSize.Height + i;

					var subtile = new Tile(this, x, y, metatile.Z);
					if (!string.IsNullOrEmpty(WatermarkImage))
						subdata = Watermark(subdata).GetBytes();
					Cache.Set(subtile, subdata);
					if (x == tile.X && y == tile.Y)
						return subdata;
				}
			return null;
		}

		public override byte[] Render(ITile tile)
		{
			if (MetaTile)
			{
				MetaTile metatile = GetMetaTile(tile);
				try
				{
					Cache.Lock(metatile);
					return Cache.Get(tile) ?? RenderMetaTile(metatile, tile);
				}
				finally
				{
					Cache.Unlock(metatile);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(WatermarkImage))
					return Watermark(RenderTile(tile)).GetBytes();
				return RenderTile(tile);
			}
		}

		public Image Watermark(byte[] image)
		{
			throw new NotImplementedException();
		}

		public Image Watermark(Image image)
		{
			throw new NotImplementedException();
		}

		#region python
		/*
    def watermark (self, img):
        import StringIO, Image, ImageEnhance
        tileImage = Image.open( StringIO.StringIO(img) )
        wmark = Image.open(self.watermarkimage)
        assert self.watermarkopacity >= 0 and self.watermarkopacity <= 1
        if wmark.mode != 'RGBA':
            wmark = wmark.convert('RGBA')
        else:
            wmark = wmark.copy()
        alpha = wmark.split()[3]
        alpha = ImageEnhance.Brightness(alpha).enhance(self.watermarkopacity)
        wmark.putalpha(alpha)
        if tileImage.mode != 'RGBA':
            tileImage = tileImage.convert('RGBA')
        watermarkedImage = Image.new('RGBA', tileImage.size, (0,0,0,0))
        watermarkedImage.paste(wmark, (0,0))
        watermarkedImage = Image.composite(watermarkedImage, tileImage, watermarkedImage)
        buffer = StringIO.StringIO()
        if watermarkedImage.info.has_key('transparency'):
            watermarkedImage.save(buffer, self.extension, transparency=compositeImage.info['transparency'])
        else:
            watermarkedImage.save(buffer, self.extension)
        buffer.seek(0)
        return buffer.read()
		 */
		#endregion

		#region python
		/*
    def render (self, tile):
        if self.metaTile:
            metatile = self.getMetaTile(tile)
            try:
                self.cache.lock(metatile)
                image = self.cache.get(tile)
                if not image:
                    image = self.renderMetaTile(metatile, tile)
            finally:
                self.cache.unlock(metatile)
            return image
        else:
            if self.watermarkimage:
                return self.watermark(self.renderTile(tile))
            else:
                return self.renderTile(tile)
		*/
		#endregion

		#region python
		/*
    def renderMetaTile (self, metatile, tile):
        import StringIO, Image

        data = self.renderTile(metatile)
        image = Image.open( StringIO.StringIO(data) )

        metaCols, metaRows = self.getMetaSize(metatile.z)
        metaHeight = metaRows * self.size[1] + 2 * self.metaBuffer[1]
        for i in range(metaCols):
            for j in range(metaRows):
                minx = i * self.size[0] + self.metaBuffer[0]
                maxx = minx + self.size[0]
                ### this next calculation is because image origin is (top,left)
                maxy = metaHeight - (j * self.size[1] + self.metaBuffer[1])
                miny = maxy - self.size[1]
                subimage = image.crop((minx, miny, maxx, maxy))
                buffer = StringIO.StringIO()
                if image.info.has_key('transparency'): 
                    subimage.save(buffer, self.extension, transparency=image.info['transparency'])
                else:
                    subimage.save(buffer, self.extension)
                buffer.seek(0)
                subdata = buffer.read()
                x = metatile.x * self.metaSize[0] + i
                y = metatile.y * self.metaSize[1] + j
                subtile = Tile( self, x, y, metatile.z )
                if self.watermarkimage:
                    subdata = self.watermark(subdata)
                self.cache.set( subtile, subdata )
                if x == tile.x and y == tile.y:
                    tile.data = subdata

        return tile.data
		*/
		#endregion

		#region python
		/*
    def getMetaTile (self, tile):
        x = int(tile.x / self.metaSize[0])
        y = int(tile.y / self.metaSize[1])
        return MetaTile(self, x, y, tile.z) 
		*/
		#endregion
	}
}