using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IsochronePoc.Application.TravelTimeFilterFastApi
{
    public class TravelTimeFilterFastApiClient : ITravelTimeFilterFastApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _configuration;

        public TravelTimeFilterFastApiClient(HttpClient httpClient, ApiConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress = new Uri(_configuration.TravelTimeQueryUri + "/fast");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Application-Id", _configuration.ApplicationId);
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration.ApiKey);

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> Search(string postcode, decimal latitude, decimal longitude, IList<Venue> locations)
        {            //https://docs.traveltimeplatform.com/reference/time-filter

            // The departure time in an ISO format.
            // var departureTime = "2018-07-04T09:00:00-0500";
            //var departureTime = DateTime.Now.ToString("o");
            var departureTime = new DateTimeOffset(DateTime.UtcNow);

            // Travel time in seconds. We want 1 hour travel time so it is 60 minutes x 60 seconds.
            var travelTime = 60 * 60;

            var arrivalTime = new DateTimeOffset(DateTime.UtcNow.AddSeconds(travelTime));

            //The start location needs to be included in the request "locations"
            locations.Insert(0, new Venue
            {
                Id = 0,
                Postcode = postcode,
                Latitude = latitude,
                Longitude = longitude
            });

            var travelType = "driving+public_transport";
            //public_transport, driving, driving+public_transport

            var searchRequest = new TravelTimeFilterFastSearchRequest
            {
                Locations = locations
                    .Select(l => new Location
                    {
                        Id = l.Id.ToString(),
                        Coords = new Coords
                        {
                            Lat = Convert.ToDouble(l.Latitude),
                            Lng = Convert.ToDouble(l.Longitude)
                        }
                    }).ToArray(),
                ArrivalSearches = new ArrivalSearches
                    {
                        ManyToOne = new ManyToOne[0],
                        OneToMany = new OneToMany[]
                        {
                            new OneToMany
                            {
                                Id = "search_from_origin",
                                DepartureLocationId = "0", //The first location is the origin
                                ArrivalLocationIds = locations
                                    .Where(l => l.Id != 0)
                                    .Select(l => l.Id.ToString()).ToArray(),
                                Transportation = new Transportation
                                {
                                    Type = travelType
                                },
                                ArrivalTimePeriod = "weekday_morning",
                                TravelTime = travelTime,
                                Properties = new []
                                {
                                    "travel_time",
                                    //"distance",
                                    //"distance_breakdown",
                                    "fares",
                                    //"route"
                                },
                            }
                        }
                }
            };

            var serializerSettings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
                {
                    new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                },
            };

            var jsonRequest = JsonConvert.SerializeObject(searchRequest, serializerSettings);

            Console.WriteLine();
            Console.WriteLine($"Searching for locations near {postcode} at lat/long {latitude}, {longitude}");
            Console.WriteLine();
            Console.WriteLine($"Json request:\r\n{jsonRequest}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            //Note: BaseUri saved above is actually the target Uri
            var response = await _httpClient
                .PostAsync(_configuration.TravelTimeQueryUri + "/fast",
                    new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            stopwatch.Stop();

            Console.WriteLine($"Received {response.StatusCode} in {stopwatch.ElapsedMilliseconds:#,###}ms");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Unsuccessful call to API");
                Console.WriteLine($"Status code {response.StatusCode}, reason {response.ReasonPhrase}");
            }

            Console.WriteLine($"json: {jsonResponse}");
            Console.WriteLine();

            response.EnsureSuccessStatusCode();

            //var response = await responseMessage.Content.ReadAsAsync<PostcodeLookupResponse>();
            //var content = await response.Content.ReadAsAsync<PostcodeLookupResponse>();

            var outputPath = $@".\Data\travel_time_filter_{postcode.Replace(" ", "_")}.json";
            using (var writer = File.CreateText(outputPath))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.WriteRaw(jsonResponse);
            }

            var obj = JsonConvert.DeserializeObject<TravelTimeFilterFastSearchResponse>(jsonResponse, serializerSettings);

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                var result = (TravelTimeFilterFastSearchResponse)serializer.Deserialize(jsonReader,
                    typeof(TravelTimeFilterFastSearchResponse));

                //return result;
            }

            return jsonResponse;
        }
    }
}