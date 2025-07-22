using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Repositories;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Services.Services;
using Xunit;

namespace VitaFlow.Tests.Services
{
    public class DonationProcessServiceTests
    {
        private readonly Mock<IBloodDonationRepository> _mockBloodDonationRepository;
        private readonly Mock<IBloodInventoryRepository> _mockBloodInventoryRepository;
        private readonly Mock<IBloodRequestRepository> _mockBloodRequestRepository;
        private readonly Mock<IDonorRepository> _mockDonorRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<DonationProcessService>> _mockLogger;
        private readonly DonationProcessService _service;

        public DonationProcessServiceTests()
        {
            _mockBloodDonationRepository = new Mock<IBloodDonationRepository>();
            _mockBloodInventoryRepository = new Mock<IBloodInventoryRepository>();
            _mockBloodRequestRepository = new Mock<IBloodRequestRepository>();
            _mockDonorRepository = new Mock<IDonorRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<DonationProcessService>>();

            _service = new DonationProcessService(
                _mockBloodDonationRepository.Object,
                _mockBloodInventoryRepository.Object,
                _mockBloodRequestRepository.Object,
                _mockDonorRepository.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CompleteDonationAsync_ValidScheduledDonation_ShouldCompleteSuccessfully()
        {
            // Arrange
            var donationId = 1;
            var donorId = 123;
            var donation = new BloodDonation
            {
                Id = donationId,
                DonorId = donorId,
                BloodType = BloodType.A_Positive,
                Status = DonationStatus.Scheduled,
                DonationDate = DateTime.UtcNow.AddDays(1),
                Notes = "Initial notes"
            };

            var newInventory = new BloodInventory
            {
                Id = 1,
                BloodType = BloodType.A_Positive,
                WholeBloodVolume = 450
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);
            _mockBloodDonationRepository.Setup(x => x.UpdateAsync(It.IsAny<BloodDonation>()))
                .Returns(Task.CompletedTask);
            _mockBloodInventoryRepository.Setup(x => x.UpdateInventoryAsync(It.IsAny<BloodType>(), It.IsAny<double>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            _mockBloodInventoryRepository.Setup(x => x.AddAsync(It.IsAny<BloodInventory>()))
                .ReturnsAsync(newInventory);
            _mockNotificationService.Setup(x => x.SendDonationCompletedNotificationAsync(donorId, donationId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CompleteDonationAsync(donationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DonationStatus.Completed, result.Status);
            Assert.Equal(450, result.Volume);
            Assert.Contains("Donation completed on", result.Notes);

            _mockBloodDonationRepository.Verify(x => x.GetByIdAsync(donationId), Times.Once);
            _mockBloodDonationRepository.Verify(x => x.UpdateAsync(It.IsAny<BloodDonation>()), Times.Exactly(2));
            _mockBloodInventoryRepository.Verify(x => x.UpdateInventoryAsync(BloodType.A_Positive, 450, true), Times.Once);
            _mockBloodInventoryRepository.Verify(x => x.AddAsync(It.IsAny<BloodInventory>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendDonationCompletedNotificationAsync(donorId, donationId), Times.Once);
        }

        [Fact]
        public async Task CompleteDonationAsync_DonationNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var donationId = 999;
            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync((BloodDonation)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CompleteDonationAsync(donationId));
        }

        [Fact]
        public async Task CompleteDonationAsync_DonationAlreadyCompleted_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var donationId = 1;
            var donation = new BloodDonation
            {
                Id = donationId,
                Status = DonationStatus.Completed
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CompleteDonationAsync(donationId));
        }

        [Fact]
        public async Task CompleteDonationAsync_NotificationFails_ShouldStillCompleteSuccessfully()
        {
            // Arrange
            var donationId = 1;
            var donorId = 123;
            var donation = new BloodDonation
            {
                Id = donationId,
                DonorId = donorId,
                BloodType = BloodType.O_Negative,
                Status = DonationStatus.Scheduled,
                DonationDate = DateTime.UtcNow.AddDays(1),
                Notes = "Initial notes"
            };

            var newInventory = new BloodInventory
            {
                Id = 1,
                BloodType = BloodType.O_Negative,
                WholeBloodVolume = 450
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);
            _mockBloodDonationRepository.Setup(x => x.UpdateAsync(It.IsAny<BloodDonation>()))
                .Returns(Task.CompletedTask);
            _mockBloodInventoryRepository.Setup(x => x.UpdateInventoryAsync(It.IsAny<BloodType>(), It.IsAny<double>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            _mockBloodInventoryRepository.Setup(x => x.AddAsync(It.IsAny<BloodInventory>()))
                .ReturnsAsync(newInventory);
            _mockNotificationService.Setup(x => x.SendDonationCompletedNotificationAsync(donorId, donationId))
                .ThrowsAsync(new Exception("Notification service failed"));

            // Act
            var result = await _service.CompleteDonationAsync(donationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DonationStatus.Completed, result.Status);
            // Donation should still be completed despite notification failure
            _mockBloodDonationRepository.Verify(x => x.UpdateAsync(It.IsAny<BloodDonation>()), Times.Exactly(2));
        }

        #region ScheduleDonationAsync Tests

        [Fact]
        public async Task ScheduleDonationAsync_ValidDonorAndDate_ShouldScheduleSuccessfully()
        {
            // Arrange
            var donorId = 123;
            var scheduledDate = DateTime.Now.AddDays(7);
            var donor = new Donor
            {
                Id = donorId,
                BloodType = BloodType.B_Positive,
                FirstName = "John",
                LastName = "Doe"
            };

            var createdDonation = new BloodDonation
            {
                Id = 1,
                DonorId = donorId,
                BloodType = BloodType.B_Positive,
                Status = DonationStatus.Scheduled,
                DonationDate = scheduledDate
            };

            _mockDonorRepository.Setup(x => x.GetByIdAsync(donorId))
                .ReturnsAsync(donor);
            _mockBloodDonationRepository.Setup(x => x.AddAsync(It.IsAny<BloodDonation>()))
                .ReturnsAsync(createdDonation);
            _mockNotificationService.Setup(x => x.SendDonationReminderAsync(donorId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ScheduleDonationAsync(donorId, scheduledDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DonationStatus.Scheduled, result.Status);
            Assert.Equal(donorId, result.DonorId);
            Assert.Equal(BloodType.B_Positive, result.BloodType);
            Assert.Equal(scheduledDate, result.DonationDate);

            _mockDonorRepository.Verify(x => x.GetByIdAsync(donorId), Times.Once);
            _mockBloodDonationRepository.Verify(x => x.AddAsync(It.IsAny<BloodDonation>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendDonationReminderAsync(donorId), Times.Once);
        }

        [Fact]
        public async Task ScheduleDonationAsync_DonorNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var donorId = 999;
            var scheduledDate = DateTime.Now.AddDays(7);

            _mockDonorRepository.Setup(x => x.GetByIdAsync(donorId))
                .ReturnsAsync((Donor)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.ScheduleDonationAsync(donorId, scheduledDate));
        }

        [Fact]
        public async Task ScheduleDonationAsync_PastDate_ShouldThrowArgumentException()
        {
            // Arrange
            var donorId = 123;
            var pastDate = DateTime.Now.AddDays(-1);
            var donor = new Donor { Id = donorId, BloodType = BloodType.A_Positive };

            _mockDonorRepository.Setup(x => x.GetByIdAsync(donorId))
                .ReturnsAsync(donor);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.ScheduleDonationAsync(donorId, pastDate));
            Assert.Equal("scheduledDate", exception.ParamName);
            Assert.Contains("must be in the future", exception.Message);
        }

        [Fact]
        public async Task ScheduleDonationAsync_DateTooFarInFuture_ShouldThrowArgumentException()
        {
            // Arrange
            var donorId = 123;
            var farFutureDate = DateTime.Now.AddMonths(7);
            var donor = new Donor { Id = donorId, BloodType = BloodType.A_Positive };

            _mockDonorRepository.Setup(x => x.GetByIdAsync(donorId))
                .ReturnsAsync(donor);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.ScheduleDonationAsync(donorId, farFutureDate));
            Assert.Equal("scheduledDate", exception.ParamName);
            Assert.Contains("cannot be more than 6 months in the future", exception.Message);
        }

        [Fact]
        public async Task ScheduleDonationAsync_NotificationFails_ShouldStillScheduleSuccessfully()
        {
            // Arrange
            var donorId = 123;
            var scheduledDate = DateTime.Now.AddDays(7);
            var donor = new Donor
            {
                Id = donorId,
                BloodType = BloodType.O_Positive,
                FirstName = "Jane",
                LastName = "Smith"
            };

            var createdDonation = new BloodDonation
            {
                Id = 2,
                DonorId = donorId,
                BloodType = BloodType.O_Positive,
                Status = DonationStatus.Scheduled,
                DonationDate = scheduledDate
            };

            _mockDonorRepository.Setup(x => x.GetByIdAsync(donorId))
                .ReturnsAsync(donor);
            _mockBloodDonationRepository.Setup(x => x.AddAsync(It.IsAny<BloodDonation>()))
                .ReturnsAsync(createdDonation);
            _mockNotificationService.Setup(x => x.SendDonationReminderAsync(donorId))
                .ThrowsAsync(new Exception("Notification service failed"));

            // Act
            var result = await _service.ScheduleDonationAsync(donorId, scheduledDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DonationStatus.Scheduled, result.Status);
            // Donation should still be scheduled despite notification failure
            _mockBloodDonationRepository.Verify(x => x.AddAsync(It.IsAny<BloodDonation>()), Times.Once);
        }

        #endregion

        #region CancelDonationAsync Tests

        [Fact]
        public async Task CancelDonationAsync_ValidDonationAndReason_ShouldCancelSuccessfully()
        {
            // Arrange
            var donationId = 1;
            var donorId = 123;
            var reason = "Donor is feeling unwell";
            var donation = new BloodDonation
            {
                Id = donationId,
                DonorId = donorId,
                BloodType = BloodType.A_Positive,
                Status = DonationStatus.Scheduled,
                Notes = "Initial notes"
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);
            _mockBloodDonationRepository.Setup(x => x.UpdateAsync(It.IsAny<BloodDonation>()))
                .Returns(Task.CompletedTask);
            _mockNotificationService.Setup(x => x.SendDonationReminderAsync(donorId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.CancelDonationAsync(donationId, reason);

            // Assert
            Assert.Equal(DonationStatus.Canceled, donation.Status);
            Assert.Contains("Donation cancelled on", donation.Notes);
            Assert.Contains(reason, donation.Notes);

            _mockBloodDonationRepository.Verify(x => x.GetByIdAsync(donationId), Times.Once);
            _mockBloodDonationRepository.Verify(x => x.UpdateAsync(It.IsAny<BloodDonation>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendDonationReminderAsync(donorId), Times.Once);
        }

        [Fact]
        public async Task CancelDonationAsync_DonationNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var donationId = 999;
            var reason = "Some reason";

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync((BloodDonation)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CancelDonationAsync(donationId, reason));
        }

        [Fact]
        public async Task CancelDonationAsync_EmptyReason_ShouldThrowArgumentException()
        {
            // Arrange
            var donationId = 1;
            var emptyReason = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CancelDonationAsync(donationId, emptyReason));
            Assert.Equal("reason", exception.ParamName);
            Assert.Contains("Cancellation reason is required", exception.Message);
        }

        [Fact]
        public async Task CancelDonationAsync_NullReason_ShouldThrowArgumentException()
        {
            // Arrange
            var donationId = 1;
            string nullReason = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CancelDonationAsync(donationId, nullReason));
            Assert.Equal("reason", exception.ParamName);
        }

        [Fact]
        public async Task CancelDonationAsync_CompletedDonation_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var donationId = 1;
            var reason = "Some reason";
            var donation = new BloodDonation
            {
                Id = donationId,
                Status = DonationStatus.Completed
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CancelDonationAsync(donationId, reason));
            Assert.Contains("Cannot cancel a completed donation", exception.Message);
        }

        [Fact]
        public async Task CancelDonationAsync_AlreadyCancelledDonation_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var donationId = 1;
            var reason = "Some reason";
            var donation = new BloodDonation
            {
                Id = donationId,
                Status = DonationStatus.Canceled
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CancelDonationAsync(donationId, reason));
            Assert.Contains("Donation is already cancelled", exception.Message);
        }

        [Fact]
        public async Task CancelDonationAsync_NotificationFails_ShouldStillCancelSuccessfully()
        {
            // Arrange
            var donationId = 1;
            var donorId = 123;
            var reason = "Emergency situation";
            var donation = new BloodDonation
            {
                Id = donationId,
                DonorId = donorId,
                BloodType = BloodType.O_Negative,
                Status = DonationStatus.Scheduled,
                Notes = "Initial notes"
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);
            _mockBloodDonationRepository.Setup(x => x.UpdateAsync(It.IsAny<BloodDonation>()))
                .Returns(Task.CompletedTask);
            _mockNotificationService.Setup(x => x.SendDonationReminderAsync(donorId))
                .ThrowsAsync(new Exception("Notification service failed"));

            // Act
            await _service.CancelDonationAsync(donationId, reason);

            // Assert
            Assert.Equal(DonationStatus.Canceled, donation.Status);
            Assert.Contains(reason, donation.Notes);
            // Donation should still be cancelled despite notification failure
            _mockBloodDonationRepository.Verify(x => x.UpdateAsync(It.IsAny<BloodDonation>()), Times.Once);
        }

        #endregion

        #region AssignDonationToRequestAsync Tests

        [Fact]
        public async Task AssignDonationToRequestAsync_ValidDonationAndRequest_ShouldAssignSuccessfully()
        {
            // Arrange
            var donationId = 1;
            var requestId = 10;
            var donorId = 123;
            var recipientId = 456;

            var donation = new BloodDonation
            {
                Id = donationId,
                DonorId = donorId,
                BloodType = BloodType.APositive,
                Status = DonationStatus.Completed,
                Volume = 450,
                Notes = "Initial notes"
            };

            var bloodRequest = new BloodRequest
            {
                Id = requestId,
                RecipientId = recipientId,
                RequiredBloodType = BloodType.APositive,
                VolumeNeeded = 400,
                Status = RequestStatus.New,
                IsWholeBloodNeeded = true
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);
            _mockBloodRequestRepository.Setup(x => x.GetByIdAsync(requestId))
                .ReturnsAsync(bloodRequest);
            _mockBloodInventoryRepository.Setup(x => x.GetTotalVolumeByBloodTypeAsync(BloodType.APositive, true))
                .ReturnsAsync(500.0);
            _mockBloodDonationRepository.Setup(x => x.UpdateAsync(It.IsAny<BloodDonation>()))
                .Returns(Task.CompletedTask);
            _mockBloodRequestRepository.Setup(x => x.UpdateRequestStatusAsync(requestId, RequestStatus.InProgress))
                .Returns(Task.CompletedTask);
            _mockBloodDonationRepository.Setup(x => x.GetDonationsByRequestIdAsync(requestId))
                .ReturnsAsync(new List<BloodDonation> { donation });
            _mockBloodRequestRepository.Setup(x => x.UpdateRequestStatusAsync(requestId, RequestStatus.Fulfilled))
                .Returns(Task.CompletedTask);
            _mockNotificationService.Setup(x => x.SendRequestStatusUpdateAsync(recipientId, requestId))
                .Returns(Task.CompletedTask);
            _mockNotificationService.Setup(x => x.SendDonationReminderAsync(donorId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AssignDonationToRequestAsync(donationId, requestId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(requestId, result.BloodRequestId);
            Assert.Contains($"Assigned to blood request {requestId}", result.Notes);

            _mockBloodDonationRepository.Verify(x => x.GetByIdAsync(donationId), Times.Once);
            _mockBloodRequestRepository.Verify(x => x.GetByIdAsync(requestId), Times.Once);
            _mockBloodDonationRepository.Verify(x => x.UpdateAsync(It.IsAny<BloodDonation>()), Times.Once);
            _mockBloodRequestRepository.Verify(x => x.UpdateRequestStatusAsync(requestId, RequestStatus.InProgress), Times.Once);
            _mockNotificationService.Verify(x => x.SendRequestStatusUpdateAsync(recipientId, requestId), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AssignDonationToRequestAsync_DonationNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var donationId = 999;
            var requestId = 10;

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync((BloodDonation)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AssignDonationToRequestAsync(donationId, requestId));
        }

        [Fact]
        public async Task AssignDonationToRequestAsync_IncompatibleBloodTypes_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var donationId = 1;
            var requestId = 10;

            var donation = new BloodDonation
            {
                Id = donationId,
                BloodType = BloodType.BPositive,
                Status = DonationStatus.Completed,
                Volume = 450
            };

            var bloodRequest = new BloodRequest
            {
                Id = requestId,
                RequiredBloodType = BloodType.APositive,
                Status = RequestStatus.New,
                VolumeNeeded = 400
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);
            _mockBloodRequestRepository.Setup(x => x.GetByIdAsync(requestId))
                .ReturnsAsync(bloodRequest);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AssignDonationToRequestAsync(donationId, requestId));
            Assert.Contains("Blood type incompatibility", exception.Message);
        }

        [Fact]
        public async Task AssignDonationToRequestAsync_UniversalDonor_ShouldAssignSuccessfully()
        {
            // Arrange
            var donationId = 1;
            var requestId = 10;
            var donorId = 123;
            var recipientId = 456;

            var donation = new BloodDonation
            {
                Id = donationId,
                DonorId = donorId,
                BloodType = BloodType.ONegative, // Universal donor
                Status = DonationStatus.Completed,
                Volume = 450,
                Notes = "Initial notes"
            };

            var bloodRequest = new BloodRequest
            {
                Id = requestId,
                RecipientId = recipientId,
                RequiredBloodType = BloodType.APositive, // Should be compatible
                VolumeNeeded = 400,
                Status = RequestStatus.New,
                IsWholeBloodNeeded = true
            };

            _mockBloodDonationRepository.Setup(x => x.GetByIdAsync(donationId))
                .ReturnsAsync(donation);
            _mockBloodRequestRepository.Setup(x => x.GetByIdAsync(requestId))
                .ReturnsAsync(bloodRequest);
            _mockBloodInventoryRepository.Setup(x => x.GetTotalVolumeByBloodTypeAsync(BloodType.ONegative, true))
                .ReturnsAsync(500.0);
            _mockBloodDonationRepository.Setup(x => x.UpdateAsync(It.IsAny<BloodDonation>()))
                .Returns(Task.CompletedTask);
            _mockBloodRequestRepository.Setup(x => x.UpdateRequestStatusAsync(requestId, RequestStatus.InProgress))
                .Returns(Task.CompletedTask);
            _mockBloodDonationRepository.Setup(x => x.GetDonationsByRequestIdAsync(requestId))
                .ReturnsAsync(new List<BloodDonation> { donation });
            _mockBloodRequestRepository.Setup(x => x.UpdateRequestStatusAsync(requestId, RequestStatus.Fulfilled))
                .Returns(Task.CompletedTask);
            _mockNotificationService.Setup(x => x.SendRequestStatusUpdateAsync(recipientId, requestId))
                .Returns(Task.CompletedTask);
            _mockNotificationService.Setup(x => x.SendDonationReminderAsync(donorId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AssignDonationToRequestAsync(donationId, requestId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(requestId, result.BloodRequestId);
            _mockBloodDonationRepository.Verify(x => x.UpdateAsync(It.IsAny<BloodDonation>()), Times.Once);
        }

        #endregion

        #region CreateBloodRequestAsync Tests

        [Fact]
        public async Task CreateBloodRequestAsync_ValidRequest_ShouldCreateSuccessfully()
        {
            // Arrange
            var recipientId = 456;
            var request = new BloodRequest
            {
                RecipientId = recipientId,
                RequiredBloodType = BloodType.APositive,
                VolumeNeeded = 500,
                IsWholeBloodNeeded = true,
                IsEmergency = false,
                RequiredByDate = DateTime.UtcNow.AddDays(3),
                MedicalNotes = "Patient needs blood for surgery"
            };

            var createdRequest = new BloodRequest
            {
                Id = 1,
                RecipientId = recipientId,
                RequiredBloodType = BloodType.APositive,
                VolumeNeeded = 500,
                Status = RequestStatus.New,
                RequestDate = DateTime.UtcNow,
                IsWholeBloodNeeded = true,
                MedicalNotes = "Patient needs blood for surgery"
            };

            _mockBloodRequestRepository.Setup(x => x.AddAsync(It.IsAny<BloodRequest>()))
                .ReturnsAsync(createdRequest);
            _mockNotificationService.Setup(x => x.SendRequestStatusUpdateAsync(recipientId, 1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateBloodRequestAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RequestStatus.New, result.Status);
            Assert.Equal(recipientId, result.RecipientId);
            Assert.Equal(500, result.VolumeNeeded);
            Assert.True(result.IsWholeBloodNeeded);

            _mockBloodRequestRepository.Verify(x => x.AddAsync(It.IsAny<BloodRequest>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendRequestStatusUpdateAsync(recipientId, 1), Times.Once);
        }

        [Fact]
        public async Task CreateBloodRequestAsync_EmergencyRequest_ShouldSendEmergencyNotifications()
        {
            // Arrange
            var recipientId = 456;
            var request = new BloodRequest
            {
                RecipientId = recipientId,
                RequiredBloodType = BloodType.ONegative,
                VolumeNeeded = 800,
                IsWholeBloodNeeded = true,
                IsEmergency = true,
                MedicalNotes = "Emergency surgery required"
            };

            var createdRequest = new BloodRequest
            {
                Id = 2,
                RecipientId = recipientId,
                RequiredBloodType = BloodType.ONegative,
                VolumeNeeded = 800,
                Status = RequestStatus.New,
                IsEmergency = true,
                MedicalNotes = "Emergency surgery required"
            };

            var compatibleDonors = new List<Donor>
            {
                new Donor { Id = 1, BloodType = BloodType.ONegative },
                new Donor { Id = 2, BloodType = BloodType.ONegative }
            };

            _mockBloodRequestRepository.Setup(x => x.AddAsync(It.IsAny<BloodRequest>()))
                .ReturnsAsync(createdRequest);
            _mockDonorRepository.Setup(x => x.GetAvailableDonorsByBloodTypeAsync(BloodType.ONegative))
                .ReturnsAsync(compatibleDonors);
            _mockNotificationService.Setup(x => x.SendEmergencyRequestAsync(It.IsAny<IEnumerable<int>>()))
                .Returns(Task.CompletedTask);
            _mockNotificationService.Setup(x => x.SendRequestStatusUpdateAsync(recipientId, 2))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateBloodRequestAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsEmergency);

            _mockDonorRepository.Verify(x => x.GetAvailableDonorsByBloodTypeAsync(BloodType.ONegative), Times.Once);
            _mockNotificationService.Verify(x => x.SendEmergencyRequestAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == 2)), Times.Once);
            _mockNotificationService.Verify(x => x.SendRequestStatusUpdateAsync(recipientId, 2), Times.Once);
        }

        [Fact]
        public async Task CreateBloodRequestAsync_NullRequest_ShouldThrowArgumentNullException()
        {
            // Arrange
            BloodRequest nullRequest = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateBloodRequestAsync(nullRequest));
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public async Task CreateBloodRequestAsync_InvalidVolume_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new BloodRequest
            {
                RecipientId = 456,
                RequiredBloodType = BloodType.APositive,
                VolumeNeeded = 0 // Invalid
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateBloodRequestAsync(request));
            Assert.Equal("request", exception.ParamName);
            Assert.Contains("Volume needed must be greater than 0", exception.Message);
        }

        [Fact]
        public async Task CreateBloodRequestAsync_EmergencyWithoutMedicalNotes_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new BloodRequest
            {
                RecipientId = 456,
                RequiredBloodType = BloodType.APositive,
                VolumeNeeded = 500,
                IsEmergency = true,
                MedicalNotes = "" // Empty for emergency
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateBloodRequestAsync(request));
            Assert.Equal("request", exception.ParamName);
            Assert.Contains("Emergency requests must include medical notes", exception.Message);
        }

        #endregion
    }
}
