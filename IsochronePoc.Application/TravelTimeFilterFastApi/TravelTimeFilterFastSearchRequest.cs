using Newtonsoft.Json;

namespace IsochronePoc.Application.TravelTimeFilterFastApi
{
    public class TravelTimeFilterFastSearchRequest
    {
        [JsonProperty("locations")]
        public Location[] Locations { get; set; }

        [JsonProperty("arrival_searches")]
        public ArrivalSearches ArrivalSearches { get; set; }
    }

    public class ArrivalSearches
    {
        [JsonProperty("many_to_one")]
        public ManyToOne[] ManyToOne { get; set; }

        [JsonProperty("one_to_many")]
        public OneToMany[] OneToMany { get; set; }
    }

    public class ManyToOne
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("departure_location_ids")]
        public string[] DepartureLocationIds { get; set; }

        [JsonProperty("arrival_location_id")]
        public string ArrivalLocationId { get; set; }

        [JsonProperty("transportation")]
        public Transportation Transportation { get; set; }

        [JsonProperty("arrival_time_period")]
        public string ArrivalTimePeriod { get; set; }

        [JsonProperty("travel_time")]
        public long TravelTime { get; set; }

        [JsonProperty("properties")]
        public string[] Properties { get; set; }
    }

    public class Transportation
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class OneToMany
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("arrival_location_ids")]
        public string[] ArrivalLocationIds { get; set; }

        [JsonProperty("departure_location_id")]
        public string DepartureLocationId { get; set; }

        [JsonProperty("transportation")]
        public Transportation Transportation { get; set; }

        [JsonProperty("arrival_time_period")]
        public string ArrivalTimePeriod { get; set; }

        [JsonProperty("travel_time")]
        public long TravelTime { get; set; }

        [JsonProperty("properties")]
        public string[] Properties { get; set; }
    }

    public class Location
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("coords")]
        public Coords Coords { get; set; }
    }

    public class Coords
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }
}
