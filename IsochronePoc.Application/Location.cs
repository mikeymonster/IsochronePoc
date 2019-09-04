using Newtonsoft.Json;

namespace IsochronePoc.Application
{
    public class Location
    {
        [JsonProperty("lat")]
        public decimal Latitude { get; set; }
        [JsonProperty("lng")]
        public decimal Longitude { get; set; }
    }
}
