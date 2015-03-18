//
// File: Layer.cs
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
using System.Diagnostics;
using System.Drawing;
using GeoCache.Core;

namespace GeoCache.Extensions.Base
{
    public abstract class Layer : ILayer
    {
        private IBBox _bBox = new BBox(-180, -90, 180, 90);
        private IBBox _mapBBox = new BBox(-180, -90, 180, 90);
        private bool _delayedLoading = true;
        private string _extension = "png";
        private ExtentType _extentType = ExtentType.Loose;
        private string _layers = string.Empty;
        protected int _levels;
        private Resolutions _resolutions;
        private string _srs = "EPSG:32633";
        private object _units = "m";

        protected Layer()
        {
            DelayedLoading = true;
        }

        protected Layer(string name) :
            this(
                name, 
                null, 
                new BBox(-180, -90, 180, 90),
                "EPSG:4326", 
                string.Empty, 
                null,
                new Size(256, 256), 
                20, 
                null,
                "png", 
                null, 
                null,
                null, 
                0.2F,
                ExtentType.Strict, 
                null, 
                string.Empty)
        {
        }

        protected Layer(
                string name, 
                string layers, 
                IBBox bbox,
                string srs, 
                string description, 
                double? maxResolution,
                Size size, 
                int levels, 
                Resolutions resolutions,
                string extension, 
                string mimeType, 
                ICache cache,
                string watermarkImage, 
                float? watermarkOpacity,
                ExtentType extentType, 
                object units, 
                string tmsType)
        {
            Name = name;
            Description = description;
            if (string.IsNullOrEmpty(layers))
                Layers = name;

            BBox = bbox;
            Size = size;
            Units = units;
            Srs = srs;
            Extension = extension;
            ContentType = string.IsNullOrEmpty(mimeType) ? Format : mimeType;
            Cache = cache;
            //Debug = debug;
            ExtentType = extentType;
            TmsType = tmsType;
            _levels = levels;
            if (resolutions != null)
                Resolutions = resolutions;
            else
                MaxResolution = maxResolution;
            WatermarkImage = watermarkImage;
            WatermarkOpacity = watermarkOpacity;
        }

        #region properties

        #region python __init__ and slots
        /*
def __init__ (self, name, layers = None, bbox = (-180, -90, 180, 90),
                        srs  = "EPSG:4326", description = "", maxresolution = None,
                        size = (256, 256), levels = 20, resolutions = None,
                        extension = "png", mime_type = None, cache = None,  debug = True, 
                        watermarkimage = None, watermarkopacity = 0.2,
                        extent_type = "strict", units = None, tms_type = "" ):
        self.name   = name
        self.description = description
        self.layers = layers or name
        if isinstance(bbox, str): bbox = map(float,bbox.split(","))
        self.bbox = bbox
        if isinstance(size, str): size = map(int,size.split(","))
        self.size = size
        self.units = units
        self.srs  = srs
        if extension.lower() == 'jpg': extension = 'jpeg' # MIME
        self.extension = extension.lower()
        self.mime_type = mime_type or self.format() 
        if isinstance(debug, str):
            debug = debug.lower() not in ("false", "off", "no", "0")
        self.cache = cache
        self.debug = debug
        self.extent_type = extent_type
        self.tms_type = tms_type
        if resolutions:
            if isinstance(resolutions, str):
                resolutions = map(float,resolutions.split(","))
            self.resolutions = resolutions
        else:
            maxRes = None
            if not maxresolution:
                width  = bbox[2] - bbox[0]
                height = bbox[3] - bbox[1]
                if width >= height:
                    aspect = int( float(width) / height + .5 ) # round up
                    maxRes = float(width) / (size[0] * aspect)
                else:
                    aspect = int( float(height) / width + .5 ) # round up
                    maxRes = float(height) / (size[1] * aspect)
            else:
                maxRes = float(maxresolution)
            self.resolutions = [maxRes / 2 ** i for i in range(int(levels))]
        self.watermarkimage = watermarkimage
        self.watermarkopacity = float(watermarkopacity)
		 */

        /*
        __slots__ = ( "name", "layers", "bbox", 
                      "size", "resolutions", "extension", "srs",
                      "cache", "debug", "description", 
                      "watermarkimage", "watermarkopacity",
                      "extent_type", "tms_type", "units", "mime_type")		 
         */
        #endregion

        public string Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }

        /// <summary>
        /// Transformation - EPSG:4326, EPSG:32633 etc.
        /// </summary>
        public string Srs
        {
            get { return _srs; }
            set { _srs = value; }
        }

        public ICache Cache { get; set; }

        public string Description { get; set; }
        
        public string WatermarkImage { get; set; }
        
        public float? WatermarkOpacity { get; set; }

        /// <summary>
        /// Tms (Tile Map Service?) type Eg google
        /// </summary>
        public object TmsType { get; set; }

        public object Units
        {
            get { return _units; }
            set { _units = value; }
        }

        public double? MaxResolution { get; set; }

        public string Name { get; set; }

        public IBBox BBox
        {
            get { return _bBox; }
            set { _bBox = value; }
        }

        public Size Size { get; set; }

        public Resolutions Resolutions
        {
            get
            {
                if (_resolutions == null)
                {
                    _resolutions = (MaxResolution != null)
                                    ? Resolutions.Get(_levels, (double)MaxResolution)
                                    : BBox.GetResolutions(_levels, Size);
                }
                return _resolutions;
            }
            set { _resolutions = value; }
        }

        public string Extension
        {
            get { return _extension; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                _extension = value.Equals("jpg", StringComparison.OrdinalIgnoreCase) ? "jpeg" : value.ToLower();
            }
        }

        public ExtentType ExtentType
        {
            get { return _extentType; }
            set { _extentType = value; }
        }

        public string ContentType { get; set; }

        public Size MetaBuffer { get; set; }

        public bool DelayedLoading
        {
            get { return _delayedLoading; }
            set { _delayedLoading = value; }
        }

        #endregion

        public ITile GetTile(IBBox bbox)
        {
            return new Tile(this, GetCell(bbox));
        }

        public ITile GetTile(Cell cell)
        {
            return new Tile(this, cell);
        }

        public string Format
        {
            get { return "image/" + Extension; }
        }

        public virtual byte[] Render(ITile tile)
        {
            return RenderTile(tile);
        }

        public abstract Size GetMetaSize(int z);

        /// <summary>
        /// The bounding box where there is data
        /// This can be used to "reject" a request without caching or processing it
        /// </summary>
        public IBBox MapBBox
        {
            get { return _mapBBox; }
            set { _mapBBox = value; }
        }

        #region python
        /*
		def renderTile (self, tile):
			# To be implemented by subclasses
			pass 
		 */
        #endregion

        #region python
        /*
		def grid (self, z):
			width  = (self.bbox[2] - self.bbox[0]) / (self.resolutions[z] * self.size[0])
			height = (self.bbox[3] - self.bbox[1]) / (self.resolutions[z] * self.size[1])
			return (width, height)
		 */
        #endregion

        #region python
        /*
    def getLevel (self, res, size = [256, 256]):
        max_diff = res / max(size[0], size[1])
        z = None
        for i in range(len(self.resolutions)):
            if abs( self.resolutions[i] - res ) < max_diff:
                res = self.resolutions[i]
                z = i
                break
        if z is None:
            raise TileCacheException("can't find resolution index for %f. Available resolutions are: \n%s" % (res, self.resolutions))
        return z
		 */
        #endregion

        public int GetLevel(double res)
        {
            return GetLevel(res, new Size(256, 256));
        }

        public int GetLevel(double res, Size size)
        {
            double maxDiff = res / Math.Max(size.Width, size.Height);
            for (int i = 0; i < Resolutions.Count; i++)
            {
                if (Math.Abs(Resolutions[i] - res) < maxDiff)
                {
                    //res = this.Resolutions[i];
                    return i;
                }
            }
            throw new Exception(string.Format("can't find resolution index for {0}. Available resolutions are: \n{1}", res, Resolutions));
        }

        public Cell GetCell(IBBox bbox)
        {
            return GetCell(bbox, true);
        }

        private Cell GetCell(double minX, double minY, double maxX, double maxY, bool exact)
        {
            return GetCell(new BBox(minX, minY, maxX, maxY), exact);
        }

        public Cell GetCell(IBBox bbox, bool exact)
        {
            if (!BBox.Contains(bbox.MinX, bbox.MinY))
            {
                string message = string.Format("Lower left corner ({0}, {1}) is outside layer bounds {2}.", bbox.MinX, bbox.MinY, BBox);
                const bool FORCE_STRICT = false;
                if (FORCE_STRICT || exact && ExtentType == ExtentType.Strict)
                    throw new ArgumentOutOfRangeException("bbox",
                                                          message +
                                                          "\nTo remove this condition, set extent-type to loose in your configuration.");
                Trace.TraceInformation("Layer.GetCell: {0}", message);
            }

            int z = GetLevel(bbox.GetResolution(Size), Size);
            double res = Resolutions[z];

            double x0 = (bbox.MinX - BBox.MinX) / (res * Size.Width);
            double y0 = (bbox.MinY - BBox.MinY) / (res * Size.Height);

            int x = Convert.ToInt32(Math.Round(x0));
            int y = Convert.ToInt32(Math.Round(y0));

            double tileX = ((x * res * Size.Width) + BBox.MinX);
            double tileY = ((y * res * Size.Height) + BBox.MinY);

            if (exact)
            {
                if (Math.Abs(bbox.MinX - tileX) / res > 1)
                    throw new Exception(string.Format("Current x value {0} is too far from tile corner x {1}", bbox.MinX, tileX));
                if (Math.Abs(bbox.MinY - tileY) / res > 1)
                    throw new Exception(string.Format("Current y value {0} is too far from tile corner y {1}", bbox.MinX, tileX));
            }
            //var quadKey = Microsoft.MapPoint.VirtualEarthTileSystem.TileXYToQuadKey(x, y, z);
            //if (string.IsNullOrEmpty(quadKey))
            //    quadKey = string.Format("<empty x={0}, y={1}, z={2}>", x, y, z);
            //System.Diagnostics.Trace.WriteLine("GetCell - TileXYToQuadKey:" + quadKey, "Layer.GetCell");

            return new Cell(x, y, z);
        }

        public Cell GetClosestCell(int z, double minX, double minY)
        {
            double res = Resolutions[z];
            double maxX = minX + Size.Width * res;
            double maxY = minY + Size.Height * res;
            return GetCell(minX, minY, maxX, maxY, false);
        }

        public SizeD Grid(int z)
        {
            double width = (BBox.MaxX - BBox.MinX) / (Resolutions[z] * Size.Width);
            double height = (BBox.MaxY - BBox.MinY) / (Resolutions[z] * Size.Height);
            return new SizeD(width, height);
        }

        public abstract byte[] RenderTile(ITile tile);

        #region python
        /*
		def render (self, tile):
			return self.renderTile(tile)
		 */
        #endregion

        #region python
        /*
		def format (self):
			return "image/" + self.extension
		 */
        #endregion

        #region python
        /*
		def getTile (self, bbox):
			coord = self.getCell(bbox)
			if not coord: return None
			return Tile(self, *coord)
		 */
        #endregion

        #region python
        /*
		def getClosestCell (self, z, (minx, miny)):
			res = self.resolutions[z]
			maxx = minx + self.size[0] * res
			maxy = miny + self.size[1] * res
			return self.getCell((minx, miny, maxx, maxy), False)
		*/
        #endregion

        #region python
        /*
		def getCell (self, (minx, miny, maxx, maxy), exact = True):
			if exact and self.extent_type == "strict" and not self.contains((minx, miny)): 
				raise TileCacheException("Lower left corner (%f, %f) is outside layer bounds %s. \nTo remove this condition, set extent_type=loose in your configuration." 
						 % (minx, miny, self.bbox))
				return None

			res = self.getResolution((minx, miny, maxx, maxy))
			x = y = None

			z = self.getLevel(res, self.size)

			res = self.resolutions[z]
			x0 = (minx - self.bbox[0]) / (res * self.size[0])
			y0 = (miny - self.bbox[1]) / (res * self.size[1])
	        
			x = int(round(x0))
			y = int(round(y0))
	        
			tilex = ((x * res * self.size[0]) + self.bbox[0])
			tiley = ((y * res * self.size[1]) + self.bbox[1])
			if exact:
				if (abs(minx - tilex)  / res > 1):
					raise TileCacheException("Current x value %f is too far from tile corner x %f" % (minx, tilex))  
	            
				if (abs(miny - tiley)  / res > 1):
					raise TileCacheException("Current y value %f is too far from tile corner y %f" % (miny, tiley))  
	        
			return (x, y, z)
		 */
        #endregion
    }
}