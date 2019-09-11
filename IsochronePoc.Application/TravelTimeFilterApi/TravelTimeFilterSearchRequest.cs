using System;
using Newtonsoft.Json;

namespace IsochronePoc.Application.TravelTimeFilterApi
{
    public class TravelTimeFilterSearchRequest
    {
        [JsonProperty("locations")]
        public Location[] Locations { get; set; }

        [JsonProperty("departure_searches")]
        public DepartureSearch[] DepartureSearches { get; set; }

        [JsonProperty("arrival_searches")]
        public ArrivalSearch[] ArrivalSearches { get; set; }
    }

    public class ArrivalSearch
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("departure_location_ids")]
        public string[] DepartureLocationIds { get; set; }

        [JsonProperty("arrival_location_id")]
        public string ArrivalLocationId { get; set; }

        [JsonProperty("transportation")]
        public Transportation Transportation { get; set; }

        [JsonProperty("arrival_time")]
        public DateTimeOffset ArrivalTime { get; set; }

        [JsonProperty("travel_time")]
        public long TravelTime { get; set; }

        [JsonProperty("properties")]
        public string[] Properties { get; set; }
    }

    public class Transportation
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class DepartureSearch
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("departure_location_id")]
        public string DepartureLocationId { get; set; }

        [JsonProperty("arrival_location_ids")]
        public string[] ArrivalLocationIds { get; set; }

        [JsonProperty("transportation")]
        public Transportation Transportation { get; set; }

        [JsonProperty("departure_time")]
        public DateTimeOffset DepartureTime { get; set; }

        [JsonProperty("travel_time")]
        public long TravelTime { get; set; }

        [JsonProperty("properties")]
        public string[] Properties { get; set; }

        [JsonProperty("range")]
        public Range Range { get; set; }
    }

    public class Range
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("max_results")]
        public long MaxResults { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }
    }

    public class Location
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("coords")]
        public Coords Coords { get; set; }
    }

    public class Coords
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    //public class TravelTimeFilterSearchRequest
    //{
    //    public static TravelTimeFilterSearchRequest FromJson(string json) => JsonConvert.DeserializeObject<TravelTimeFilterSearchRequest>(json, QuickType.Converter.Settings);
    //}

    //public static class Serialize
    //{
    //    public static string ToJson(this TravelTimeFilterSearchRequest self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
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
    //        },
    //    };
    //}
}
