using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

const string inputFilePath = "/Users/johngant/territories.json";
const string outputFilePath = "/Users/johngant/territories.csv";
Regex newResidentialPattern = new(@"^[NX]-(?<area>\p{Lu}{2})-(?<number>\d{2})$");
Regex businessPattern = new(@"^B-(?<number>\d{2})$");

IEnumerable<Territory> territories = JsonConvert.DeserializeObject<IEnumerable<Territory>>(File.ReadAllText(inputFilePath)) ?? throw new InvalidOperationException();

MapboxUtility mapbox = new(
    "[mapbox api key needed]",
    "light-v10",
    Color.FromArgb(0x99, 0x4c, 0xb3),
    Color.FromArgb((int)Math.Round(255 * .2), 0x99, 0x4c, 0xb3),
    new Size(1200, 800));

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

string q = "\"";
string qcq = "\",\"";
Func<Coordinate, string> formatCoordinates = coords =>
    $"[{coords.Longitude},{coords.Latitude}]";
Func<IEnumerable<Coordinate>?, string> formatPath = coordSet =>
    $"[{string.Join(',', coordSet?.Select(formatCoordinates) ?? Enumerable.Empty<string>())}]";
File.WriteAllText(outputFilePath, $"{q}TypeCode{qcq}TypeName{qcq}Number{qcq}Area{qcq}Link{qcq}Notes{qcq}OldNumber{qcq}Coordinates{q}\n");
File.AppendAllLines(
    outputFilePath,
    residentialTerritories.Concat(businessTerritories).Select(t =>
    $"{q}{t.TypeCode}{qcq}{t.TypeName}{qcq}{t.Number}{qcq}{t.Area}{qcq}{t.Link}{qcq}{t.Notes}{qcq}{t.OldNumber}{qcq}{formatPath(t.Coordinates)}{q}"));



