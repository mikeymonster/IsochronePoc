using Newtonsoft.Json;

namespace IsochronePoc.Application.TravelTimeFilterApi
{
    public class TravelTimeFilterSearchResponse
    {
        [JsonProperty("results")]
        public Result[] Results { get; set; }
    }

    public class Result
    {
        [JsonProperty("search_id")]
        public string SearchId { get; set; }

        [JsonProperty("locations")]
        public ResponseLocation[] ResponseLocations { get; set; }

        [JsonProperty("unreachable")]
        public string[] Unreachable { get; set; }
    }

    public class ResponseLocation
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("properties")]
        public Property[] Properties { get; set; }
    }

    public class Property
    {
        [JsonProperty("travel_time")]
        public long TravelTime { get; set; }

        [JsonProperty("distance", NullValueHandling = NullValueHandling.Ignore)]
        public long? Distance { get; set; }

        [JsonProperty("distance_breakdown", NullValueHandling = NullValueHandling.Ignore)]
        public DistanceBreakdown[] DistanceBreakdown { get; set; }

        [JsonProperty("fares", NullValueHandling = NullValueHandling.Ignore)]
        public Fares Fares { get; set; }
    }

    public class DistanceBreakdown
    {
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("distance")]
        public long Distance { get; set; }
    }

    public class Fares
    {
        [JsonProperty("breakdown")]
        public Breakdown[] Breakdown { get; set; }

        [JsonProperty("tickets_total")]
        public Ticket[] TicketsTotal { get; set; }
    }

    public class Breakdown
    {
        [JsonProperty("modes")]
        public string[] Modes { get; set; }

        [JsonProperty("route_part_ids")]
        public long[] RoutePartIds { get; set; }

        [JsonProperty("tickets")]
        public Ticket[] Tickets { get; set; }
    }

    public class Ticket
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    //public class TravelTimeFilterSearchResponse
    //{
    //    public static TravelTimeFilterApi.TravelTimeFilterSearchResponse FromJson(string json) => JsonConvert.DeserializeObject<TravelTimeFilterApi.TravelTimeFilterSearchResponse>(json, QuickType.Converter.Settings);
    //}

    //public static class Serialize
    //{
    //    public static string ToJson(this TravelTimeFilterApi.TravelTimeFilterSearchResponse self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    //}

    //internal static class Converter
    //{
    //    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    //    {
    //        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
    //        DateParseHandling = DateParseHandling.None,
    //        Converters =
    //    {
    //        new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
    //    },
    //    };
    //}
}
