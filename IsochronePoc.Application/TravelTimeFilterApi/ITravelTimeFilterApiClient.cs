using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsochronePoc.Application.TravelTimeFilterApi
{
    public interface ITravelTimeFilterApiClient
    {
        Task<string> Search(string postcode, decimal latitude, decimal longitude, IList<Venue> locations);
    }
}