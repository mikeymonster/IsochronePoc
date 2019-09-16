using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace IsochronePoc.Application.GeoLocations
{
    public class PostCodeLookupResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("result")]
        public PostCodeLookupResultDto Result { get; set; }
    }

    public class PostCodeLookupResultDto
    {
        public string Postcode { get; set; }
        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public string Country { get; set; }

        public string Region { get; set; }

        public string OutCode { get; set; }

        public string Admin_District { get; set; }

        public string Admin_County { get; set; }

        public LocationCodesDto Codes { get; set; }
    }

    public class LocationCodesDto
    {
        public string Admin_District { get; set; }

        public string Admin_County { get; set; }

        public string Admin_Ward { get; set; }

        public string Parish { get; set; }

        public string Parliamentary_Constituency { get; set; }

        public string Ccg { get; set; }

        public string Ced { get; set; }

        public string Nuts { get; set; }
    }

}
