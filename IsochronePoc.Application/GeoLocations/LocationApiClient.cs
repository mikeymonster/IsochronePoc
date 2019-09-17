using System;
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

        public async Task<(bool, string)> IsValidPostcode(string postcode)
        {
            try
            {
                var postcodeLookupResultDto = await GetGeoLocationData(postcode);
                return (true, postcodeLookupResultDto.Postcode);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        public async Task<(bool, string)> IsTerminatedPostcode(string postcode)
        {
            try
            {
                var postcodeLookupResultDto = await GetTerminatedPostcodeGeoLocationData(postcode);
                return (true, postcodeLookupResultDto.Postcode);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        public async Task<Location> GetGeoLocationData(string postcode)
        {
            //Postcodes.io Returns 404 for "CV12 wt" so I have removed all special characters to get best possible result
            var lookupUrl = $"{_configuration.PostcodeRetrieverBaseUrl}/{postcode.ToLetterOrDigit()}";

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

        public async Task<Location> GetTerminatedPostcodeGeoLocationData(string postcode)
        {
            var baseUrl = _configuration.PostcodeRetrieverBaseUrl.Substring(0,
                _configuration.PostcodeRetrieverBaseUrl.LastIndexOf("/", StringComparison.Ordinal));
            //Postcodes.io Returns 404 for "CV12 wt" so I have removed all special characters to get best possible result
            var lookupUrl = $"{baseUrl}/terminated_postcodes/{postcode.ToLetterOrDigit()}";
            //terminated_postcodes
            var responseMessage = await _httpClient.GetAsync(lookupUrl);

            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadAsAsync<TerminatedPostCodeLookupResponse>();

            return new Location
            {
                Postcode = response.Result.Postcode,
                Latitude = decimal.Parse(response.Result.Latitude),
                Longitude = decimal.Parse(response.Result.Longitude),
                TerminatedYear = response.Result.TerminatedYear,
                TerminatedMonth = response.Result.TerminatedMonth,
            };
        }
    }
}
