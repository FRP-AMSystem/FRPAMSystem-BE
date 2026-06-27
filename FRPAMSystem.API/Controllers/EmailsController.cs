using System.ComponentModel.DataAnnotations;
using FRPAMSystem.BusinessTier.Payload.Email;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Gửi email thử SMTP. Email người nhận không cần tồn tại trong database.
        /// </summary>
        [HttpPost("test")]
        [AllowAnonymous]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Request body is required." });
            }

            try
            {
                await _emailService.SendTestAsync(request);

                return Ok(new
                {
                    success = true,
                    message = $"Test email sent to {request.ToEmail}",
                    data = new
                    {
                        request.ToEmail,
                        subject = "FRPAM System - Test Email"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
