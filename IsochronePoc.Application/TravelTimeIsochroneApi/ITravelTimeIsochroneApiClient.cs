using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsochronePoc.Application.TravelTimeFilterApi
{
    public interface ITravelTimeIsochroneApiClient
    {
        Task<string> Search(string postcode, decimal latitude, decimal longitude, IList<Application.Location> locations);
    }
}