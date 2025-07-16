using System;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Entities
{
    // Represents a blood donation.
    public class BloodDonation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DonorId { get; set; }
        public Donor Donor { get; set; }
        public BloodType BloodType { get; set; }
        public double Volume { get; set; }
        public DateTime DonationDate { get; set; }
        public DonationStatus Status { get; set; }
        public string Notes { get; set; }
        public Guid BloodRequestId { get; set; }
        public BloodRequest BloodRequest { get; set; }
        public int? InventoryId { get; set; }
        public BloodInventory Inventory { get; set; }
    }
}
