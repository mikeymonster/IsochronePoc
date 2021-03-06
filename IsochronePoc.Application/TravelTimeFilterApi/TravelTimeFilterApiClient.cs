﻿using System;
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

        public async Task<IList<DistanceSearchResult>> Search(string postcode, decimal latitude, decimal longitude, IList<Application.Location> locations)
        {
            //https://docs.traveltimeplatform.com/reference/time-filter

            // The departure time in an ISO format.
            // var departureTime = "2018-07-04T09:00:00-0500";
            //var departureTime = DateTime.Now.ToString("o");
            var departureTime = new DateTimeOffset(DateTime.UtcNow);

            // Travel time in seconds. We want 1 hour travel time so it is 60 minutes x 60 seconds.
            var travelTime = 60 * 60;

            //The start location needs to be included in the request "locations"
            locations.Insert(0, new Application.Location
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

            //Console.WriteLine();
            //Console.WriteLine($"Searching for locations near {postcode} at lat/long {latitude}, {longitude}");
            //Console.WriteLine();
            //Console.WriteLine($"Json request:\r\n{jsonRequest}");
            //Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            //Note: BaseUri saved above is actually the target Uri
            var response = await _httpClient
                .PostAsync(_configuration.TravelTimeQueryUri,
                    new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            stopwatch.Stop();

            //Console.WriteLine($"Received {response.StatusCode} in {stopwatch.ElapsedMilliseconds:#,###}ms");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Unsuccessful call to API");
                Console.WriteLine($"Status code {response.StatusCode}, reason {response.ReasonPhrase}");
            }

            //Console.WriteLine($"json: {jsonResponse}");
            //Console.WriteLine();

            response.EnsureSuccessStatusCode();

            //var response = await responseMessage.Content.ReadAsAsync<PostcodeLookupResponse>();
            //var content = await response.Content.ReadAsAsync<PostcodeLookupResponse>();

            var outputPath = $@".\Data\travel_time_filter_{postcode.Replace(" ", "_")}.json";
            using (var writer = File.CreateText(outputPath))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.WriteRaw(jsonResponse);
            }

            var responseObject = JsonConvert.DeserializeObject<TravelTimeFilterSearchResponse>(jsonResponse, serializerSettings);
            return await BuildResults(responseObject);

            //using (var stream = await response.Content.ReadAsStreamAsync())
            //using (var reader = new StreamReader(stream))
            //using (var jsonReader = new JsonTextReader(reader))
            //{
            //    var serializer = new JsonSerializer();
            //    var result = (TravelTimeFilterSearchResponse)serializer.Deserialize(jsonReader,
            //        typeof(TravelTimeFilterSearchResponse));

            //    return await BuildResults(result);
            //}
        }

        private Task<IList<DistanceSearchResult>> BuildResults(TravelTimeFilterSearchResponse response)
        {
            var result = new List<DistanceSearchResult>();

            foreach (var item in response.Results)
            {
                foreach (var destination in item.ResponseLocations)
                {
                    try
                    {
                        result.Add(new DistanceSearchResult
                        {
                            Id = int.Parse(destination.Id),
                            Distance = destination.Properties[0].Distance ?? -1,
                            TravelTime = destination.Properties[0].TravelTime,
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            
            return Task.FromResult((IList<DistanceSearchResult>)result);
        }
    }
}
