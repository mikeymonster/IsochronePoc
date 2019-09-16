using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsochronePoc.Application.TravelTimeFilterFastApi
{
    public interface ITravelTimeFilterFastApiClient
    {
        Task<string> Search(string postcode, decimal latitude, decimal longitude, IList<Application.Location> locations);
    }
}