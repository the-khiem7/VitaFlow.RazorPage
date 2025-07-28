namespace Models.DTOs
{
    public class UserUpdateDTO
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? UserIdCard { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
}