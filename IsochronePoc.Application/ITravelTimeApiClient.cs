using System.Threading.Tasks;

namespace IsochronePoc.Application
{
    public interface ITravelTimeApiClient
    {
        Task<string> Search(string postCode, decimal latitude, decimal longitude);
    }
}