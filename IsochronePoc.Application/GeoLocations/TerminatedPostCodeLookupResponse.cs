using Newtonsoft.Json;

namespace IsochronePoc.Application.GeoLocations
{
    public class TerminatedPostCodeLookupResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("result")]
        public TerminatedPostCodeLookupResultDto Result { get; set; }
    }

    public class TerminatedPostCodeLookupResultDto
    {
        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("year_terminated")]
        public string TerminatedYear { get; set; }
        
        [JsonProperty("month_terminated")]
        public string TerminatedMonth { get; set; }
    }
}
