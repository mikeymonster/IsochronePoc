using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IsochronePoc.Application
{
    public class GoogleDistanceMatrixApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _configuration;
        private static object _listLocker = new object();

        public GoogleDistanceMatrixApiClient(HttpClient httpClient, ApiConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress = new Uri(_configuration.GoogleMapsApiBaseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            //_httpClient.DefaultRequestHeaders.Add("X-Application-Id", _configuration.ApplicationId);
            //_httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration.ApiKey);

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task Search(Venue origin, IList<Venue> venues)
        {
            const int batchSize = 100; //Max client-side elements: 100

            //var batches = new List<IList<Venue>>();
            var batches = new Dictionary<int, IList<Venue>>();
            var items = venues;
            var batchNo = 0;
            while (items.Any())
            {
                var batch = items.Take(batchSize);
                batches.Add(++batchNo, batch.ToList());
                items = items.Skip(batchSize).ToList();
            }

            var results = new List<GoogleDistanceMatrixResponse>();

            var stopwatch = Stopwatch.StartNew();

            Parallel.ForEach(batches, (batch) =>
            {
                //foreach (var batch in batches)
                {
                    Console.WriteLine($"Have batch {batch.Key} of size {batch.Value.Count}");

                    //for (var i = 0; i < batch.Count; i++)
                    //{
                    //    Debug.WriteLine($"{i} {batch[i].Postcode}");
                    //}

                    //TODO: Get a result and add to a thread-safe collection
                    var response = SearchBatch(origin, batch.Value).GetAwaiter().GetResult();
                    if(response != null && 
                       string.Compare(response.Status, "OK", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        lock (_listLocker)
                        {
                            results.Add(response);
                        }
                    }
                }
            });

            stopwatch.Stop();
            Console.WriteLine($"Ran {batches.Count} batches of {batchSize} ({venues.Count()}) in {stopwatch.ElapsedMilliseconds:#,###}ms");
            Console.WriteLine($"Have {results.Count} results");
        }

        private async Task<GoogleDistanceMatrixResponse> SearchBatch(Venue origin, IList<Venue> venues)
        {
            //https://developers.google.com/maps/documentation/distance-matrix/intro

            //Call:
            //http://maps.googleapis.com/maps/api/distancematrix/outputFormat?parameters
            //var uri = "distancematrix";
            //NOTE: Assumes api url already has ending /
            var uriBuilder = new StringBuilder($@"{_configuration.GoogleMapsApiBaseUrl}distancematrix/json?");

            uriBuilder.Append($"origins={origin.Latitude}%2C{origin.Longitude}");
            uriBuilder.Append("&destinations=");

            for (int i = 0; i < venues.Count; i++)
            {
                var venue = venues[i];

                if (i > 0)
                {
                    uriBuilder.Append($"%7C");
                }
                //uriBuilder.Append($"{venue.Latitude}%2C{venue.Longitude}");
                //uriBuilder.Append($"{WebUtility.UrlEncode(venue.Postcode)}");
                uriBuilder.Append($"{venue.Postcode.Replace(" ", "")}");
            }

            uriBuilder.Append($"&key={_configuration.GoogleMapsApiKey}");

            Console.WriteLine("Calling google distance matrix api with uri");
            Console.WriteLine(uriBuilder);

            var stopwatch = Stopwatch.StartNew();

            var response = await _httpClient.GetAsync(uriBuilder.ToString());

            stopwatch.Stop();

            Console.WriteLine($"Received {response.StatusCode} in {stopwatch.ElapsedMilliseconds:#,###}ms");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //if (content.Length >= 150)
            //{
            //Console.WriteLine(content);
            //}

            //Debug.WriteLine(content);

            return null;
        }
    }
}
