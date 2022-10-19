using System.Drawing;
using System.Text;

public class MapboxUtility
{

    private readonly string apiKey;
    private readonly string styleName;
    private readonly Color pathColor;
    private readonly Color fillColor;
    private readonly Size size;

    public MapboxUtility(string apiKey, string styleName, Color pathColor, Color fillColor, Size size)
    {
        this.apiKey = apiKey;
        this.styleName = styleName;
        this.pathColor = pathColor;
        this.fillColor = fillColor;
        this.size = size;
    }

    public string GetMapboxUrl(IEnumerable<Coordinate> coordinates)
    {
        //https://api.mapbox.com/styles/v1/mapbox/streets-v11/static/path-2+ffaa00+ffaa00-0.2(_seK_ibE_seK_seK_seK_seK)/auto/900x600?access_token=pk.eyJ1Ijoiam1nYW50IiwiYSI6ImNsOHRhandjazAweGEzbmxoMGRkbmphMmsifQ.deVAt4ooCD4vowzMFm5aOQ
        string baseEndpoint = "https://api.mapbox.com/styles/v1/mapbox";
        var (min, max) = GetBoundingBox(coordinates);
        string encodedPath = PolylineUtility.EncodePolyline(coordinates);
        string colorInfo = $"{pathColor.R:X2}{pathColor.G:X2}{pathColor.B:X2}+{fillColor.R:X2}{fillColor.G:X2}{fillColor.B:X2}-{fillColor.A / 255D}";
        return $"{baseEndpoint}/{styleName}/static/path-2+{colorInfo}({Uri.EscapeDataString(encodedPath)})/[{min.Longitude},{min.Latitude},{max.Longitude},{max.Latitude}]/{size.Width}x{size.Height}?padding=10&access_token={apiKey}";
    }

    private (Coordinate min, Coordinate max) GetBoundingBox(IEnumerable<Coordinate> coordinates)
    {
        decimal minLat = decimal.MaxValue;
        decimal minLng = decimal.MaxValue;
        decimal maxLat = decimal.MinValue;
        decimal maxLng = decimal.MinValue;
        foreach (Coordinate coordinate in coordinates)
        {
            if (coordinate.Latitude < minLat)
            {
                minLat = coordinate.Latitude;
            }
            if (coordinate.Longitude < minLng)
            {
                minLng = coordinate.Longitude;
            }
            if (coordinate.Latitude > maxLat)
            {
                maxLat = coordinate.Latitude;
            }
            if (coordinate.Longitude > maxLng)
            {
                maxLng = coordinate.Longitude;
            }
        }
        decimal latRange = maxLat - minLat;
        decimal lngRange = maxLng - minLng;
        decimal buffer = latRange * .025M;
        Coordinate min = new Coordinate { Latitude = minLat - buffer, Longitude = minLng - buffer };
        Coordinate max = new Coordinate { Latitude = maxLat + buffer, Longitude = maxLng + buffer };
        return (min, max);
    }
}