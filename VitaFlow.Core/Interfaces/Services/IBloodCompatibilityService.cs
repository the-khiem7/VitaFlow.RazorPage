using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for blood compatibility operations.
    /// </summary>
    public interface IBloodCompatibilityService
    {
        Task<bool> IsCompatibleAsync(BloodType donorType, BloodType recipientType);
        Task<IEnumerable<BloodType>> GetCompatibleDonorTypesAsync(BloodType recipientType);
        Task<IEnumerable<BloodType>> GetCompatibleRecipientTypesAsync(BloodType donorType);
    }
}
