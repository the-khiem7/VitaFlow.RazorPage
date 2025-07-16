using System;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Entities
{
    // Represents blood inventory.
    public class BloodInventory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public BloodType BloodType { get; set; }
        public double WholeBloodVolume { get; set; }
        public double RedCellsVolume { get; set; }
        public double PlasmaVolume { get; set; }
        public double PlateletsVolume { get; set; }
        public DateTime CollectionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsAvailable { get; set; }
        public string StorageLocation { get; set; }
        public string BatchNumber { get; set; }
    }
}
