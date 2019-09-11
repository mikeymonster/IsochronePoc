
namespace IsochronePoc.Application
{
    public class ApiConfiguration
    {
        //TravelTime keys
        public string ApplicationId { get; set; }
        public string ApiKey { get; set; }
        public string IsochroneQueryUri { get; set; }
        public string TravelTimeQueryUri { get; set; }

        //Google keys
        public string GoogleMapsApiBaseUrl { get; set; }
        public string GoogleMapsApiKey { get; set; }
    }
}
