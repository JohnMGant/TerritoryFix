using System.Drawing;
using System.Text;
using Newtonsoft.Json;

public class MapboxUtility
{

    private readonly string apiKey;
    private readonly string styleName;
    private readonly string colorHex;
    private readonly decimal opacityRatio;
    private readonly int width;
    private readonly int height;

    public MapboxUtility(string apiKey, string styleName, string colorHex, decimal opacityRatio, int width, int height)
    {
        this.apiKey = apiKey;
        this.styleName = styleName;
        this.colorHex = colorHex;
        this.opacityRatio = opacityRatio;
        this.width = width;
        this.height = height;
    }

    public string GetMapboxUrl(IEnumerable<Coordinate> coordinates)
    {
        //https://docs.mapbox.com/api/maps/static-images/
        string baseEndpoint = "https://api.mapbox.com/styles/v1/mapbox";
        string geoJsonString = JsonConvert.SerializeObject(GeoJsonUtility.WrapGeoJsonWithStyle(GeoJsonUtility.GetGeoJsonPolygon(new[] { coordinates }), colorHex, opacityRatio));
        string geoJson = Uri.EscapeDataString(geoJsonString);
        return $"{baseEndpoint}/{styleName}/static/geojson({geoJson})/auto/{width}x{height}?padding=10&access_token={apiKey}";
    }

}