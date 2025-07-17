using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Repositories;
using VitaFlow.Services;
using VitaFlow.Services.Interfaces;
using VitaFlow.Services.Validators;

namespace VitaFlow.Tests.Services
{
    public class DonorServiceTests
    {
        private readonly Mock<IDonorRepository> _donorRepositoryMock;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<DonorService>> _loggerMock;
        private readonly IValidator<Donor> _validator;
        private readonly IDonorService _donorService;

        public DonorServiceTests()
        {
            _donorRepositoryMock = new Mock<IDonorRepository>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<DonorService>>();
            _validator = new DonorValidator();
            _donorService = new DonorService(_donorRepositoryMock.Object, _cache, _loggerMock.Object, _validator);
        }

        [Fact]
        public async Task RegisterDonorAsync_ValidDonor_ReturnsCreatedDonor()
        {
            var donor = new Donor { Id = 1, Name = "John Doe", BloodType = BloodType.O_Positive, Email = "john@example.com", PhoneNumber = "1234567890", DateOfBirth = DateTime.Now.AddYears(-25) };
            _donorRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Donor>())).ReturnsAsync(donor);

            var result = await _donorService.RegisterDonorAsync(donor);

            Assert.Equal(donor.Id, result.Id);
            _donorRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Donor>()), Times.Once);
        }

        [Fact]
        public async Task RegisterDonorAsync_InvalidDonor_ThrowsValidationException()
        {
            var donor = new Donor { Id = 2, Name = "", BloodType = BloodType.O_Positive, Email = "invalid", PhoneNumber = "", DateOfBirth = DateTime.Now };
            await Assert.ThrowsAsync<ValidationException>(() => _donorService.RegisterDonorAsync(donor));
        }

        [Fact]
        public async Task GetDonorByIdAsync_CacheMiss_ReturnsDonor()
        {
            var donor = new Donor { Id = 3, Name = "Jane Doe", BloodType = BloodType.A_Negative, Email = "jane@example.com", PhoneNumber = "9876543210", DateOfBirth = DateTime.Now.AddYears(-30) };
            _donorRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(donor);

            var result = await _donorService.GetDonorByIdAsync(3);

            Assert.Equal(donor.Id, result.Id);
            _donorRepositoryMock.Verify(r => r.GetByIdAsync(3), Times.Once);
        }

        [Fact]
        public async Task GetDonorByIdAsync_NotFound_ThrowsKeyNotFoundException()
        {
            _donorRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Donor)null);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _donorService.GetDonorByIdAsync(99));
        }
    }
}
