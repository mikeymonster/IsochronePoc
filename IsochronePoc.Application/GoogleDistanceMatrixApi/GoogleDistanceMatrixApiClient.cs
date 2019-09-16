using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IsochronePoc.Application.GoogleDistanceMatrixApi
{
    public class GoogleDistanceMatrixApiClient : IGoogleDistanceMatrixApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _configuration;
        private static readonly object ListLocker = new object();

        private readonly bool _useEncodedPolyline;
        private readonly int _batchSize;

        public GoogleDistanceMatrixApiClient(HttpClient httpClient, ApiConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress = new Uri(_configuration.GoogleMapsApiBaseUrl);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _useEncodedPolyline = false;
            _batchSize = _useEncodedPolyline ? 100 : 100;
        }

        public async Task<IList<DistanceSearchResult>> Search(Location origin, IList<Location> venues)
        {
            var batches = CreateBatches(venues, _batchSize);
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
            Console.WriteLine($"Have {results.Count} results from {batches.Count} batches of {_batchSize} in {stopwatch.ElapsedMilliseconds:#,###}ms");

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

        private Dictionary<int, IList<Location>> CreateBatches(IList<Location> venues, int batchSize)
        {
            var batches = new Dictionary<int, IList<Location>>();
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

        private async Task<GoogleDistanceMatrixResponse> SearchBatch(Location origin, IList<Location> venues, string travelMode = "driving")
        {
            try
            {
                //https://developers.google.com/maps/documentation/distance-matrix/intro

                var uri = BuildUri(origin, venues, travelMode, _useEncodedPolyline);

                var stopwatch = Stopwatch.StartNew();

                var response = await _httpClient.GetAsync(uri);

                stopwatch.Stop();

                Console.WriteLine($"Received {response.StatusCode} in {stopwatch.ElapsedMilliseconds:#,###}ms");

                response.EnsureSuccessStatusCode();

                //var content = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(content);
                //Debug.WriteLine(content);

                //var settings = new JsonSerializerSettings
                //{
                //    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                //    DateParseHandling = DateParseHandling.None,
                //    Converters =
                //    {
                //        new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
                //    }
                //};

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

        public string BuildUri(Location origin, IList<Location> venues, string travelMode, bool useEncodedPolyline)
        {
            //http://maps.googleapis.com/maps/api/distancematrix/outputFormat?parameters
            //var uri = "distancematrix";
            //NOTE: Assumes api url already has ending /
            var uriBuilder = new StringBuilder($@"{_configuration.GoogleMapsApiBaseUrl}");
            if (!_configuration.GoogleMapsApiBaseUrl.EndsWith("/"))
            {
                uriBuilder.Append("/");
            }
            uriBuilder.Append("distancematrix/json?");

            uriBuilder.Append($"origins={origin.Latitude},{origin.Longitude}");
            uriBuilder.Append($"&mode={travelMode}");
            uriBuilder.Append("&destinations=");

            if (useEncodedPolyline)
            {
                uriBuilder.Append("enc:");

                //var polyline = EncodeCoordinates(venues);
                var polyline = EncodePolyline(venues);
                polyline = WebUtility.UrlEncode(polyline);

                uriBuilder.Append(polyline);

                //for (var i = 0; i < venues.Count; i++)
                //{
                //    if (i > 0) uriBuilder.Append("%40");

                //    uriBuilder.Append(EncodePolyline(new List<Venue> { venues[i] }));
                //}

                uriBuilder.Append(":");
            }
            else
            {
                for (var i = 0; i < venues.Count; i++)
                {
                    var venue = venues[i];

                    if (i > 0) uriBuilder.Append("%7C");

                    uriBuilder.Append($"{venue.Latitude}%2C{venue.Longitude}");
                    //uriBuilder.Append($"{WebUtility.UrlEncode(venue.Postcode)}");
                    //uriBuilder.Append($"{venue.Postcode.Replace(" ", "")}");
                }
            }

            uriBuilder.Append($"&key={_configuration.GoogleMapsApiKey}");

            Console.WriteLine("Calling google distance matrix api with uri");
            Console.WriteLine(uriBuilder);

            var uri = uriBuilder.ToString();
            return uri;
        }

        //Implementation from https://gist.github.com/shinyzhu/4617989
        public string EncodePolyline(IList<Location> points)
        {
            var sb = new StringBuilder();

            var encodeDiff = (Action<int>)(diff =>
            {
                var shifted = diff << 1;
                if (diff < 0)
                    shifted = ~shifted;

                var rem = shifted;

                while (rem >= 0x20)
                {
                    sb.Append((char)((0x20 | (rem & 0x1f)) + 63));

                    rem >>= 5;
                }

                sb.Append((char)(rem + 63));
            });

            var lastLat = 0;
            var lastLng = 0;

            foreach (var point in points)
            {
                var lat = (int)Math.Round((double)point.Latitude * 1E5);
                var lng = (int)Math.Round((double)point.Longitude * 1E5);

                if (lat == lastLat || lng == lastLng)
                    continue;

                encodeDiff(lat - lastLat);
                encodeDiff(lng - lastLng);

                lastLat = lat;
                lastLng = lng;
            }
            return sb.ToString();
        }

        //Implementation from https://briancaos.wordpress.com/2009/10/16/google-maps-polyline-encoding-in-c/
        public static string EncodeCoordinates(IList<Location> coordinates)
        {
            int plat = 0;
            int plng = 0;
            StringBuilder encodedCoordinates = new StringBuilder();
            foreach (var coordinate in coordinates)
            {
                // Round to 5 decimal places and drop the decimal
                int late5 = (int)Math.Round((double)coordinate.Latitude * 1e5, MidpointRounding.AwayFromZero);
                int lnge5 = (int)Math.Round((double)coordinate.Longitude * 1e5, MidpointRounding.AwayFromZero);
                // Encode the differences between the coordinates

                if (late5 == plat || lnge5 == plng)
                    continue;

                encodedCoordinates.Append(EncodeSignedNumber(late5 - plat));
                encodedCoordinates.Append(EncodeSignedNumber(lnge5 - plng));
                // Store the current coordinates
                plat = late5;
                plng = lnge5;
            }
            return encodedCoordinates.ToString();
        }

        private static string EncodeSignedNumber(int num)
        {
            int sgn_num = num << 1; //shift the binary value
            if (num < 0) //if negative invert
            {
                sgn_num = ~(sgn_num);
            }
            return (EncodeNumber(sgn_num));
        }


        private static string EncodeNumber(int num)
        {
            StringBuilder encodeString = new StringBuilder();
            while (num >= 0x20)
            {
                encodeString.Append((char)((0x20 | (num & 0x1f)) + 63));
                num >>= 5;
            }
            encodeString.Append((char)(num + 63));
            // All backslashes needs to be replaced with double backslashes
            // before being used in a Javascript string.
            return encodeString.ToString().Replace(@"\", @"\\");
        }
    }
}
