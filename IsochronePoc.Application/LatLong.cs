using Newtonsoft.Json;

namespace IsochronePoc.Application
{
    public class LatLong
    {
        [JsonProperty("lat")]
        public decimal Latitude { get; set; }
        [JsonProperty("lng")]
        public decimal Longitude { get; set; }
    }
}
