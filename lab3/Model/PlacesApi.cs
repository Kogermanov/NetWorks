using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HttpPlaces;

public class PlacesApi
{
    RequestWorker worker;
    String status;
    public PlacesApi()
    {
        worker = new RequestWorker();
        status = "Ready to use";
    }

    public async Task<List<PlaceInfo>> GetPlaces(string name)
    {
        var places = new List<PlaceInfo>();
        status = "Search information";
        var json = (PlacesJson) await worker.GetLocations(name);
        if (json.hits != null)
        {
            foreach (var place in json.hits)
            {
                if (place.name != "")
                {
                    var placeInfo = new PlaceInfo(
                        place.name, 
                        place.city, 
                        place.country, 
                        place.point.lat, 
                        place.point.lng,
                        place.osm_value);
                    places.Add(placeInfo);
                }
                
            }
        }
        return places;
    }


    public async Task GetWeather(PlaceInfo placeInfo)
    {
        var json = (WeatherJson) await worker.GetWeather(placeInfo.Lat, placeInfo.Lng);
        if (json != null && json.main != null)
        {
            placeInfo.AddWeather(Math.Round(json.main.temp - 273.15).ToString(),
                                (json.main.pressure * 0.75).ToString(),
                                json.main.humidity.ToString());
        }
    }

    public async Task GetInterestingPlaces(PlaceInfo placeInfo)
    {
        var json = (InterestingPlacesJson?) await worker.GetInterestingPlaces(placeInfo.Lat, placeInfo.Lng);
        if (json != null && json.features != null)
        {
            foreach(var feature in json.features)
            {
                if (feature.properties != null && feature.properties.name != null && feature.properties.name != "")
                {
                    placeInfo.AddInterestingPlaces(feature.properties.name, feature.properties.xid);
                }
            }
        }
    }

    public async Task GetDescription(PlaceInfo placeInfo)
    {
        for (int i = 0; i < placeInfo.InterestingPlaces.Count; i++)
        {
            var json = (DescriptionJson?) await worker.GetDescription(placeInfo.IdInterestingPlaces[i]);
            if (json != null && json.kinds != null)
            {
                placeInfo.Description[placeInfo.InterestingPlaces[i]] = json.kinds;
            }
        }
        status = "Complete";
    }

}