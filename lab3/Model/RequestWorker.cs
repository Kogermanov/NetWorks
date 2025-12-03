using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpPlaces;
public class RequestWorker
{
    private HttpClient httpClient = new HttpClient();
    private WeatherApi weatherApi;
    private DescriptionApi descriptionApi;
    private InterestingPlacesApi interestingPlacesApi;
    private LocationApi locationApi;

    public RequestWorker() 
    {
        weatherApi = new WeatherApi();
        descriptionApi = new DescriptionApi();
        interestingPlacesApi = new InterestingPlacesApi();
        locationApi = new LocationApi();
    }
    
    public async Task<InformationJson>? GetLocations(string nameLocation)
    {
        return await locationApi.Get(nameLocation, httpClient);
    }

    public async Task<InformationJson>? GetWeather(double lat, double lon)
    {
        return await weatherApi.Get(lon.ToString() + ";" + lat.ToString(), httpClient);
    }

    public async Task<InformationJson>? GetInterestingPlaces(double lat, double lon)
    {
        return await interestingPlacesApi.Get(lon.ToString() + ";" + lat.ToString(), httpClient);
    }

    public async Task<InformationJson>? GetDescription(string id)
    {
        return await descriptionApi.Get(id, httpClient);
    }
}
