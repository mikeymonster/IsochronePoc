using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IsochronePoc.Application;
using IsochronePoc.Application.GoogleDistanceMatrixApi;
using IsochronePoc.Application.TravelTimeFilterApi;
using IsochronePoc.Application.TravelTimeIsochroneApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IsochronePoc
{
    class Program
    {
        internal static ApiConfiguration Configuration { get; set; }

        public static async Task Main()
        {
            try
            {
                Configure();

                Console.WriteLine($"TravelTime Api nIsochrone uri - {Configuration.IsochroneQueryUri}");
                Console.WriteLine($"TravelTime Api travel time uri - {Configuration.TravelTimeQueryUri}");
                Console.WriteLine($"TravelTime Api keys - {Configuration.ApplicationId} - {Configuration.ApiKey}");
                Console.WriteLine($"Google Api uri - {Configuration.GoogleMapsApiBaseUrl}");
                Console.WriteLine($"Google Api key - {Configuration.GoogleMapsApiKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Configuration failed.");
                Console.WriteLine(ex.Message);
                return;
            }

            do
            {
                Console.WriteLine("Select an option using the number (or Enter for the default)");
                Console.WriteLine("  1 = Run google search");
                Console.WriteLine("  2 = Run TravelTime travel time search (default)");
                Console.WriteLine("  3 = Run TravelTime Isochrone search");
                Console.WriteLine("  4 = Create sample SQL query from isochrone Run json");
                Console.WriteLine("Press any other key to exit");
                Console.WriteLine("");

                try
                {
                    var key = Console.ReadKey().Key;
                    Console.WriteLine("");

                    switch (key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            await GetGoogleResult(@".\Data\LiveProviderVenues.csv");
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                        case ConsoleKey.Enter:
                            var venues = await GetVenuesFromCsv(@".\Data\LiveProviderVenues.csv");
                            await GetTravelTimeFilterResult("CV1 2WT",
                                52.400997M, -1.508122M,
                                venues);
                            break;
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            await GetTravelTimeIsochroneResult();
                            break;
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            await CreateSqlQueryFromJsonFile(@".\Data\simple_CV1_2WT.json");
                            break;
                        default:
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            } while (true);
        }

        private static void Configure()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Console.WriteLine(Directory.GetCurrentDirectory());

            var localConfiguration = builder.Build();

            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            var travelTimeApiSection = localConfiguration.GetSection("TravelTimeApi");
            var googleApiSection = localConfiguration.GetSection("GoogleApi");

            Configuration = new ApiConfiguration
            {
                ApplicationId = travelTimeApiSection["ApplicationId"],
                ApiKey = travelTimeApiSection["ApiKey"],
                IsochroneQueryUri = travelTimeApiSection["IsochroneQueryUri"],
                TravelTimeQueryUri = travelTimeApiSection["TravelTimeQueryUri"],
                GoogleMapsApiBaseUrl = googleApiSection["GoogleMapsApiBaseUrl"],
                GoogleMapsApiKey = googleApiSection["GoogleMapsApiKey"]
            };
        }

        public static async Task GetGoogleResult(string path)
        {
            var venues = await GetVenuesFromCsv(path);
            Console.WriteLine($"Have {venues.Count} venues");
            //foreach (var venue in venues)
            //{
            //    Console.WriteLine($"Venue: {venue.Postcode}, {venue.Latitude}, {venue.Longitude}");
            //}

            var client = new GoogleDistanceMatrixApiClient(new HttpClient(), Configuration);

            var startPoint = new Venue
            {
                Postcode = "CV1 2WT",
                Latitude = 52.400997M,
                Longitude = -1.508122M
            };

            var stopwatch = Stopwatch.StartNew();

            var searchResults = await client.Search(startPoint, venues);

            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine($"Retrieved {searchResults.Count} search results in  in {stopwatch.ElapsedMilliseconds:#,###}ms");

            Console.WriteLine("Results that could not be determined:");
            foreach (var searchResult in searchResults.Where(x => x.Distance < 0 || x.TravelTime < 0))
            {
                Console.WriteLine($"{searchResult.Address} at {searchResult.Distance}{searchResult.DistanceUnits}, travel time {searchResult.TravelTimeString}");
            }

            foreach (var searchResult in searchResults.OrderByDescending(x => x.TravelTime))
            {
                //TODO: Check api docs - can we ask for miles?
                if (searchResult.DistanceUnits != "km")
                {
                }

                var distanceInKm = searchResult.Distance / 1000;
                var distanceInMiles = searchResult.Distance / 1609.34;
                Console.WriteLine($"{searchResult.Address} at {distanceInMiles:#.0}mi ({distanceInKm:#.0}km), travel time {searchResult.TravelTimeString}");
            }
        }
        public static async Task GetTravelTimeFilterResult(string postcode, decimal latitude, decimal longitude, IList<Venue> venues)
        {
            Console.WriteLine($"Have {venues.Count} venues");

            var client = new TravelTimeFilterApiClient(new HttpClient(), Configuration);

            var outputPath = $@".\Data\sample_{postcode.Replace(" ", "_")}.json";

            var result = await client.Search(postcode, latitude, longitude, venues);
            //var path = @".\Data\sample_isochrone.json";

            using (var writer = File.CreateText(outputPath))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.WriteRaw(result);
            }

            var stopwatch = Stopwatch.StartNew();

            stopwatch.Stop();
            //Console.WriteLine($"Retrieved {searchResults.Count} search results in  in {stopwatch.ElapsedMilliseconds:#,###}ms");
        }
        
        public static async Task GetTravelTimeIsochroneResult()
        {
            //var data = await LoadJson(path);
            //var data = await GetIsochrone("OX2 9GX", 51.742141M, -1.295653M);
            var data = await GetIsochrone("CV1 2WT", 52.400997M, -1.508122M);
            //var data = await GetIsochrone("NE2 4RL", 54.98543M, -1.606414M);

            foreach (var location in data)
            {
                Console.WriteLine($"{location.Latitude}, {location.Longitude}");
            }
        }

        public static async Task<IList<LatLong>> GetIsochrone(string postcode, decimal latitude, decimal longitude)
        {
            var client = new TravelTimeApiIsochroneClient(new HttpClient(), Configuration);

            var outputPath = $@".\Data\isochrones_{postcode.Replace(" ", "_")}.json";

            var result = await client.Search(postcode, latitude, longitude);
            //var path = @".\Data\sample_isochrone.json";

            using (var writer = File.CreateText(outputPath))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.WriteRaw(result);
            }

            return await LoadJson(outputPath);
        }

        public static async Task<IList<LatLong>> LoadJson(string path)
        {
            try
            {
                using (var reader = File.OpenText(path))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var json = await JObject.LoadAsync(jsonReader);

                    //var results = json
                    //    .SelectTokens(
                    //        "$.results");

                    var shapes = json
                        .SelectTokens(
                            "$.results[0].shapes[0].shell");

                    var serializer = new JsonSerializer();

                    //var newShapes = serializer.Deserialize<List<Shape>>(shapes.);
                    //JsonConvert.DeserializeObject<List<Shape>>();

                    //foreach (var shape in shapes.Children())
                    //{
                    //    var r = shape.ToObject<LatLong>(serializer);
                    //}

                    var shapeResults = shapes.Children()
                        .Select(x => x.ToObject<LatLong>(serializer))
                        .ToList();
                    //var serializer = new JsonSerializer();
                    //var result = (List<LatLong>)serializer.Deserialize(file, typeof(List<LatLong>));

                    //return shapeResults.First().Shell;
                    return shapeResults;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<IList<Venue>> GetVenuesFromCsv(string path)
        {
            var venues = new List<Venue>();

            using (var reader = File.OpenText(path))
            {
                await reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    try
                    {

                        var split = line.Split(',');
                        venues.Add(new Venue
                        {
                            Id = int.Parse(split[0]),
                            Postcode = split[1],
                            Latitude = decimal.Parse(split[2]),
                            Longitude = decimal.Parse(split[3])
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return venues;
        }

        public static async Task CreateSqlQueryFromJsonFile(string path)
        {
            var testData = await LoadJson(path);
            await WriteToSqlFile(CreateSqlQuery(testData), @".\Data\Spatial Query.sql");

            //Console.WriteLine("");
            Console.WriteLine("LatLongs string for SQL Query for sample data:");
            //Console.WriteLine($"{sqlString}");
            //Console.WriteLine("");

            await WriteToCsv(testData, @".\Data\simple_CV1_2WT.csv");
        }

        private static string CreateSqlQuery(IList<LatLong> data)
        {
            var sb = new StringBuilder();

            sb.AppendLine("WITH polygons");
            sb.AppendLine("AS(SELECT 'p1' id,");
            sb.Append("geography::STGeomFromText('polygon ((");

            //https://bertwagner.com/2018/01/23/inverted-polygons-how-to-troubleshoot-sql-servers-left-hand-rule/
            //Either write backwards, or add 
            var isFirstItem = true;
            for (var i = data.Count - 1; i >= 0; i--)
            //for (var i = 0; i < data.Count; i++)
            //foreach (var location in data.Reverse())
            {
                //if (sb.Length > 0)
                if (isFirstItem)
                {
                    isFirstItem = false;
                }
                else
                {
                    sb.Append(", ");
                }

                //sb.Append($"{location.Longitude} {location.Latitude}");
                sb.Append($"{data[i].Longitude} {data[i].Latitude}");
                //sb.Append($"{location.Latitude} {location.Longitude}");
            }

            sb.AppendLine("))',");
            sb.Append("4326) poly),");
            //sb.AppendLine("4326).ReorientObject() poly),");

            sb.AppendLine("points");
            sb.AppendLine("AS(SELECT[Postcode], [Location] as p FROM ProviderVenue)");
            sb.AppendLine("SELECT DISTINCT");
            sb.AppendLine("       points.Postcode, ");
            sb.AppendLine("       points.p.STAsText() as Location,	");
            sb.AppendLine("       points.p.Lat as Latitude,");
            sb.AppendLine("       points.p.Long as Longitude");
            sb.AppendLine("FROM polygons");
            sb.AppendLine("     RIGHT JOIN points ON polygons.poly.STIntersects(points.p) = 1");
            sb.AppendLine("WHERE polygons.id IS NOT NULL;");

            //return "SELECT * FROM ProviderVenue";
            return sb.ToString();
        }

        private static async Task WriteToSqlFile(string query, string path)
        {
            using (var writer = File.CreateText(path))
            {
                await writer.WriteAsync(query);
            }
        }

        private static async Task WriteToCsv(IList<LatLong> data, string path)
        {
            using (var writer = File.CreateText(path))
            {
                await writer.WriteLineAsync("Longitude, Latitude");

                foreach (var location in data)
                {
                    await writer.WriteLineAsync($"{location.Longitude}, {location.Latitude}");
                }
            }
        }
    }
}
