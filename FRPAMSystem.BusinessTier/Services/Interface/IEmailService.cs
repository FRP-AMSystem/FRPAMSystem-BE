using FRPAMSystem.BusinessTier.Payload.Email;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEmailService
    {
        Task SendAsync(SendEmailRequest request);
    }
}
