using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;

namespace VitaFlow.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for location-based operations.
    /// </summary>
    public interface ILocationService
    {
        Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2);
        Task<IEnumerable<Location>> GetLocationsWithinRadiusAsync(double latitude, double longitude, double radiusInKm);
    }
}
