using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Infrastructure.Repositories.Interfaces;
using System.Linq; // Added for .Where()

namespace VitaFlow.Services.Services
{
    // Implementation of the ILocationService interface.
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public LocationService(ILogger<LocationService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        
        /// <summary>
        /// Calculates the distance between two geographical coordinates using the Haversine formula.
        /// </summary>
        /// <param name="lat1">Latitude of the first point</param>
        /// <param name="lon1">Longitude of the first point</param>
        /// <param name="lat2">Latitude of the second point</param>
        /// <param name="lon2">Longitude of the second point</param>
        /// <returns>Distance in kilometers</returns>
        public Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
        {
            try
            {
                _logger.LogInformation("Calculating distance between ({Lat1}, {Lon1}) and ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
                
                // Implementation of the Haversine formula
                const double earthRadiusKm = 6371.0; // Earth radius in kilometers
                
                var dLat = DegreesToRadians(lat2 - lat1);
                var dLon = DegreesToRadians(lon2 - lon1);
                
                lat1 = DegreesToRadians(lat1);
                lat2 = DegreesToRadians(lat2);
                
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * 
                        Math.Cos(lat1) * Math.Cos(lat2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = earthRadiusKm * c;
                
                return Task.FromResult(distance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance between coordinates");
                throw;
            }
        }
        
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        // Lấy tất cả location trong bán kính radiusInKm từ điểm (latitude, longitude)
        public async Task<IEnumerable<Location>> GetLocationsWithinRadiusAsync(double latitude, double longitude, double radiusInKm)
        {
            try
            {
                _logger.LogInformation("Finding locations within {Radius}km of ({Lat}, {Lon})", radiusInKm, latitude, longitude);
                var repo = _unitOfWork.GetRepository<Location>();
                var allLocations = await repo.GetListAsync(predicate: null);
                // Lọc các location có khoảng cách đến điểm trung tâm nhỏ hơn hoặc bằng bán kính
                var locationsWithinRadius = allLocations.Where(location =>
                    CalculateDistance(latitude, longitude, location.Latitude, location.Longitude) <= radiusInKm
                );
                return locationsWithinRadius;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding locations within radius");
                throw;
            }
        }

        // Hàm tính khoảng cách Haversine giữa hai điểm (km)
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) *
                    Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }
    }
}
