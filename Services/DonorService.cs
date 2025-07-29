using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.DTOs;
using Models;
using Repositories.Interfaces;
using static Models.DTOs.DonorDTO;
using Services;
using Microsoft.AspNetCore.Mvc;
using Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class DonorService : IDonorService
    {
        private readonly IGenericRepository<Donor> _repository;
        private readonly IGenericRepository<BloodType> _bloodTypeRepository;

        public DonorService(IGenericRepository<Donor> repository, IGenericRepository<BloodType> bloodTypeRepository)
        {
            _repository = repository;
            _bloodTypeRepository = bloodTypeRepository;
        }

        public async Task<IEnumerable<DonorDto>> GetAllAsync()
        {
            var donors = await _repository.GetAllAsync();
            return donors.Select(d => new DonorDto
            {
                DonorId = d.DonorId,
                UserId = d.UserId,
                BloodTypeId = d.BloodTypeId,
                IsAvailable = d.IsAvailable,
                LastDonationDate = d.LastDonationDate,
                NextEligibleDate = d.NextEligibleDate,
                LocationId = d.LocationId,
                ClosestFacilityId = d.ClosestFacilityId
            });
        }

        public async Task<DonorDto> GetByIdAsync(Guid id)
        {
            var d = await _repository.GetByIdAsync(id);
            if (d == null) return null;

            return new DonorDto
            {
                DonorId = d.DonorId,
                UserId = d.UserId,
                BloodTypeId = d.BloodTypeId,
                IsAvailable = d.IsAvailable,
                LastDonationDate = d.LastDonationDate,
                NextEligibleDate = d.NextEligibleDate,
                LocationId = d.LocationId,
                ClosestFacilityId = d.ClosestFacilityId
            };
        }

        public async Task<DonorDto> CreateAsync(CreateDonorDto dto)
        {
            var entity = new Donor
            {
                DonorId = Guid.NewGuid(),
                UserId = dto.UserId,
                BloodTypeId = dto.BloodTypeId,
                IsAvailable = dto.IsAvailable,
                LastDonationDate = dto.LastDonationDate,
                NextEligibleDate = dto.NextEligibleDate,
                LocationId = dto.LocationId,
                ClosestFacilityId = dto.ClosestFacilityId
            };

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return new DonorDto
            {
                DonorId = entity.DonorId,
                UserId = entity.UserId,
                BloodTypeId = entity.BloodTypeId,
                IsAvailable = entity.IsAvailable,
                LastDonationDate = entity.LastDonationDate,
                NextEligibleDate = entity.NextEligibleDate,
                LocationId = entity.LocationId,
                ClosestFacilityId = entity.ClosestFacilityId
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateDonorDto dto)
        {
            var donor = await _repository.GetByIdAsync(id);
            if (donor == null) return false;

            donor.UserId = dto.UserId;
            donor.BloodTypeId = dto.BloodTypeId;
            donor.IsAvailable = dto.IsAvailable;
            donor.LastDonationDate = dto.LastDonationDate;
            donor.NextEligibleDate = dto.NextEligibleDate;
            donor.LocationId = dto.LocationId;
            donor.ClosestFacilityId = dto.ClosestFacilityId;

            _repository.Update(donor);
            return await _repository.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var donor = await _repository.GetByIdAsync(id);
            if (donor == null) return false;

            _repository.Remove(donor);
            return await _repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<BloodType>> GetBloodTypesAsync()
        {
            return await _bloodTypeRepository.GetAllAsync();
        }

        public async Task<IEnumerable<NearbyDonorDto>> GetNearbyDonorsAsync(double latitude, double longitude, double radiusInKm)
        {
            // Use repository method instead of reflection
            var query = await _repository.GetQueryableAsync();
            var donors = await query
                .Include(d => d.Location)
                .Include(d => d.BloodType)
                .Include(d => d.User)
                .Where(d => d.Location != null && d.Location.Latitude != null && d.Location.Longitude != null)
                .ToListAsync();

            var nearbyDonors = new List<NearbyDonorDto>();

            foreach (var donor in donors)
            {
                if (donor.Location?.Latitude == null || donor.Location?.Longitude == null)
                    continue;

                var distance = CalculateDistance(
                    latitude,
                    longitude,
                    donor.Location.Latitude.Value,
                    donor.Location.Longitude.Value
                );

                if (distance <= radiusInKm)
                {
                    nearbyDonors.Add(new NearbyDonorDto
                    {
                        FullName = donor.User?.FullName ?? donor.FullName ?? "Anonymous",
                        BloodType = donor.BloodType != null ? $"{donor.BloodType.AboType}{donor.BloodType.RhFactor}" : "Unknown",
                        Distance = Math.Round(distance, 2),
                        Address = donor.Location.Address ?? "No address provided"
                    });
                }
            }

            return nearbyDonors.OrderBy(d => d.Distance);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Distance in kilometers
        }

        private double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
