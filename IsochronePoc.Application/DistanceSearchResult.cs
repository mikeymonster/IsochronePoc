using System;

namespace IsochronePoc.Application
{
    public class DistanceSearchResult
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string DistanceUnits { get; set; }
        public double Distance { get; set; }
        public string TravelTimeString { get; set; }
        public double TravelTime { get; set; }
        public string Raw { get; set; }
    }
}
