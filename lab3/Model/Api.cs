using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpPlaces;

public abstract class HttpApi
{
    public abstract Task<InformationJson>? Get(String information, HttpClient httpClient);
}
