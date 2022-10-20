using Newtonsoft.Json;

public static class GeoJsonUtility
{
    public static dynamic GetGeoJsonPolygon(IEnumerable<IEnumerable<Coordinate>> coordinates)
    {
        return new
        {
            type = "Polygon",
            coordinates = new[] { coordinates.SelectMany(cset => cset.Select(c => new[] { c.Longitude, c.Latitude })) }
        };
    }

    public static dynamic WrapGeoJsonWithStyle(dynamic geoJson, string colorHex, decimal opacityRatio)
    {
        return new
        {
            type = "FeatureCollection",
            features = new[] {
                new {
                    type = "Feature",
                    geometry = geoJson,
                    properties = new Dictionary<string, dynamic> {
                        {"stroke", $"#{colorHex}"},
                        {"fill", $"#{colorHex}"},
                        {"fill-opacity", opacityRatio}
                    }
                }
            },
        };
    }

}