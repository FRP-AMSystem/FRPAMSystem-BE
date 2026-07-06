namespace FRPAMSystem.BusinessTier.Payload.Users
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string? Password { get; set; }

        public string Email { get; set; } = string.Empty;

        public int RoleId { get; set; }
    }
}
