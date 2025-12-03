using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace HttpPlaces;
public class LocationApi : HttpApi
{
    public override async Task<InformationJson> Get(string name, HttpClient httpClient)
    {
        var message = await httpClient.GetAsync($"https://graphhopper.com/api/1/geocode?q={name}&locale=ru&limit=10&reverse=false&debug=false&provider=default&key=bd0739b1-e29d-444d-9859-c5e02f013656");
        string response = await message.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PlacesJson>(response);
    }
}
