using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Entities
{
    /// <summary>
    /// Represents blood compatibility information.
    /// </summary>
    public class BloodCompatibility
    {
        public int Id { get; set; }
        public BloodType DonorBloodType { get; set; }
        public BloodType RecipientBloodType { get; set; }
        public bool IsCompatible { get; set; }
    }
}
