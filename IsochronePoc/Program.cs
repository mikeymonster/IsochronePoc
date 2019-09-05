using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IsochronePoc.Application;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IsochronePoc
{
    class Program
    {
        internal static ApiConfiguration Configuration { get; set; }

        static async Task Main()
        {
            try
            {
                Configure();

                Console.WriteLine($"TravelTime Api uri - {Configuration.IsochroneQueryUri}");
                Console.WriteLine($"TravelTime Api keys - {Configuration.ApplicationId} - {Configuration.ApiKey}");
                Console.WriteLine($"Google Api uri - {Configuration.GoogleMapsApiBaseUrl}");
                Console.WriteLine($"Google Api key - {Configuration.GoogleMapsApiKey}");

                await GetGoogleResult(@".\Data\TestVenues.csv");

                //Console.WriteLine("Press Enter to continue, or any other key to exit");
                //var key = Console.ReadKey().Key;
                //if (key != ConsoleKey.Enter)
                //{
                //    return;
                //}


                /******************************************/


                //var dataPath = @".\Data\sample_isochrone.json";
                //var data = await LoadJson(dataPath);
                var data = await GetIsochrone("OX2 9GX", 51.742141M, -1.295653M);
                var data2 = await GetIsochrone("CV1 2WT", 52.400997M, -1.508122M);
                var data3 = await GetIsochrone("NE2 4RL", 54.98543M, -1.606414M);
                //
                //foreach (var location in data)
                //{
                //    Console.WriteLine($"{location.Latitude}, {location.Longitude}");
                //}

                Console.WriteLine();
                Console.WriteLine("Locations string for SQL Query for sample data:");
                var testData = await LoadJson(@".\Data\simple_CV1_2WT.json");
                await WriteToSqlFile(CreateSqlQuery(testData), @".\Data\Spatial Query.sql");

                //Console.WriteLine("");
                //Console.WriteLine($"{sqlString}");
                //Console.WriteLine("");

                await WriteToCsv(testData, @".\Data\simple_CV1_2WT.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
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
            await client.Search(startPoint, venues);
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
                    var split = line.Split(',');
                    venues.Add(new Venue
                    {
                        Postcode = split[0],
                        Longitude = decimal.Parse(split[1]),
                        Latitude = decimal.Parse(split[2])
                    });
                }
            }

            return venues;
        }

        public static async Task<IList<Location>> GetIsochrone(string postCode, decimal latitude, decimal longitude)
        {
            var client = new TravelTimeApiClient(new HttpClient(), Configuration);

            var outputPath = $@".\Data\sample_{postCode.Replace(" ", "_")}.json";

            var result = await client.Search(postCode, latitude, longitude);
            //var path = @".\Data\sample_isochrone.json";

            //using (var reader = File.OpenText(path))
            //using (var jsonReader = new JsonTextReader(reader))
            using (var writer = File.CreateText(outputPath))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                //var json = await JObject.LoadAsync(jsonReader);
                //jsonWriter.WriteRaw(json.ToString());
                jsonWriter.WriteRaw(result);
            }

            return await LoadJson(outputPath);
        }

        public static async Task<IList<Location>> LoadJson(string path)
        {
            //using (var stream = await responseMessage.Content.ReadAsStreamAsync())
            //using (var reader = new StreamReader(stream))
            //using (var jsonReader = new JsonTextReader(reader))
            //{
            //    var json = await JObject.LoadAsync(jsonReader);
            //    var englandAndWalesHolidays = json
            //        .SelectTokens(
            //            "$.divisions.england-and-wales..[?(@.date)]");

            try
            {
                using (var reader = File.OpenText(path))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var json = await JObject.LoadAsync(jsonReader);

                    var results = json
                        .SelectTokens(
                            "$.results");

                    var shapes = json
                        .SelectTokens(
                            "$.results[0].shapes[0].shell");

                    var serializer = new JsonSerializer();

                    //var newShapes = serializer.Deserialize<List<Shape>>(shapes.);
                    //JsonConvert.DeserializeObject<List<Shape>>();

                    //foreach (var shape in shapes.Children())
                    //{
                    //    var r = shape.ToObject<Location>(serializer);
                    //}

                    var shapeResults = shapes.Children()
                        .Select(x => x.ToObject<Location>(serializer))
                        .ToList();
                    //var serializer = new JsonSerializer();
                    //var result = (List<Location>)serializer.Deserialize(file, typeof(List<Location>));

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

        private static string CreateSqlQuery(IList<Location> data)
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

        private static async Task WriteToCsv(IList<Location> data, string path)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine("Longitude, Latitude");

                foreach (var location in data)
                {
                    writer.WriteLine($"{location.Longitude}, {location.Latitude}");
                }
            }
        }
    }
}
