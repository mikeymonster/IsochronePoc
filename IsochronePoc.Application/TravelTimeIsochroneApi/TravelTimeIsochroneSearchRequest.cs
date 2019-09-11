using Newtonsoft.Json;

namespace IsochronePoc.Application.TravelTimeIsochroneApi

{
    public class TravelTimeIsochroneSearchRequest
    {
        [JsonProperty("departure_searches")]
        public DepartureSearch[] DepartureSearches { get; set; }

        [JsonProperty("arrival_searches")]
        public ArrivalSearch[] ArrivalSearches { get; set; }
    }

    public class ArrivalSearch
    {
        //Not filled in as we won't be using it
    }

    public class DepartureSearch
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("coords")]
        public Coords Coords { get; set; }

        [JsonProperty("transportation")]
        public Transportation Transportation { get; set; }

        [JsonProperty("departure_time")]
        public string DepartureTime { get; set; }

        [JsonProperty("travel_time")]
        public long TravelTime { get; set; }
    }

    public class Coords
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public class Transportation
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    //public class TravelTimeSearch
    //{
    //    public static TravelTimeSearch FromJson(string json) => 
    //        JsonConvert.DeserializeObject<TravelTimeSearch>(json, Converter.Settings);
    //}

    //public static class Serialize
    //{
    //    public static string ToJson(this TravelTimeSearch self) => JsonConvert.SerializeObject(self, Converter.Settings);
    //}

    //internal static class Converter
    //{
    //    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    //    {
    //        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
    //        DateParseHandling = DateParseHandling.None,
    //        Converters =
    //        {
    //            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
    //        }
    //    };
    //}
}
