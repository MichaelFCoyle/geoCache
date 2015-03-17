//
// File: ImageResponseValidator.cs
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

using System;
using GeoCache.Layers.Wms;
using System.Drawing;
using System.IO;

namespace GeoCache.Layers.Wms.ImageResponseValidators
{
	public class ImageResponseValidator : IResponseValidator
	{
		public ImageResponseValidator()
		{
			ValidateImageData = true;
		}

		protected bool ValidateImageData { get; set; }

		public bool Validate(byte[] data)
		{
			using (var stream = new MemoryStream(data))
			{
				Image image;
				try 
                { 
                    image = Image.FromStream(stream, true, ValidateImageData); 
                }
				catch
				{
					if (ThrowIfNotImage)
						throw new InvalidDataException("Service failed to return image-data. Data:" + new System.Text.UTF7Encoding().GetString(data));
					else
						return false;
				}
				return ValidateImage(image);
			}
		}

		public virtual bool ValidateImage(Image image)
		{
			return true;
		}

		public bool ThrowIfNotImage { get; set; }

		public string ThrowIfNotImageString
		{
			set
			{
				bool temp;
				if (Boolean.TryParse(value, out temp))
					ThrowIfNotImage = temp;
			}
		}
	}
}
