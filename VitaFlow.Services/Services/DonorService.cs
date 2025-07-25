using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Infrastructure.Repositories.Interfaces;
using VitaFlow.Core.Common;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Services.Services
{
    // Concrete implementation of donor business logic with caching, logging, error handling, and performance monitoring.
    public class DonorService : IDonorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DonorService> _logger;
        private readonly IValidator<Donor> _validator;
        private const string DonorCacheKey = "Donor_{0}";
        private const string AllDonorsCacheKey = "AllDonors";

        public DonorService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<DonorService> logger, IValidator<Donor> validator)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
            _validator = validator;
        }

        public async Task<Donor> GetDonorByIdAsync(Guid id)
        {
            var cacheKey = string.Format(DonorCacheKey, id);
            if (_cache.TryGetValue(cacheKey, out Donor? donor) && donor != null)
            {
                _logger.LogInformation($"Cache hit for donor {id}");
                return donor;
            }
            var sw = Stopwatch.StartNew();
            try
            {
                var repo = _unitOfWork.GetRepository<Donor>();
                donor = await repo.GetByIdAsync(id);
                if (donor == null)
                {
                    _logger.LogWarning($"Donor with id {id} not found.");
                    throw new KeyNotFoundException($"Donor with id {id} not found.");
                }
                _cache.Set(cacheKey, donor, TimeSpan.FromMinutes(10));
                return donor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving donor {id}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetDonorByIdAsync({id}) executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<IEnumerable<Donor>> GetAllDonorsAsync()
        {
            if (_cache.TryGetValue(AllDonorsCacheKey, out IEnumerable<Donor>? donors) && donors != null)
            {
                _logger.LogInformation("Cache hit for all donors");
                return donors;
            }
            var sw = Stopwatch.StartNew();
            try
            {
                var repo = _unitOfWork.GetRepository<Donor>();
                donors = await repo.GetListAsync();
                _cache.Set(AllDonorsCacheKey, donors, TimeSpan.FromMinutes(10));
                return donors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all donors");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetAllDonorsAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<Donor> RegisterDonorAsync(Donor donor)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var validationResult = await _validator.ValidateAsync(donor);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validation failed for donor registration");
                    throw new ValidationException(validationResult.Errors);
                }
                Donor created = null;
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<Donor>();
                    await repo.InsertAsync(donor);
                    created = donor;
                });
                _cache.Remove(AllDonorsCacheKey);
                _logger.LogInformation($"Donor registered with id {created.Id}");
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering donor");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"RegisterDonorAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task UpdateDonorAsync(Donor donor)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var validationResult = await _validator.ValidateAsync(donor);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validation failed for donor update");
                    throw new ValidationException(validationResult.Errors);
                }
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<Donor>();
                    await repo.UpdateAsync(donor);
                });
                _cache.Remove(string.Format(DonorCacheKey, donor.Id));
                _cache.Remove(AllDonorsCacheKey);
                _logger.LogInformation($"Donor updated with id {donor.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating donor");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"UpdateDonorAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        // NOTE: Cần bổ sung property LocationId (Guid) và Location (Location) vào entity Donor để hỗ trợ lọc theo vị trí.

        // Tìm donor phù hợp với blood type của người nhận theo quy tắc truyền máu quốc tế và còn hoạt động
        public async Task<IEnumerable<Donor>> FindCompatibleDonorsAsync(BloodType recipientBloodType)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var repo = _unitOfWork.GetRepository<Donor>();
                var donors = await repo.GetListAsync(predicate: d => d.IsActive);
                // Lọc donor phù hợp theo quy tắc truyền máu quốc tế
                var compatibleDonors = donors.Where(d => IsCompatible(d.BloodType, recipientBloodType));
                return compatibleDonors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding compatible donors");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"FindCompatibleDonorsAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        // Hàm kiểm tra compatibility theo quy tắc truyền máu quốc tế
        private bool IsCompatible(BloodType donor, BloodType recipient)
        {
            // Tách nhóm máu và Rh
            string donorType = donor.ToString();
            string recipientType = recipient.ToString();
            bool donorRh = donorType.EndsWith("+");
            bool recipientRh = recipientType.EndsWith("+");
            string donorABO = donorType.TrimEnd('+', '-');
            string recipientABO = recipientType.TrimEnd('+', '-');

            // Rh compatibility
            if (donorRh && !recipientRh) return false; // Rh+ không cho Rh-

            // ABO compatibility
            if (donorABO == "O") return true;
            if (donorABO == "A" && (recipientABO == "A" || recipientABO == "AB")) return true;
            if (donorABO == "B" && (recipientABO == "B" || recipientABO == "AB")) return true;
            if (donorABO == "AB" && recipientABO == "AB") return true;
            return false;
        }

        // Tìm donor gần vị trí chỉ định (dùng công thức Haversine, chỉ donor còn hoạt động và có Location hợp lệ)
        public async Task<IEnumerable<Donor>> FindNearbyDonorsAsync(double latitude, double longitude, double radiusInKm)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var repo = _unitOfWork.GetRepository<Donor>();
                var donors = await repo.GetListAsync(predicate: d => d.IsActive);
                // Lọc donor có Location hợp lệ và trong bán kính radiusInKm
                var nearbyDonors = donors.Where(d =>
                    d.Location != null &&
                    GeoUtils.CalculateDistance(latitude, longitude, d.Location.Latitude, d.Location.Longitude) <= radiusInKm
                );
                return nearbyDonors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby donors");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"FindNearbyDonorsAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        // Cập nhật ngày donor sẵn sàng hiến máu tiếp theo (ví dụ: sau 3 tháng)
        public async Task UpdateDonorAvailabilityAsync(int donorId)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var repo = _unitOfWork.GetRepository<Donor>();
                // Lấy donor theo id
                var donor = await repo.GetByIdAsync(new Guid(donorId.ToString()));
                if (donor == null)
                {
                    _logger.LogWarning($"Donor with id {donorId} not found.");
                    throw new KeyNotFoundException($"Donor with id {donorId} not found.");
                }
                // Cập nhật ngày sẵn sàng hiến máu tiếp theo (ví dụ: 3 tháng kể từ hôm nay)
                donor.NextAvailableDate = DateTime.UtcNow.AddMonths(3);
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    repo.UpdateAsync(donor);
                });
                _cache.Remove(string.Format(DonorCacheKey, donorId));
                _logger.LogInformation($"Donor availability updated for id {donorId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating donor availability");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"UpdateDonorAvailabilityAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<IEnumerable<BloodDonation>> GetDonorHistoryAsync(int donorId)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Since GetDonorHistoryAsync seems to be missing from the interface, use a placeholder.
                // Assuming we'd need to implement this or extend the repository interface.

                // Using Task.Delay to make the method truly async
                await Task.Delay(1); // Just to ensure the method is actually async

                var history = new List<BloodDonation>();
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donor history");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetDonorHistoryAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public Task<Donor> GetDonorByIdAsync(int id)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<Donor>();
                var donor = repo.GetByIdAsync(new Guid(id.ToString()));
                if (donor == null)
                {
                    _logger.LogWarning($"Donor with id {id} not found.");
                    throw new KeyNotFoundException($"Donor with id {id} not found.");
                }
                return donor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby donors");
                throw;
            }
        }
    }
}
