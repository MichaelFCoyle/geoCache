using System;
using System.Drawing;
using System.Net;
using GeoCache.Core;
using GeoCache.Extensions.Base;

namespace Seed
{
    class OsmLayer : Layer
    {
        public OsmLayer(int levels)
        {
            this._levels = levels;
        }

        public OsmLayer(string url, string name, IBBox mapBounds)
        {
            Url =new Uri( url);
            Name = name;
            MapBBox = mapBounds;
        }

        public Uri Url { get; set; }

        public override Size GetMetaSize(int z)
        {
            throw new NotImplementedException();
        }

        public override byte[] RenderTile(ITile tile)
        {
            string v = string.Format("{0}/{1}/{2}/{3}/{4}.png", Url, Name, tile.Z, tile.X, tile.Y);
            using (WebClient wc = new WebClient())
                return wc.DownloadData(v);
        }
    }
}
