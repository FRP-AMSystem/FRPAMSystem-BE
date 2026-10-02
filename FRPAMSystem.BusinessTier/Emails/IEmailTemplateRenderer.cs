namespace FRPAMSystem.BusinessTier.Emails
{
    public interface IEmailTemplateRenderer
    {
        string RenderHtml(EmailTemplateModel model);
    }
}
