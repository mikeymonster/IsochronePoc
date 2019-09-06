using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IsochronePoc.Application
{
    public class TravelTimeApiClient : ITravelTimeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _configuration;

        public TravelTimeApiClient(HttpClient httpClient, ApiConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress = new Uri(_configuration.IsochroneQueryUri);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Application-Id", _configuration.ApplicationId);
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration.ApiKey);

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> Search(string postCode, decimal latitude, decimal longitude)
        {
            /*
            var request = {
	            // This will be a search where we depart a specified time.
	            departure_searches: [ {
		            // The id is useful when you send multiple searches in one request. Since we create only one search it doesn't matter much since we expect only one result.
		            id: "first_location",
		            // The coordinates for the departure location in a hash. { lat: <lat>, lng: <lng> }
		            "coords": coords,
		            // The transportation type for this search. We will be using public transport. 
		            transportation: {
			            type: "public_transport"
		            },
		            // The departure time in an ISO format.
		            departure_time: departureTime,
		            // Travel time in seconds.
		            travel_time: travelTime
	            } ],
	            // We will not be creating any shapes with a specified arrival time in this example so the array is empty.
	            arrival_searches: [] 
            };             
            */

            // The departure time in an ISO format.
            // var departureTime = "2018-07-04T09:00:00-0500";
            var departureTime = DateTime.Now.ToString("o");
            // Travel time in seconds. We want 1 hour travel time so it is 60 minutes x 60 seconds.
            var travelTime = 60 * 60;

            var search = new TravelTimeSearchRequest
            {
                DepartureSearches = new[]
                {
                    new DepartureSearch
                    {
                        Id = "first_location",
                        Coords = new Coords
                        {
                            Lat = Convert.ToDouble(latitude),
                            Lng = Convert.ToDouble(longitude)
                        },
                        Transportation = new Transportation
                        {
                            Type = "public_transport"
                        },
                        DepartureTime = departureTime,
                        TravelTime = travelTime
                    }
                },
                ArrivalSearches = new ArrivalSearch[0]
            };

            var jsonRequest = JsonConvert.SerializeObject(search);

            //Console.WriteLine("Json request:");
            //Console.WriteLine(jsonRequest);
            Console.WriteLine();
            Console.WriteLine($"Searching for {postCode} at lat/long {latitude}, {longitude}");


            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient
                .PostAsync(_configuration.IsochroneQueryUri,
                    new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            stopwatch.Stop();

            Console.WriteLine($"Received {response.StatusCode} in {stopwatch.ElapsedMilliseconds:#,###}ms");

            response.EnsureSuccessStatusCode();

            //var response = await responseMessage.Content.ReadAsAsync<PostCodeLookupResponse>();
            //var content = await response.Content.ReadAsAsync<PostCodeLookupResponse>();
            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
