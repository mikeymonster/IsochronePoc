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
            var batches = CreateBatches(venues, batchSize);
            var results = new List<GoogleDistanceMatrixResponse>();

            var stopwatch = Stopwatch.StartNew();

            Parallel.ForEach(batches, async batch =>
            {
                var (key, value) = batch;
                Console.WriteLine($"Processing batch {key} of size {value.Count}");

                var response = await SearchBatch(origin, value);
                if (response != null)
                {
                    lock (_listLocker)
                    {
                        results.Add(response);
                    }
                }
            });

            stopwatch.Stop();
            Console.WriteLine($"Have {results.Count} results from {batches.Count} batches of {batchSize} in {stopwatch.ElapsedMilliseconds:#,###}ms");

            await BuildGoogleResults(results);
        }

        private async Task BuildGoogleResults(List<GoogleDistanceMatrixResponse> responses)
        {
            foreach (var item in responses)
            {
                if (string.Compare(item.Status, "OK", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    var message = $"Failure response from google api - {item.Status}";
                    Console.WriteLine(message);
                    throw new Exception(message);
                }

                Console.WriteLine($"Have {item.Rows.Length} rows, {item.DestinationAddresses.Length} destinations, {item.OriginAddresses.Length}");

                Console.WriteLine("OriginAddresses:");
                foreach (var origin in item.OriginAddresses)
                {
                    Console.WriteLine($"  {origin}");
                }

                Console.WriteLine("DestinationAddresses:");
                foreach (var destination in item.DestinationAddresses)
                {
                    Console.WriteLine($"  {destination}");
                }

                Console.WriteLine("Rows:");
                foreach (var row in item.Rows)
                {
                    Console.WriteLine($"  Row has {row.Elements.Length} elements");
                    foreach (var element in row.Elements)
                    {
                        Console.WriteLine($"    {element.Status}, {element.Distance}, {element.Duration}");
                    }
                }

                foreach (var row in item.Rows)
                {
                    
                }
                
            }
        }

        private Dictionary<int, IList<Venue>> CreateBatches(IList<Venue> venues, int batchSize)
        {
            var batches = new Dictionary<int, IList<Venue>>();
            var items = venues;
            var batchNo = 0;
            while (items.Any())
            {
                var batch = items.Take(batchSize);
                batches.Add(++batchNo, batch.ToList());
                items = items.Skip(batchSize).ToList();
            }

            return batches;
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

            //var content = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(content);
            //Debug.WriteLine(content);

            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
                {
                    new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
                }
            };

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                var result = (GoogleDistanceMatrixResponse)serializer.Deserialize(jsonReader,
                        typeof(GoogleDistanceMatrixResponse));

                return result;
            }
        }
    }
}
