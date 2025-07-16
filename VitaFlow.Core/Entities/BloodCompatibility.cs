using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Entities
{
    // Represents blood compatibility information.
    public class BloodCompatibility
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public BloodType DonorBloodType { get; set; }
        public BloodType RecipientBloodType { get; set; }
        public bool IsCompatible { get; set; }
    }
}
