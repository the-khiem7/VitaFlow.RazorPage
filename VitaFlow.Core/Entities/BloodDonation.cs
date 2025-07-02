using System;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Entities
{
    /// <summary>
    /// Represents a blood donation.
    /// </summary>
    public class BloodDonation
    {
        public int Id { get; set; }
        public int DonorId { get; set; }
        public Donor Donor { get; set; }
        public BloodType BloodType { get; set; }
        public double Volume { get; set; }
        public DateTime DonationDate { get; set; }
        public DonationStatus Status { get; set; }
        public string Notes { get; set; }
        public int? BloodRequestId { get; set; }
        public BloodRequest BloodRequest { get; set; }
        public int? InventoryId { get; set; }
        public BloodInventory Inventory { get; set; }
    }
}
