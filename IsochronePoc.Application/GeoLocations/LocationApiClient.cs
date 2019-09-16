using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IsochronePoc.Application.Extensions;

namespace IsochronePoc.Application.GeoLocations
{
    public class LocationApiClient : ILocationApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _configuration;

        public LocationApiClient(HttpClient httpClient, ApiConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Location> GetGeoLocationData(string postCode)
        {
            //Postcodes.io Returns 404 for "CV12 wt" so I have removed all special characters to get best possible result
            var lookupUrl = $"{_configuration.PostcodeRetrieverBaseUrl}/{postCode.ToLetterOrDigit()}";

            var responseMessage = await _httpClient.GetAsync(lookupUrl);

            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadAsAsync<PostCodeLookupResponse>();

            return new Location
            {
                Postcode = response.Result.Postcode,
                Latitude = decimal.Parse(response.Result.Latitude),
                Longitude = decimal.Parse(response.Result.Longitude),
            };
        }
    }
}
