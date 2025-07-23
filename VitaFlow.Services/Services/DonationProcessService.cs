using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Infrastructure.Repositories.Interfaces;
using VitaFlow.Infrastructure.Common.Exceptions;

namespace VitaFlow.Services.Services
{
    /// <summary>
    /// Implementation of the IDonationProcessService interface.
    /// </summary>
    public class DonationProcessService : IDonationProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DonationProcessService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public async Task<BloodDonation> ScheduleDonationAsync(Guid donorId, DateTime scheduledDate)
        {
            try 
            {
                var donorRepo = _unitOfWork.GetRepository<Donor>();
                var donationRepo = _unitOfWork.GetRepository<BloodDonation>();

                var donor = await donorRepo.GetByIdAsync(donorId);
                if (donor == null)
                {
                    throw new NotFoundException($"Donor with ID {donorId} not found");
                }

                var bloodDonation = new BloodDonation
                {
                    DonorId = donor.Id,
                    BloodType = donor.BloodType,
                    DonationDate = scheduledDate,
                    Status = DonationStatus.Scheduled,
                    Volume = 0, // Will be updated when donation is completed
                };

                await donationRepo.InsertAsync(bloodDonation);
                await _unitOfWork.CommitAsync();

                return bloodDonation;
            }
            catch (Exception ex) when (ex is not NotFoundException)
            {
                throw new Exception($"Error scheduling blood donation for donor {donorId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<BloodDonation> CompleteDonationAsync(Guid donationId)
        {
            try
            {
                var donationRepo = _unitOfWork.GetRepository<BloodDonation>();
                var donation = await donationRepo.GetByIdAsync(donationId);

                if (donation == null)
                {
                    throw new NotFoundException($"Blood donation with ID {donationId} not found");
                }

                if (donation.Status != DonationStatus.Scheduled)
                {
                    throw new InvalidOperationException($"Blood donation {donationId} is not in Scheduled status");
                }

                // Create inventory entry
                var inventoryRepo = _unitOfWork.GetRepository<BloodInventory>();
                var inventory = new BloodInventory
                {
                    BloodType = donation.BloodType,
                    WholeBloodVolume = donation.Volume,
                    CollectionDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(42), // Standard shelf life for whole blood
                    IsAvailable = true
                };

                // Update donation status
                donation.Status = DonationStatus.Completed;

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    await inventoryRepo.InsertAsync(inventory);
                    donationRepo.UpdateAsync(donation);
                });

                // Send notification to donor
                var notificationRepo = _unitOfWork.GetRepository<Notification>();
                var notification = new Notification
                {
                    UserId = donation.DonorId,
                    Title = "Hiến máu hoàn thành",
                    Message = "Cảm ơn bạn đã hoàn thành quy trình hiến máu. Máu của bạn sẽ giúp cứu sống nhiều người!",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await notificationRepo.InsertAsync(notification);

                return donation;
            }
            catch (Exception ex) when (ex is not NotFoundException)
            {
                throw new Exception($"Error completing blood donation {donationId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task CancelDonationAsync(Guid donationId, string reason)
        {
            try
            {
                var donationRepo = _unitOfWork.GetRepository<BloodDonation>();
                var donation = await donationRepo.GetByIdAsync(donationId);

                if (donation == null)
                {
                    throw new NotFoundException($"Blood donation with ID {donationId} not found");
                }

                if (donation.Status == DonationStatus.Completed || donation.Status == DonationStatus.Canceled)
                {
                    throw new InvalidOperationException($"Cannot cancel blood donation {donationId} in {donation.Status} status");
                }

                // Update donation status and notes
                donation.Status = DonationStatus.Canceled;
                donation.Notes = reason;

                await _unitOfWork.ProcessInTransactionAsync(() =>
                {
                    donationRepo.UpdateAsync(donation);
                    return Task.CompletedTask;
                });

                // Send notification to donor
                var notificationRepo = _unitOfWork.GetRepository<Notification>();
                var notification = new Notification
                {
                    UserId = donation.DonorId,
                    Title = "Lịch hẹn hiến máu đã bị hủy",
                    Message = $"Lịch hẹn hiến máu của bạn đã bị hủy với lý do: {reason}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await notificationRepo.InsertAsync(notification);
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not InvalidOperationException)
            {
                throw new Exception($"Error cancelling blood donation {donationId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<BloodDonation> AssignDonationToRequestAsync(Guid donationId, Guid requestId)
        {
            try
            {
                var donationRepo = _unitOfWork.GetRepository<BloodDonation>();
                var requestRepo = _unitOfWork.GetRepository<BloodRequest>();

                var donation = await donationRepo.GetByIdAsync(donationId);
                if (donation == null)
                {
                    throw new NotFoundException($"Blood donation with ID {donationId} not found");
                }

                var request = await requestRepo.GetByIdAsync(requestId);
                if (request == null)
                {
                    throw new NotFoundException($"Blood request with ID {requestId} not found");
                }

                // Validate blood type compatibility
                if (donation.BloodType != request.RequiredBloodType)
                {
                    throw new InvalidOperationException($"Blood type mismatch: Donation is {donation.BloodType}, Request needs {request.RequiredBloodType}");
                }

                // Validate donation status
                if (donation.Status != DonationStatus.Completed)
                {
                    throw new InvalidOperationException($"Blood donation {donationId} is not completed");
                }

                // Validate request status
                if (request.Status != RequestStatus.New && request.Status != RequestStatus.InProgress)
                {
                    throw new InvalidOperationException($"Blood request {requestId} cannot accept donations in {request.Status} status");
                }

                // Associate donation with request
                donation.BloodRequestId = request.Id;

                // Update request status if needed
                request.Status = RequestStatus.InProgress;
                request.AssignedDonations ??= new List<BloodDonation>();
                request.AssignedDonations.Add(donation);

                // Check if request is fulfilled
                var totalAssignedVolume = request.AssignedDonations.Sum(d => d.Volume);
                if (totalAssignedVolume >= request.VolumeNeeded)
                {
                    request.Status = RequestStatus.Fulfilled;
                }

                await _unitOfWork.ProcessInTransactionAsync(() =>
                {
                    donationRepo.UpdateAsync(donation);
                    requestRepo.UpdateAsync(request);
                    return Task.CompletedTask;
                });

                return donation;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not InvalidOperationException)
            {
                throw new Exception($"Error assigning donation {donationId} to request {requestId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<BloodRequest> CreateBloodRequestAsync(BloodRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var recipientRepo = _unitOfWork.GetRepository<Recipient>();
                var recipient = await recipientRepo.GetByIdAsync(request.RecipientId);
                if (recipient == null)
                {
                    throw new NotFoundException($"Recipient with ID {request.RecipientId} not found");
                }

                // Set initial request properties
                request.Status = RequestStatus.New;
                request.RequestDate = DateTime.UtcNow;
                request.RequiredByDate ??= DateTime.UtcNow.AddDays(7); // Default to 7 days if not specified

                var requestRepo = _unitOfWork.GetRepository<BloodRequest>();
                await requestRepo.InsertAsync(request);
                await _unitOfWork.CommitAsync();

                // If it's an emergency request, notify emergency donors
                if (request.IsEmergency)
                {
                    var donorRepo = _unitOfWork.GetRepository<Donor>();
                    var emergencyDonors = await donorRepo.GetListAsync(
                        predicate: d => d.IsEmergencyDonor && 
                                      d.BloodType == request.RequiredBloodType &&
                                      d.IsActive);

                    var notificationRepo = _unitOfWork.GetRepository<Notification>();
                    var notifications = emergencyDonors.Select(d => new Notification
                    {
                        UserId = d.Id,
                        Title = "Yêu cầu máu khẩn cấp",
                        Message = $"Có một yêu cầu máu khẩn cấp phù hợp với nhóm máu của bạn ({request.RequiredBloodType}). Xin hãy liên hệ ngay nếu có thể giúp đỡ.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });

                    await _unitOfWork.ProcessInTransactionAsync(async () =>
                    {
                        foreach (var notification in notifications)
                        {
                            await notificationRepo.InsertAsync(notification);
                        }
                    });
                }

                return request;
            }
            catch (Exception ex) when (ex is not ArgumentNullException && ex is not NotFoundException)
            {
                throw new Exception("Error creating blood request", ex);
            }
        }

        /// <inheritdoc />
        public async Task<BloodRequest> FulfillRequestAsync(Guid requestId)
        {
            try
            {
                var requestRepo = _unitOfWork.GetRepository<BloodRequest>();
                var request = await requestRepo.GetByIdAsync(requestId);

                if (request == null)
                {
                    throw new NotFoundException($"Blood request with ID {requestId} not found");
                }

                if (request.Status == RequestStatus.Fulfilled)
                {
                    throw new InvalidOperationException($"Blood request {requestId} is already fulfilled");
                }

                if (request.Status == RequestStatus.Canceled || request.Status == RequestStatus.Expired)
                {
                    throw new InvalidOperationException($"Cannot fulfill blood request {requestId} in {request.Status} status");
                }

                // Validate that assigned donations meet the requirements
                if (request.AssignedDonations == null || !request.AssignedDonations.Any())
                {
                    throw new InvalidOperationException($"Blood request {requestId} has no assigned donations");
                }

                var totalVolume = request.AssignedDonations.Sum(d => d.Volume);
                if (totalVolume < request.VolumeNeeded)
                {
                    throw new InvalidOperationException($"Blood request {requestId} needs {request.VolumeNeeded}ml but only {totalVolume}ml assigned");
                }

                // Update request status
                request.Status = RequestStatus.Fulfilled;

                await _unitOfWork.ProcessInTransactionAsync(() =>
                {
                    requestRepo.UpdateAsync(request);
                    return Task.CompletedTask;
                });

                // Notify the recipient
                var notificationRepo = _unitOfWork.GetRepository<Notification>();
                var notification = new Notification
                {
                    UserId = request.RecipientId,
                    Title = "Yêu cầu máu đã được hoàn thành",
                    Message = "Yêu cầu máu của bạn đã được hoàn thành. Vui lòng liên hệ nhân viên y tế để biết thêm chi tiết.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await notificationRepo.InsertAsync(notification);

                return request;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not InvalidOperationException)
            {
                throw new Exception($"Error fulfilling blood request {requestId}", ex);
            }
        }
    }
}
