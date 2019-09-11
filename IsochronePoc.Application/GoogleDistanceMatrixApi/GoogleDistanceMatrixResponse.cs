using Newtonsoft.Json;

namespace IsochronePoc.Application.GoogleDistanceMatrixApi
{
    public class GoogleDistanceMatrixResponse
    {
        [JsonProperty("destination_addresses")]
        public string[] DestinationAddresses { get; set; }

        [JsonProperty("origin_addresses")]
        public string[] OriginAddresses { get; set; }

        [JsonProperty("rows")]
        public Row[] Rows { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class Row
    {
        [JsonProperty("elements")]
        public Element[] Elements { get; set; }
    }

    public class Element
    {
        [JsonProperty("distance")]
        public Distance Distance { get; set; }

        [JsonProperty("duration")]
        public Distance Duration { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class Distance
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }
    }

    //public partial class GoogleDistanceMatrixResponse
    //{
    //    public static GoogleDistanceMatrixResponse FromJson(string json) => 
    //        JsonConvert.DeserializeObject<GoogleDistanceMatrixResponse>(json, QuickType.Converter.Settings);
    //}

    //public static class Serialize
    //{
    //    public static string ToJson(this GoogleDistanceMatrixResponse self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
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
