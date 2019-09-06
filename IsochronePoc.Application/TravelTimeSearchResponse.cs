using Newtonsoft.Json;

namespace IsochronePoc.Application
{
    public class TravelTimeSearchResponse
    {
        [JsonProperty("results")]
        public Result[] Results { get; set; }
    }

    public class Result
    {
        [JsonProperty("search_id")]
        public string SearchId { get; set; }

        [JsonProperty("shapes")]
        public Shape[] Shapes { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public class Properties
    {
    }

    public class Shape
    {
        [JsonProperty("shell")]
        public Shell[] Shell { get; set; }

        [JsonProperty("holes")]
        public Shell[][] Holes { get; set; }
    }

    public class Shell
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }
}
