

namespace HttpPlaces;

public class DescriptionJson : InformationJson
{
    public string? xid { get; set; }
    public string? name { get; set; }
    public Address? address { get; set; }
    public string? kinds { get; set; }
    public Info? info { get; set; }
    public Wiki? wikipedia_extracts { get; set; }
}