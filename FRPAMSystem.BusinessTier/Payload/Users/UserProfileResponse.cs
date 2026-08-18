namespace FRPAMSystem.BusinessTier.Payload.Users
{
    public class UserProfileResponse
    {
        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? RoleName { get; set; }
    }
}
