using Newtonsoft.Json;

namespace IsochronePoc.Application.TravelTimeFilterFastApi
{
    public class TravelTimeFilterFastSearchResponse
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
        public Properties Properties { get; set; }
    }

    public class Properties
    {
        [JsonProperty("travel_time")]
        public long TravelTime { get; set; }

        [JsonProperty("fares")]
        public Fares Fares { get; set; }
    }

    public class Fares
    {
        [JsonProperty("tickets_total")]
        public TicketsTotal[] TicketsTotal { get; set; }
    }

    public class TicketsTotal
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}
