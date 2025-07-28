using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTOs;
using static Models.DTOs.DonorDTO;

namespace Services
{
    public interface IDonorService
    {
        Task<IEnumerable<DonorDto>> GetAllAsync();
        Task<DonorDto> GetByIdAsync(Guid id);
        Task<DonorDto> CreateAsync(CreateDonorDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateDonorDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<Models.BloodType>> GetBloodTypesAsync();
        Task<IEnumerable<NearbyDonorDto>> GetNearbyDonorsAsync(double latitude, double longitude, double radiusInKm);
        //Task<DonorHealthCheckDto> HealthCheckAsync(Guid donorId);
    }

    public class NearbyDonorDto
    {
        public string FullName { get; set; }
        public string BloodType { get; set; }
        public double Distance { get; set; }
        public string Address { get; set; }
    }
}
