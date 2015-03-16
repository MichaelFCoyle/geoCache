<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="geoCache.Demo._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="style.css" type="text/css">
    <style type="text/css">
            html, body, #map {
                margin: 0;
                width: 100%;
                height: 100%;
            }

            #text {
                position: absolute;
                bottom: 1em;
                left: 1em;
                width: 512px;
                z-index: 20000;
                background-color: white;
                padding: 0 0.5em 0.5em 0.5em;
            }
        </style>
    <script src="http://openlayers.org/api/OpenLayers.js" type="text/javascript"></script>
</head>
<body>

    <div id="map"></div> 
    <div id="text">
        <h1 id="title">TrueNorth Geospatial</h1>

        <div id="docs">
            <p>This example uses CSS to define the dimensions of the map element in order to fill the screen.
            When the user resizes the window, the map size changes correspondingly. No scroll bars!</p>
        </div>
    </div>

    <script defer="defer" type="text/javascript">

        var map = new OpenLayers.Map("map",
            {
                maxExtent: new OpenLayers.Bounds(-20037508.34, -20037508.34, 20037508.34, 20037508.34),
                numZoomLevels: 18,
                maxResolution: 156543.0339,
                units: 'm',
                projection: "EPSG:900913",
                displayProjection: new OpenLayers.Projection("EPSG:4326"),
                controls: [
                    new OpenLayers.Control.Navigation(),
                    new OpenLayers.Control.PanZoomBar(),
                    new OpenLayers.Control.LayerSwitcher(),
                    new OpenLayers.Control.Attribution(),
                    new OpenLayers.Control.TouchNavigation()]
        });

        // create sphericalmercator layers
        var osmLayer = new OpenLayers.Layer.OSM("OpenStreetMap");
        map.addLayer(osmLayer);

        map.addLayer(new OpenLayers.Layer.WMS("GeoBC",
                        "geoCache.ashx?",
                        {
                            layers: 'geobc',
                            transparent: true,
                            format: 'image/png',
                            srs: "EPSG:900913"
                        }
            ));

        var centerLL = new OpenLayers.LonLat(-123, 49.3);
        var centerM = centerLL.transform(new OpenLayers.Projection("EPSG:4326"), map.getProjectionObject());
        map.setCenter(centerM, 12);
        //map.zoomToMaxExtent();
        //if (!map.getCenter()) map.zoomToMaxExtent();
    </script>
</body>
</html>
