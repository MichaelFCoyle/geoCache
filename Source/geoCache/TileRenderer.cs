//
// File: TileRenderer.cs
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

using GeoCache.Core;
using GeoCache.Core.Web;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace GeoCache
{
    public class TileRenderer : ITileRenderer
    {
        private readonly object cacheLock = new object();
        private ICache _cache;

        public ICache Cache
        {
            get
            {
                if (_cache == null)
                {
                    lock (cacheLock)
                    {
                        _cache = ObjectManager.GetCache(null, null);
                        if (_cache == null)
                            throw new Exception("Unable to find cache.");
                    }
                }
                return _cache;
            }
            set { _cache = value; }
        }

        #region ITileRenderer Members
        #region python - renderTile
        /*
    def renderTile (self, tile, force = False):
        from warnings import warn
        start = time.time()

        # do more cache checking here: SRS, width, height, layers 

        layer = tile.layer
        image = None
        if not force: image = self.cache.get(tile)
        if not image:
            data = layer.render(tile)
            if (data): image = self.cache.set(tile, data)
            else: raise Exception("Zero length data returned from layer.")
            if layer.debug:
                sys.stderr.write(
                "Cache miss: %s, Tile: x: %s, y: %s, z: %s, time: %s\n" % (
                    tile.bbox(), tile.x, tile.y, tile.z, (time.time() - start)) )
        else:
            if layer.debug:
                sys.stderr.write(
                "Cache hit: %s, Tile: x: %s, y: %s, z: %s, time: %s, debug: %s\n" % (
                    tile.bbox(), tile.x, tile.y, tile.z, (time.time() - start), layer.debug) )
        
        return (layer.mime_type, image)
		 */
        #endregion

        public virtual void RenderTile(IHttpResponse response, ITile tile, bool force)
        {
            //do more cache checking here: SRS, width, height, layers 
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            byte[] image = null;

            if (!tile.Layer.MapBBox.Contains(tile.Bounds.MinX, tile.Bounds.MinY))
                image = Helpers.Create(tile.Size.Width, tile.Size.Height);
            else if (!force)
                image = Cache.Get(tile);
            
            if (image == null)
            {
                if (AbortIfTileNotInCache(response, tile))
                    return;

                //TODO: 
                //Test to serve a small image to the client, with a
                //Response.AppendHeader("Refresh", "10; URL=<current url...>"
                try
                {
                    image = tile.Layer.Render(tile);
                    if (image != null)
                        image = Cache.Set(tile, image);
                    else
                        throw new InvalidDataException("Zero length data returned from layer.");
                }
                finally
                {
                    stopWatch.Stop();
                    Trace.TraceInformation("TileRenderer.RenderTile: Cache miss: {0}, Tile: x: {1}, y: {2}, z: {3}, time: {4}", tile.BBox, tile.X, tile.Y, tile.Z, stopWatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                stopWatch.Stop();
                Trace.TraceInformation("TileRenderer.RenderTile: Cache hit: {0}, Tile: x: {1}, y: {2}, z: {3}, time: {4}", tile.BBox, tile.X, tile.Y, tile.Z, stopWatch.Elapsed.TotalMilliseconds);
            }

            if (image != null)
            {
                response.ContentType = tile.Layer.ContentType;
                response.OutputStream.Write(image, 0, image.Length);
            }
        }
        #endregion

        protected static string GetAppSetting(string setting, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[setting];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        protected virtual bool AbortIfTileNotInCache(IHttpResponse response, ITile tile) => false;

    }
}