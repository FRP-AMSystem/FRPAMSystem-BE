namespace FRPAMSystem.BusinessTier.Payload.Auth
{
    public class PasswordVerifyRequest
    {
        public string Password { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;
    }
}
