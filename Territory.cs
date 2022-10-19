public record Territory
{
    public int Id { get; init; }
    public string? Locality { get; init; }
    public string? Number { get; init; }
    public string? Description { get; init; }
    public Coordinate? CenterPoint { get; init; }
    public IEnumerable<Coordinate>? Boundary { get; init; }
}