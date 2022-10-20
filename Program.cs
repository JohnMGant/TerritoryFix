using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

const string inputFilePath = "/Users/johngant/territories.json";
const string outputFilePath = "/Users/johngant/territories.csv";
Regex newResidentialPattern = new(@"^[NX]-(?<area>\p{Lu}{2})-(?<number>\d{2})$");
Regex businessPattern = new(@"^B-(?<number>\d{2})$");

IEnumerable<Territory> territories = JsonConvert.DeserializeObject<IEnumerable<Territory>>(File.ReadAllText(inputFilePath)) ?? throw new InvalidOperationException();

string apiKey = args[0];
string styleName = args[1];
string colorHex = args[2];
decimal opacityRatio = decimal.Parse(args[3]);
int width = int.Parse(args[4]);
int height = int.Parse(args[5]);
string imageDirectory = args[6];

MapboxUtility mapbox = new(apiKey, styleName, colorHex, opacityRatio, width, height);

var residentialTerritories =
    (
        from t in territories
        let m = newResidentialPattern.Match(t.Number ?? "")
        where m.Success
        let area = m.Groups["area"].Value
        let number = m.Groups["number"].Value
        orderby area, number
        select new
        {
            Area = area,
            Number = number,
            Territory = t
        }
    ).Select((r, i) => new
    {
        TypeCode = "R",
        TypeName = "Residential",
        Number = i + 1,
        Area = r.Area,
        Notes = r.Territory.Description ?? "",
        Link = r.Territory.Boundary is not null ? mapbox.GetMapboxUrl(r.Territory.Boundary) : "",
        OldNumber = $"{r.Area}-{r.Number}",
        Coordinates = r.Territory.Boundary
    });

var businessTerritories =
    from t in territories
    let m = businessPattern.Match(t.Number ?? "")
    let number = m.Groups["number"].Value
    where m.Success
    orderby number
    select new
    {
        TypeCode = "B",
        TypeName = "Business",
        Number = int.Parse(number),
        Area = "Business",
        Notes = t.Description,
        Link = t.Boundary is not null ? mapbox.GetMapboxUrl(t.Boundary) : "",
        OldNumber = t.Number,
        Coordinates = t.Boundary
    };

var allTerritories = residentialTerritories.Concat(businessTerritories).ToArray();

string q = "\"";
string qcq = "\",\"";
Func<Coordinate, string> formatCoordinates = coords =>
    $"[{coords.Longitude},{coords.Latitude}]";
Func<IEnumerable<Coordinate>?, string> formatPath = coordSet =>
    $"[{string.Join(',', coordSet?.Select(formatCoordinates) ?? Enumerable.Empty<string>())}]";
File.WriteAllText(outputFilePath, $"{q}TypeCode{qcq}TypeName{qcq}Number{qcq}Area{qcq}Link{qcq}Notes{qcq}OldNumber{qcq}Coordinates{q}\n");
File.AppendAllLines(
    outputFilePath,
    allTerritories.Select(t =>
    $"{q}{t.TypeCode}{qcq}{t.TypeName}{qcq}{t.Number}{qcq}{t.Area}{qcq}{t.Link}{qcq}{t.Notes}{qcq}{t.OldNumber}{qcq}{formatPath(t.Coordinates)}{q}"));

if (!Directory.Exists(imageDirectory))
{
    Directory.CreateDirectory(imageDirectory);
}

HttpClient client = new();
foreach (var territory in allTerritories)
{
    string fileName = Path.Combine(imageDirectory, $"{territory.TypeCode}-{territory.Number}.png");
    byte[] imageData = client.GetByteArrayAsync(territory.Link).Result;
    File.WriteAllBytes(fileName, imageData);
}
