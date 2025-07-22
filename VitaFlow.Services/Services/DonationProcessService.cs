using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Repositories;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Services.Services
{
    /// <summary>
    /// Implementation of the IDonationProcessService interface.
    /// </summary>
    public class DonationProcessService : IDonationProcessService
    {
        private readonly IBloodDonationRepository _bloodDonationRepository;
        private readonly IBloodInventoryRepository _bloodInventoryRepository;
        private readonly IBloodRequestRepository _bloodRequestRepository;
        private readonly IDonorRepository _donorRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DonationProcessService> _logger;

        // Constructor for dependency injection
        public DonationProcessService(
            IBloodDonationRepository bloodDonationRepository,
            IBloodInventoryRepository bloodInventoryRepository,
            IBloodRequestRepository bloodRequestRepository,
            IDonorRepository donorRepository,
            INotificationService notificationService,
            ILogger<DonationProcessService> logger)
        {
            _bloodDonationRepository = bloodDonationRepository ?? throw new ArgumentNullException(nameof(bloodDonationRepository));
            _bloodInventoryRepository = bloodInventoryRepository ?? throw new ArgumentNullException(nameof(bloodInventoryRepository));
            _bloodRequestRepository = bloodRequestRepository ?? throw new ArgumentNullException(nameof(bloodRequestRepository));
            _donorRepository = donorRepository ?? throw new ArgumentNullException(nameof(donorRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<BloodDonation> ScheduleDonationAsync(int donorId, DateTime scheduledDate)
        {
            try
            {
                _logger.LogInformation($"Scheduling donation for donor {donorId} on {scheduledDate}");

                // Validate donor exists
                var donor = await _donorRepository.GetByIdAsync(donorId);
                if (donor == null)
                {
                    _logger.LogWarning($"Donor with id {donorId} not found");
                    throw new KeyNotFoundException($"Donor with id {donorId} not found");
                }

                // Validate scheduled date is in the future
                if (scheduledDate <= DateTime.Now)
                {
                    _logger.LogWarning($"Scheduled date {scheduledDate} must be in the future");
                    throw new ArgumentException("Scheduled date must be in the future", nameof(scheduledDate));
                }

                // Validate scheduled date is not too far in the future (e.g., max 6 months)
                if (scheduledDate > DateTime.Now.AddMonths(6))
                {
                    _logger.LogWarning($"Scheduled date {scheduledDate} is too far in the future");
                    throw new ArgumentException("Scheduled date cannot be more than 6 months in the future", nameof(scheduledDate));
                }

                // Create new BloodDonation record
                var bloodDonation = new BloodDonation
                {
                    DonorId = donorId,
                    Donor = donor,
                    BloodType = donor.BloodType,
                    DonationDate = scheduledDate,
                    Status = DonationStatus.Scheduled,
                    Notes = $"Donation scheduled on {DateTime.Now:yyyy-MM-dd HH:mm} for {scheduledDate:yyyy-MM-dd HH:mm}",
                    Volume = 0 // Will be set when donation is completed
                };

                // Save to database
                var createdDonation = await _bloodDonationRepository.AddAsync(bloodDonation);

                _logger.LogInformation($"Successfully scheduled donation with id {createdDonation.Id} for donor {donorId}");

                // Send notification to donor about scheduled donation
                try
                {
                    await _notificationService.SendDonationReminderAsync(donorId);
                    _logger.LogInformation($"Scheduling notification sent to donor {donorId}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to send scheduling notification to donor {donorId}, but donation scheduling was successful");
                }

                return createdDonation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scheduling donation for donor {donorId}");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<BloodDonation> CompleteDonationAsync(int donationId)
        {
            try
            {
                _logger.LogInformation($"Starting completion process for donation {donationId}");

                // Get the donation record
                var donation = await _bloodDonationRepository.GetByIdAsync(donationId);
                if (donation == null)
                {
                    _logger.LogWarning($"Donation with id {donationId} not found");
                    throw new KeyNotFoundException($"Donation with id {donationId} not found");
                }

                // Validate that the donation can be completed
                if (donation.Status != DonationStatus.Scheduled)
                {
                    _logger.LogWarning($"Donation {donationId} cannot be completed. Current status: {donation.Status}");
                    throw new InvalidOperationException($"Donation cannot be completed. Current status: {donation.Status}");
                }

                // Update donation status to Completed
                donation.Status = DonationStatus.Completed;
                donation.DonationDate = DateTime.UtcNow; // Update to actual completion time
                donation.Volume = 450; // Standard blood donation volume in ml
                donation.Notes = $"{donation.Notes}\nDonation completed on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                // Update the donation in database
                await _bloodDonationRepository.UpdateAsync(donation);
                _logger.LogInformation($"Donation {donationId} status updated to Completed");

                // Update blood inventory
                await UpdateInventoryAsync(donation);

                // Send notification to donor
                try
                {
                    await _notificationService.SendDonationCompletedNotificationAsync(donation.DonorId, donationId);
                    _logger.LogInformation($"Completion notification sent to donor {donation.DonorId}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to send notification to donor {donation.DonorId}, but donation completion was successful");
                }

                _logger.LogInformation($"Successfully completed donation {donationId}");
                return donation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing donation {donationId}");
                throw;
            }
        }

        /// <summary>
        /// Updates the blood inventory after a successful donation
        /// </summary>
        /// <param name="donation">The completed donation</param>
        private async Task UpdateInventoryAsync(BloodDonation donation)
        {
            try
            {
                _logger.LogInformation($"Updating inventory for donation {donation.Id}, blood type {donation.BloodType}");

                // Update inventory using the repository method
                await _bloodInventoryRepository.UpdateInventoryAsync(donation.BloodType, donation.Volume, true);

                // Create new inventory record for this specific donation
                var newInventory = new BloodInventory
                {
                    BloodType = donation.BloodType,
                    WholeBloodVolume = donation.Volume,
                    RedCellsVolume = 0,
                    PlasmaVolume = 0,
                    PlateletsVolume = 0,
                    CollectionDate = donation.DonationDate,
                    ExpiryDate = DateTime.UtcNow.AddDays(42), // Blood typically expires in 42 days
                    IsAvailable = true,
                    StorageLocation = "Main Storage",
                    BatchNumber = $"BATCH-{donation.Id}-{DateTime.UtcNow:yyyyMMdd}"
                };

                var createdInventory = await _bloodInventoryRepository.AddAsync(newInventory);

                // Link the donation to the new inventory item
                donation.InventoryId = createdInventory.Id;
                await _bloodDonationRepository.UpdateAsync(donation);

                _logger.LogInformation($"Inventory successfully updated for blood type {donation.BloodType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating inventory for donation {donation.Id}");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task CancelDonationAsync(int donationId, string reason)
        {
            try
            {
                _logger.LogInformation($"Starting cancellation process for donation {donationId}");

                // Validate reason is provided
                if (string.IsNullOrWhiteSpace(reason))
                {
                    _logger.LogWarning("Cancellation reason cannot be empty");
                    throw new ArgumentException("Cancellation reason is required", nameof(reason));
                }

                // Get the donation record
                var donation = await _bloodDonationRepository.GetByIdAsync(donationId);
                if (donation == null)
                {
                    _logger.LogWarning($"Donation with id {donationId} not found");
                    throw new KeyNotFoundException($"Donation with id {donationId} not found");
                }

                // Validate that the donation can be cancelled
                if (donation.Status == DonationStatus.Completed)
                {
                    _logger.LogWarning($"Donation {donationId} cannot be cancelled. Already completed");
                    throw new InvalidOperationException("Cannot cancel a completed donation");
                }

                if (donation.Status == DonationStatus.Canceled)
                {
                    _logger.LogWarning($"Donation {donationId} is already cancelled");
                    throw new InvalidOperationException("Donation is already cancelled");
                }

                // Update donation status to Cancelled
                donation.Status = DonationStatus.Canceled;
                donation.Notes = $"{donation.Notes}\nDonation cancelled on {DateTime.UtcNow:yyyy-MM-dd HH:mm}. Reason: {reason}";

                // Update the donation in database
                await _bloodDonationRepository.UpdateAsync(donation);
                _logger.LogInformation($"Donation {donationId} status updated to Cancelled");

                // Send notification to donor about cancellation
                try
                {
                    await _notificationService.SendDonationReminderAsync(donation.DonorId);
                    _logger.LogInformation($"Cancellation notification sent to donor {donation.DonorId}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to send cancellation notification to donor {donation.DonorId}, but donation cancellation was successful");
                }

                _logger.LogInformation($"Successfully cancelled donation {donationId}. Reason: {reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling donation {donationId}");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<BloodDonation> AssignDonationToRequestAsync(int donationId, int requestId)
        {
            try
            {
                _logger.LogInformation($"Assigning donation {donationId} to request {requestId}");

                // Get the donation record
                var donation = await _bloodDonationRepository.GetByIdAsync(donationId);
                if (donation == null)
                {
                    _logger.LogWarning($"Donation with id {donationId} not found");
                    throw new KeyNotFoundException($"Donation with id {donationId} not found");
                }

                // Get the blood request record
                var bloodRequest = await _bloodRequestRepository.GetByIdAsync(requestId);
                if (bloodRequest == null)
                {
                    _logger.LogWarning($"Blood request with id {requestId} not found");
                    throw new KeyNotFoundException($"Blood request with id {requestId} not found");
                }

                // Validate donation status - must be Completed to be assignable
                if (donation.Status != DonationStatus.Completed)
                {
                    _logger.LogWarning($"Donation {donationId} cannot be assigned. Status: {donation.Status}. Only completed donations can be assigned");
                    throw new InvalidOperationException($"Only completed donations can be assigned to requests. Current status: {donation.Status}");
                }

                // Validate request status - must be active (New or InProgress)
                if (bloodRequest.Status == RequestStatus.Fulfilled ||
                    bloodRequest.Status == RequestStatus.Canceled ||
                    bloodRequest.Status == RequestStatus.Expired)
                {
                    _logger.LogWarning($"Blood request {requestId} cannot receive assignments. Status: {bloodRequest.Status}");
                    throw new InvalidOperationException($"Blood request cannot receive assignments. Current status: {bloodRequest.Status}");
                }

                // Validate blood type compatibility
                if (!AreBloodTypesCompatible(donation.BloodType, bloodRequest.RequiredBloodType))
                {
                    _logger.LogWarning($"Blood type incompatibility: Donation ({donation.BloodType}) cannot fulfill request ({bloodRequest.RequiredBloodType})");
                    throw new InvalidOperationException($"Blood type incompatibility: {donation.BloodType} cannot fulfill {bloodRequest.RequiredBloodType}");
                }

                // Check if donation is already assigned to another request
                if (donation.BloodRequestId.HasValue)
                {
                    _logger.LogWarning($"Donation {donationId} is already assigned to request {donation.BloodRequestId}");
                    throw new InvalidOperationException($"Donation is already assigned to another request (ID: {donation.BloodRequestId})");
                }

                // Check inventory availability if needed
                await ValidateInventoryAvailabilityAsync(donation, bloodRequest);

                // Assign donation to request
                donation.BloodRequestId = requestId;
                donation.BloodRequest = bloodRequest;
                donation.Notes = $"{donation.Notes}\nAssigned to blood request {requestId} on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                // Update donation
                await _bloodDonationRepository.UpdateAsync(donation);
                _logger.LogInformation($"Donation {donationId} successfully assigned to request {requestId}");

                // Update request status to InProgress if it was New
                if (bloodRequest.Status == RequestStatus.New)
                {
                    await _bloodRequestRepository.UpdateRequestStatusAsync(requestId, RequestStatus.InProgress);
                    _logger.LogInformation($"Blood request {requestId} status updated to InProgress");
                }

                // Check if the request is now fully fulfilled
                await CheckAndUpdateRequestFulfillmentAsync(requestId);

                // Send notifications
                try
                {
                    await _notificationService.SendRequestStatusUpdateAsync(bloodRequest.RecipientId, requestId);
                    _logger.LogInformation($"Assignment notification sent to recipient {bloodRequest.RecipientId}");

                    if (donation.DonorId > 0)
                    {
                        await _notificationService.SendDonationReminderAsync(donation.DonorId);
                        _logger.LogInformation($"Assignment notification sent to donor {donation.DonorId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send assignment notifications, but assignment was successful");
                }

                _logger.LogInformation($"Successfully assigned donation {donationId} to request {requestId}");
                return donation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning donation {donationId} to request {requestId}");
                throw;
            }
        }

        /// <summary>
        /// Validates blood type compatibility between donation and request
        /// </summary>
        /// <param name="donorBloodType">Blood type of the donation</param>
        /// <param name="requiredBloodType">Required blood type for the request</param>
        /// <returns>True if compatible, false otherwise</returns>
        private bool AreBloodTypesCompatible(BloodType donorBloodType, BloodType requiredBloodType)
        {
            // Universal donor (O-) can donate to anyone
            if (donorBloodType == BloodType.ONegative)
                return true;

            // Universal recipient (AB+) can receive from anyone
            if (requiredBloodType == BloodType.ABPositive)
                return true;

            // Exact match is always compatible
            if (donorBloodType == requiredBloodType)
                return true;

            // Additional compatibility rules based on ABO and Rh systems
            return donorBloodType switch
            {
                BloodType.OPositive => requiredBloodType is BloodType.APositive or BloodType.BPositive or BloodType.ABPositive,
                BloodType.ANegative => requiredBloodType is BloodType.APositive or BloodType.ANegative or BloodType.ABPositive or BloodType.ABNegative,
                BloodType.APositive => requiredBloodType is BloodType.APositive or BloodType.ABPositive,
                BloodType.BNegative => requiredBloodType is BloodType.BPositive or BloodType.BNegative or BloodType.ABPositive or BloodType.ABNegative,
                BloodType.BPositive => requiredBloodType is BloodType.BPositive or BloodType.ABPositive,
                BloodType.ABNegative => requiredBloodType is BloodType.ABPositive or BloodType.ABNegative,
                _ => false
            };
        }

        /// <summary>
        /// Validates that the donation has sufficient inventory for the request
        /// </summary>
        /// <param name="donation">The donation to check</param>
        /// <param name="request">The blood request</param>
        private async Task ValidateInventoryAvailabilityAsync(BloodDonation donation, BloodRequest request)
        {
            try
            {
                // Check if donation has sufficient volume for the request
                if (donation.Volume < request.VolumeNeeded)
                {
                    _logger.LogWarning($"Insufficient volume: Donation has {donation.Volume}ml, request needs {request.VolumeNeeded}ml");
                    throw new InvalidOperationException($"Insufficient blood volume: Donation has {donation.Volume}ml, but request needs {request.VolumeNeeded}ml");
                }

                // Check inventory availability for specific blood type
                var totalAvailable = await _bloodInventoryRepository.GetTotalVolumeByBloodTypeAsync(donation.BloodType, request.IsWholeBloodNeeded);

                if (totalAvailable < request.VolumeNeeded)
                {
                    _logger.LogWarning($"Insufficient inventory: Available {totalAvailable}ml, needed {request.VolumeNeeded}ml for blood type {donation.BloodType}");
                    throw new InvalidOperationException($"Insufficient inventory: Only {totalAvailable}ml available, but {request.VolumeNeeded}ml needed");
                }

                _logger.LogInformation($"Inventory validation passed: {totalAvailable}ml available for {request.VolumeNeeded}ml needed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating inventory availability");
                throw;
            }
        }

        /// <summary>
        /// Checks if a blood request is fully fulfilled and updates its status accordingly
        /// </summary>
        /// <param name="requestId">The blood request ID to check</param>
        private async Task CheckAndUpdateRequestFulfillmentAsync(int requestId)
        {
            try
            {
                var request = await _bloodRequestRepository.GetByIdAsync(requestId);
                if (request == null) return;

                // Get all donations assigned to this request
                var assignedDonations = await _bloodDonationRepository.GetDonationsByRequestIdAsync(requestId);
                var totalAssignedVolume = assignedDonations.Where(d => d.Status == DonationStatus.Completed).Sum(d => d.Volume);

                // Check if request is fulfilled
                if (totalAssignedVolume >= request.VolumeNeeded)
                {
                    await _bloodRequestRepository.UpdateRequestStatusAsync(requestId, RequestStatus.Fulfilled);
                    _logger.LogInformation($"Blood request {requestId} marked as fulfilled. Total volume: {totalAssignedVolume}ml");

                    // Send fulfillment notification
                    try
                    {
                        await _notificationService.SendRequestStatusUpdateAsync(request.RecipientId, requestId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to send fulfillment notification for request {requestId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking request fulfillment for request {requestId}");
            }
        }

        /// <inheritdoc />
        public async Task<BloodRequest> CreateBloodRequestAsync(BloodRequest request)
        {
            try
            {
                _logger.LogInformation($"Creating blood request for recipient {request.RecipientId}");

                // Validate input
                if (request == null)
                {
                    _logger.LogWarning("Blood request cannot be null");
                    throw new ArgumentNullException(nameof(request), "Blood request cannot be null");
                }

                // Validate required fields
                if (request.RecipientId <= 0)
                {
                    _logger.LogWarning("Invalid recipient ID");
                    throw new ArgumentException("Valid recipient ID is required", nameof(request));
                }

                if (request.VolumeNeeded <= 0)
                {
                    _logger.LogWarning($"Invalid volume needed: {request.VolumeNeeded}");
                    throw new ArgumentException("Volume needed must be greater than 0", nameof(request));
                }

                if (request.VolumeNeeded > 2000) // Reasonable limit for a single request
                {
                    _logger.LogWarning($"Volume needed is too high: {request.VolumeNeeded}ml");
                    throw new ArgumentException("Volume needed cannot exceed 2000ml for a single request", nameof(request));
                }

                // Validate required by date if provided
                if (request.RequiredByDate.HasValue && request.RequiredByDate <= DateTime.UtcNow)
                {
                    _logger.LogWarning($"Required by date {request.RequiredByDate} must be in the future");
                    throw new ArgumentException("Required by date must be in the future", nameof(request));
                }

                // Set default values and ensure consistency
                request.Status = RequestStatus.New;
                request.RequestDate = DateTime.UtcNow;
                request.Id = 0; // Ensure it's treated as new record

                // Ensure at least one blood component is needed
                if (!request.IsWholeBloodNeeded && !request.IsRedCellsNeeded && 
                    !request.IsPlasmaNeeded && !request.IsPlateletsNeeded)
                {
                    _logger.LogInformation("No specific component specified, defaulting to whole blood");
                    request.IsWholeBloodNeeded = true;
                }

                // Validate medical notes for emergency requests
                if (request.IsEmergency && string.IsNullOrWhiteSpace(request.MedicalNotes))
                {
                    _logger.LogWarning("Emergency requests require medical notes");
                    throw new ArgumentException("Emergency requests must include medical notes", nameof(request));
                }

                // Set emergency priority if required by date is very soon (less than 24 hours)
                if (request.RequiredByDate.HasValue && 
                    request.RequiredByDate <= DateTime.UtcNow.AddHours(24) && 
                    !request.IsEmergency)
                {
                    _logger.LogInformation($"Setting emergency flag for urgent request (required by {request.RequiredByDate})");
                    request.IsEmergency = true;
                }

                // Save to database
                var createdRequest = await _bloodRequestRepository.AddAsync(request);
                _logger.LogInformation($"Blood request created successfully with ID {createdRequest.Id}");

                // Send notifications based on urgency
                try
                {
                    if (createdRequest.IsEmergency)
                    {
                        await SendEmergencyNotificationsAsync(createdRequest);
                    }
                    else
                    {
                        await SendStandardNotificationsAsync(createdRequest);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to send notifications for blood request {createdRequest.Id}, but request creation was successful");
                }

                _logger.LogInformation($"Successfully created blood request {createdRequest.Id} for recipient {request.RecipientId}");
                return createdRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating blood request for recipient {request?.RecipientId}");
                throw;
            }
        }

        /// <summary>
        /// Sends emergency notifications for urgent blood requests
        /// </summary>
        /// <param name="request">The blood request</param>
        private async Task SendEmergencyNotificationsAsync(BloodRequest request)
        {
            try
            {
                _logger.LogInformation($"Sending emergency notifications for blood request {request.Id}");

                // Find compatible donors for emergency notification
                var compatibleDonors = await _donorRepository.GetAvailableDonorsByBloodTypeAsync(request.RequiredBloodType);
                var donorIds = compatibleDonors.Select(d => d.Id).ToList();

                if (donorIds.Any())
                {
                    await _notificationService.SendEmergencyRequestAsync(donorIds);
                    _logger.LogInformation($"Emergency notification sent to {donorIds.Count} compatible donors");
                }
                else
                {
                    _logger.LogWarning($"No compatible donors found for emergency request {request.Id} (blood type: {request.RequiredBloodType})");
                }

                // Send status update to recipient
                await _notificationService.SendRequestStatusUpdateAsync(request.RecipientId, request.Id);
                _logger.LogInformation($"Emergency request confirmation sent to recipient {request.RecipientId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending emergency notifications for request {request.Id}");
                throw;
            }
        }

        /// <summary>
        /// Sends standard notifications for regular blood requests
        /// </summary>
        /// <param name="request">The blood request</param>
        private async Task SendStandardNotificationsAsync(BloodRequest request)
        {
            try
            {
                _logger.LogInformation($"Sending standard notifications for blood request {request.Id}");

                // Send confirmation to recipient
                await _notificationService.SendRequestStatusUpdateAsync(request.RecipientId, request.Id);
                _logger.LogInformation($"Request confirmation sent to recipient {request.RecipientId}");

                // For standard requests, we might want to notify fewer donors or use a different strategy
                // This could be implemented as a background job or scheduled notification
                _logger.LogInformation($"Standard request {request.Id} will be processed by staff for donor matching");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending standard notifications for request {request.Id}");
                throw;
            }
        }

        /// <inheritdoc />
        public Task<BloodRequest> FulfillRequestAsync(int requestId)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }
    }
}
