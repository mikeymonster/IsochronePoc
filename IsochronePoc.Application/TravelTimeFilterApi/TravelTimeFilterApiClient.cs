using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IsochronePoc.Application.TravelTimeFilterApi
{
    public class TravelTimeFilterApiClient : ITravelTimeFilterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _configuration;

        public TravelTimeFilterApiClient(HttpClient httpClient, ApiConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress = new Uri(_configuration.TravelTimeQueryUri);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Application-Id", _configuration.ApplicationId);
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration.ApiKey);

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> Search(string postcode, decimal latitude, decimal longitude, IList<Venue> locations)
        {
            /*
                Host: api.traveltimeapp.com
                Content-Type: application/json
                Accept: application/json
                X-Application-Id: ...
                X-Api-Key: ...

                {
                  "locations": [
                    {
                      "id": "London center",
                      "coords": {
                        "lat": 51.508930,
                        "lng": -0.131387
                      }
                    },
                    {
                      "id": "Hyde Park",
                      "coords": {
                        "lat": 51.508824,
                        "lng": -0.167093
                      }
                    },
                    {
                      "id": "ZSL London Zoo",
                      "coords": {
                        "lat": 51.536067,
                        "lng": -0.153596
                      }
                    }
                  ],
                  "departure_searches": [
                    {
                      "id": "forward search example",
                      "departure_location_id": "London center",
                      "arrival_location_ids": [
                        "Hyde Park",
                        "ZSL London Zoo"
                      ],
                      "transportation": {
                        "type": "bus"
                      },
                      "departure_time": "2019-09-11T08:00:00Z",
                      "travel_time": 1800,
                      "properties": [
                        "travel_time"
                      ],
                      "range": {
                        "enabled": true,
                        "max_results": 3,
                        "width": 600
                      }
                    }
                  ],
                  "arrival_searches": [
                    {
                      "id": "backward search example",
                      "departure_location_ids": [
                        "Hyde Park",
                        "ZSL London Zoo"
                      ],
                      "arrival_location_id": "London center",
                      "transportation": {
                        "type": "public_transport"
                      },
                      "arrival_time": "2019-09-11T08:00:00Z",
                      "travel_time": 1900,
                      "properties": [
                        "travel_time",
                        "distance",
                        "distance_breakdown",
                        "fares"
                      ]
                    }
                  ]
                }
            */

            //https://docs.traveltimeplatform.com/reference/time-filter

            // The departure time in an ISO format.
            // var departureTime = "2018-07-04T09:00:00-0500";
            //var departureTime = DateTime.Now.ToString("o");
            //var departureTime = new DateTimeOffset(DateTime.UtcNow);
            var departureTime = new DateTimeOffset(DateTime.UtcNow.AddDays(1));

            // Travel time in seconds. We want 1 hour travel time so it is 60 minutes x 60 seconds.
            var travelTime = 60 * 60;

            //var locationsForSearch = new Location[locations.Count];
            //for (var i = 0; i < locations.Count; i++)
            //{
            //    locationsForSearch[i] = new Location
            //    {
            //        Id = locations[i].Id.ToString(),
            //        Coords = new Coords
            //        {
            //            Lat = Convert.ToDouble(locations[i].Latitude),
            //            Lng = Convert.ToDouble(locations[i].Longitude)

            //        }
            //    };
            //}

            //var locs = locations.Select(l => new Location
            //{
            //    Id = l.Id.ToString(),
            //    Coords = new Coords
            //    {
            //        Lat = Convert.ToDouble(l.Latitude),
            //        Lng = Convert.ToDouble(l.Longitude)
            //    }
            //}).ToArray();

            //if(locs.Any(l => string.IsNullOrWhiteSpace(l.Id)))
            //{

            //}

            //The start location needs to be included in the request "locations"
            locations.Insert(0, new Venue
            {
                Id = 0,
                Postcode = postcode,
                Latitude = latitude,
                Longitude = longitude
            });

            var travelType = "driving";
            //Possible values:
            //cycling
            //driving
            //driving+train
            //cycling+public_transport
            //public_transport
            //walking
            //coach
            //bus
            //train
            //ferry
            //cycling+ferry
            //driving+ferry

            var searchRequest = new TravelTimeFilterSearchRequest
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
                DepartureSearches = new[]
                {
                    new DepartureSearch
                    {
                        Id = "search_from_origin",
                        DepartureLocationId = "0", //The first location is the origin
                        Transportation = new Transportation
                        {
                            Type = travelType
                        },
                        DepartureTime = departureTime,
                        TravelTime = travelTime,
                        //ArrivalLocationIds = new string[0],
                        ArrivalLocationIds = locations
                            .Where(l => l.Id != 0)
                            .Select(l => l.Id.ToString()).ToArray(),
                        Properties = new []
                        {
                            "travel_time",
                            "distance",
                            //"distance_breakdown",
                            //"fares",
                            //"route"
                        },
                        Range = new Range
                        {
                            Enabled = true,
                            MaxResults = 5,
                            Width = 600
                        }
                    }
                },
                ArrivalSearches = new ArrivalSearch[0]
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
                .PostAsync(_configuration.TravelTimeQueryUri,
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

            var obj = JsonConvert.DeserializeObject<TravelTimeFilterSearchResponse>(jsonResponse, serializerSettings);

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                var result = (TravelTimeFilterSearchResponse)serializer.Deserialize(jsonReader,
                    typeof(TravelTimeFilterSearchResponse));

                //return result;
            }

            return jsonResponse;
        }
    }
}
