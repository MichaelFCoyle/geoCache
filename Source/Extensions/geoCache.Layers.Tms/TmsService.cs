using System;
using System.IO;
using GeoCache.Core;
using GeoCache.Core.Web;

namespace GeoCache.Layers.Tms
{
    public class OsmService : IService
    {
        public OsmService()
        {

        }

        public OsmService(ITileRenderer tileRenderer, ILayerContainer layerContainer)
        {
            TileRenderer = tileRenderer;
            LayerContainer = layerContainer;
        }

        public void ProcessRequest(IHttpContext context)
        {
            string[] parts = context.Request.FilePath.Split('/');
            if (parts.Length != 6 ||parts[1]!="Tiles")
                throw new InvalidDataException("Zero length data returned from layer.");

            TileRenderer.RenderTile(context.Response, GetMap(parts), false);
        }

        ITile GetMap(string[] param)
        {
            string l= param[2];
            int x, y, z;
            Int32.TryParse(param[3], out z);
            Int32.TryParse(param[4], out x);

            Int32.TryParse(param[5].Replace(".png",""), out y);

            Cell cell = new Cell(x, y, z);

            //var bbox = new BBox(param["bbox"]);
            var layer = GetLayer(l);
            var tile = layer.GetTile(cell);
            if (tile == null)
                throw new Exception(string.Format("couldn't calculate tile index for layer {0} from ({1})", layer.Name, cell));
            return tile;
        }

        public ILayer GetLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
                throw new ArgumentNullException("layerName");

            if (LayerContainer == null)
                throw new NotSupportedException("Unable to get layer when LayerContainer is null");

            if (!LayerContainer.Layers.ContainsKey(layerName))
                throw new Exception(string.Format("The requested layer ({0}) does not exist.", layerName)); //Available layers are: \n ", (layername, "\n * ".join(self.service.layers.keys()))) 

            return LayerContainer.Layers[layerName];
        }

        protected ITileRenderer TileRenderer { get; set; }

        protected ILayerContainer LayerContainer { get; set; }
    }
}
