namespace HttpPlaces;


public class WeatherJson : InformationJson
{
    public Weather[]? weather { get; set; }
    public State? main { get; set; }
    public int visibility { get; set; }
    public Wind? wind { get; set; }
    public Clouds? clouds { get; set; }
    public long dt { get; set; }
    public int timezone { get; set; }
    public long id { get; set; }
    public string? name { get; set; }
}