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

namespace IsochronePoc.Application
{
    public class GoogleDistanceMatrixApiClient : IGoogleDistanceMatrixApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _configuration;
        private static readonly object ListLocker = new object();

        public GoogleDistanceMatrixApiClient(HttpClient httpClient, ApiConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress = new Uri(_configuration.GoogleMapsApiBaseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IList<DistanceSearchResult>> Search(Venue origin, IList<Venue> venues)
        {
            const int batchSize = 100; //Max client-side elements: 100
            var batches = CreateBatches(venues, batchSize);
            var results = new List<GoogleDistanceMatrixResponse>();

            var stopwatch = Stopwatch.StartNew();

            Parallel.ForEach(batches, batch =>
            {
                var (key, value) = batch;
                Console.WriteLine($"Processing batch {key} of size {value.Count}");

                //Don't use async on body of Parallel.ForEach and wait here - it doesn't block
                //var response = await SearchBatch(origin, value);
                var response = SearchBatch(origin, value).GetAwaiter().GetResult();
                if (response != null)
                {
                    lock (ListLocker)
                    {
                        results.Add(response);
                    }
                }
            });

            stopwatch.Stop();
            Console.WriteLine($"Have {results.Count} results from {batches.Count} batches of {batchSize} in {stopwatch.ElapsedMilliseconds:#,###}ms");

            foreach (var x in results)
            {
                Console.WriteLine($"{x.Rows.Length} - {x.DestinationAddresses.Length}");
            }

            //results = results.Where(x => x.DestinationAddresses.Length < 100).ToList();
            var distanceSearchResults = await BuildGoogleResults(results);
            return distanceSearchResults;
        }

        private Task<IList<DistanceSearchResult>> BuildGoogleResults(List<GoogleDistanceMatrixResponse> responses)
        {
            var result = new List<DistanceSearchResult>();

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

                //Results are returned in rows, each row containing one origin paired with each destination.
                //Since we know there is only one origin, we should be able to assume
                //that the number of destinations == number of rows
                Console.WriteLine("Rows:");
                foreach (var row in item.Rows)
                {
                    Console.WriteLine($"  Row has {row.Elements.Length} elements");
                    foreach (var element in row.Elements)
                    {
                        try
                        {
                            Console.WriteLine($"    {element.Status}, {element.Distance?.Text}: {element.Distance?.Value}, {element.Duration?.Text}: {element.Duration?.Value}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }

                //Func<string, TimeSpan> convertTime = (string x) =>
                //{
                //    return new TimeSpan(10000);
                //};

                var currentRow = item.Rows[0];
                for (int i = 0; i < item.DestinationAddresses.Length; i++)
                {
                    var element = currentRow.Elements[i];
                    var destination = item.DestinationAddresses[i];

                    try
                    {
                        result.Add(new DistanceSearchResult
                        {
                            Address = destination,
                            DistanceUnits = element.Distance?.Text,
                            Distance = element.Distance?.Value ?? -1,
                            TravelTimeString = element.Duration?.Text,
                            //TravelTime = convertTime(element.Duration.Value),
                            TravelTime = element.Duration?.Value ?? -1,
                            Raw = $"{destination}, {element.Distance?.Text}: {element.Distance?.Value}, {element.Duration?.Text}: {element.Duration?.Value}"
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

        private async Task<GoogleDistanceMatrixResponse> SearchBatch(Venue origin, IList<Venue> venues, string travelMode = "driving")
        {
            try
            {
                //https://developers.google.com/maps/documentation/distance-matrix/intro

                //Call:
                //http://maps.googleapis.com/maps/api/distancematrix/outputFormat?parameters
                //var uri = "distancematrix";
                //NOTE: Assumes api url already has ending /
                var uriBuilder = new StringBuilder($@"{_configuration.GoogleMapsApiBaseUrl}distancematrix/json?");

                uriBuilder.Append($"origins={origin.Latitude}%2C{origin.Longitude}");
                uriBuilder.Append($"&mode={travelMode}");
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
            catch (Exception ex)
            {
                Console.WriteLine($"Failure calling google api - {ex}");
                throw;
            }
        }
    }
}
