using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Services.Services
{
        // Implementation of the IBloodCompatibilityService interface.
    public class BloodCompatibilityService : IBloodCompatibilityService
    {
        private readonly ILogger<BloodCompatibilityService> _logger;
        private static readonly Dictionary<BloodType, List<BloodType>> _compatibilityMap;

        /// <summary>
        /// Static constructor to initialize the compatibility map
        /// </summary>
        static BloodCompatibilityService()
        {
            // Initialize compatibility map (donor -> compatible recipients)
            _compatibilityMap = new Dictionary<BloodType, List<BloodType>>
            {
                // O- (universal donor) can donate to all blood types
                { BloodType.ONegative, new List<BloodType> { 
                    BloodType.ONegative, BloodType.OPositive, 
                    BloodType.ANegative, BloodType.APositive, 
                    BloodType.BNegative, BloodType.BPositive, 
                    BloodType.ABNegative, BloodType.ABPositive 
                }},
                // O+ can donate to O+, A+, B+, AB+
                { BloodType.OPositive, new List<BloodType> { 
                    BloodType.OPositive, BloodType.APositive, 
                    BloodType.BPositive, BloodType.ABPositive 
                }},
                // A- can donate to A-, A+, AB-, AB+
                { BloodType.ANegative, new List<BloodType> { 
                    BloodType.ANegative, BloodType.APositive, 
                    BloodType.ABNegative, BloodType.ABPositive 
                }},
                // A+ can donate to A+, AB+
                { BloodType.APositive, new List<BloodType> { 
                    BloodType.APositive, BloodType.ABPositive 
                }},
                // B- can donate to B-, B+, AB-, AB+
                { BloodType.BNegative, new List<BloodType> { 
                    BloodType.BNegative, BloodType.BPositive, 
                    BloodType.ABNegative, BloodType.ABPositive 
                }},
                // B+ can donate to B+, AB+
                { BloodType.BPositive, new List<BloodType> { 
                    BloodType.BPositive, BloodType.ABPositive 
                }},
                // AB- can donate to AB-, AB+
                { BloodType.ABNegative, new List<BloodType> { 
                    BloodType.ABNegative, BloodType.ABPositive 
                }},
                // AB+ (universal recipient) can only donate to AB+
                { BloodType.ABPositive, new List<BloodType> { 
                    BloodType.ABPositive 
                }}
            };
        }

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public BloodCompatibilityService(ILogger<BloodCompatibilityService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Determines if the donor blood type is compatible with the recipient blood type.
        /// </summary>
        public Task<bool> IsCompatibleAsync(BloodType donorType, BloodType recipientType)
        {
            try
            {
                _logger.LogInformation("Checking compatibility between donor type {DonorType} and recipient type {RecipientType}", donorType, recipientType);
                var isCompatible = _compatibilityMap[donorType].Contains(recipientType);
                return Task.FromResult(isCompatible);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining blood compatibility between {DonorType} and {RecipientType}", donorType, recipientType);
                throw;
            }
        }

        /// <summary>
        /// Gets the compatible donor blood types for a recipient blood type.
        /// </summary>
        /// <param name="recipientType">The blood type of the recipient</param>
        /// <returns>List of compatible donor blood types</returns>
        public Task<IEnumerable<BloodType>> GetCompatibleDonorTypesAsync(BloodType recipientType)
        {
            try
            {
                _logger.LogInformation("Finding compatible donor types for recipient type {RecipientType}", recipientType);
                
                // Find all blood types that can donate to this recipient
                var compatibleDonors = new List<BloodType>();
                foreach (var donorType in Enum.GetValues<BloodType>())
                {
                    if (_compatibilityMap[donorType].Contains(recipientType))
                    {
                        compatibleDonors.Add(donorType);
                    }
                }
                
                return Task.FromResult<IEnumerable<BloodType>>(compatibleDonors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining compatible donor types for {RecipientType}", recipientType);
                throw;
            }
        }

        /// <summary>
        /// Gets the compatible recipient blood types for a donor blood type.
        /// </summary>
        /// <param name="donorType">The blood type of the donor</param>
        /// <returns>List of compatible recipient blood types</returns>
        public Task<IEnumerable<BloodType>> GetCompatibleRecipientTypesAsync(BloodType donorType)
        {
            try
            {
                _logger.LogInformation("Finding compatible recipient types for donor type {DonorType}", donorType);
                return Task.FromResult<IEnumerable<BloodType>>(_compatibilityMap[donorType]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining compatible recipient types for {DonorType}", donorType);
                throw;
            }
        }
    }
}
