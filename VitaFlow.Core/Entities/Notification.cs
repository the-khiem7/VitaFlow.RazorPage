using System;

namespace VitaFlow.Core.Entities
{
    /// <summary>
    /// Represents a notification for a user.
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; }
    }
}
