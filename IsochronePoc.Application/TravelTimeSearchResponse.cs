using Newtonsoft.Json;

namespace IsochronePoc.Application
{
    public class TravelTimeSearchResponse
    {
        [JsonProperty("results")]
        public Result[] Results { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("search_id")]
        public string SearchId { get; set; }

        [JsonProperty("shapes")]
        public Shape[] Shapes { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public partial class Properties
    {
    }

    public partial class Shape
    {
        [JsonProperty("shell")]
        public Shell[] Shell { get; set; }

        [JsonProperty("holes")]
        public Shell[][] Holes { get; set; }
    }

    public partial class Shell
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }
}
