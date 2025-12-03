using System.Collections.Generic;

namespace HttpPlaces;

public class PlaceInfo
{
    public PlaceInfo(string name, string city, string country, double lat, double lng, string kind)
    {
        Name = name;
        Kind = kind;
        City = city;
        Country = country;
        Lat = lat;
        Lng = lng;
        InterestingPlaces = new List<string>();
        IdInterestingPlaces = new List<string>();
        Description = new Dictionary<string, string>();
    }

    public void AddWeather(string temp, string pressure, string humidity)
    {
        Temp = temp;
        Pressure = pressure;
        Humidity = humidity;
    }

    public void AddInterestingPlaces(string place, string id)
    {
        InterestingPlaces.Add(place);
        IdInterestingPlaces.Add(id);
    }


    public void AddDescription(string place, string description)
    {
        Description[place] = description;
    }
    public string? Kind { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string? Temp { get; set; }
    public string? Humidity { get; set; }
    public string? Pressure { get; set; }
    public List<string> InterestingPlaces { get; set; }
    public List<string> IdInterestingPlaces { get; set; }
    public Dictionary<string, string> Description { get; set; }

}