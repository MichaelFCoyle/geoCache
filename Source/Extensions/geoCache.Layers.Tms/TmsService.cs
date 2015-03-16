using System;
using System.Collections.Specialized;
using GeoCache.Core;
using GeoCache.Core.Web;

namespace GeoCache.Layers.Tms
{
    public class TmsService : IService
    {
        public TmsService()
        {

        }

        public TmsService(ITileRenderer tileRenderer, ILayerContainer layerContainer)
        {
            TileRenderer = tileRenderer;
            LayerContainer = layerContainer;
        }

        public void ProcessRequest(IHttpContext context)
        {
            var requestParams = context.Request.Params;

            TileRenderer.RenderTile(context.Response, GetMap(requestParams), false);
        }

        ITile GetMap(NameValueCollection param)
        {
            var bbox = new BBox(param["bbox"]);
            var layer = GetLayer(param["layers"]);
            var tile = layer.GetTile(bbox);
            if (tile == null)
                throw new Exception(string.Format("couldn't calculate tile index for layer {0} from ({1})", layer.Name, bbox));
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
