//
// File: ImageHelper.cs
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
using System.IO;

namespace GeoCache.Extensions.Base
{
	internal static class ImageHelper
	{
		public static Image Open(byte[] bytes)
		{
			using (MemoryStream ms = new MemoryStream(bytes))
				return Image.FromStream(ms);
		}

		public static Image Crop(this Image src, int minX, int minY, int maxX, int maxY) =>
			Crop(src, Rectangle.FromLTRB(minX, minY, maxX, maxY));

		public static Image Crop(this Image src, Rectangle cropRect)
		{
			Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

			using (Graphics g = Graphics.FromImage(target))
				g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);

			return target;
		}

		public static byte[] GetBytes(this Image image)
		{
			using (var ms = new MemoryStream())
			{
				image.Save(ms, image.RawFormat);
				return ms.ToArray();
			}
		}
	}
}
