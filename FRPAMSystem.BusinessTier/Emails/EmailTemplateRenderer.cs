using System.Net;
using System.Reflection;

namespace FRPAMSystem.BusinessTier.Emails
{
    public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        private const string LayoutResource = "FRPAMSystem.BusinessTier.Emails.Templates.Layout.html";
        private const string BodyResource = "FRPAMSystem.BusinessTier.Emails.Templates.Body.html";

        private readonly string _layout;
        private readonly string _body;

        public EmailTemplateRenderer()
        {
            _layout = LoadResource(LayoutResource);
            _body = LoadResource(BodyResource);
        }

        public string RenderHtml(EmailTemplateModel model)
        {
            var copy = EmailNotificationTemplateCatalog.Get(model.NotificationType);
            var fullName = string.IsNullOrWhiteSpace(model.FullName) ? "User" : model.FullName;

            var referenceLine = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.ReferenceType) && model.ReferenceId.HasValue)
            {
                referenceLine =
                    $"<p style=\"margin:14px 0 0;font-size:12px;color:#6b7c6c;\">Reference: {Html(model.ReferenceType)} #{model.ReferenceId.Value}</p>";
            }

            var inner = _body
                .Replace("{{FullName}}", Html(fullName), StringComparison.Ordinal)
                .Replace("{{Badge}}", Html(copy.Badge), StringComparison.Ordinal)
                .Replace("{{Accent}}", copy.Accent, StringComparison.Ordinal)
                .Replace("{{Title}}", Html(model.Title), StringComparison.Ordinal)
                .Replace("{{Intro}}", Html(copy.Intro), StringComparison.Ordinal)
                .Replace("{{Message}}", Html(model.Message), StringComparison.Ordinal)
                .Replace("{{ReferenceLine}}", referenceLine, StringComparison.Ordinal);

            return _layout
                .Replace("{{Title}}", Html(model.Title), StringComparison.Ordinal)
                .Replace("{{Content}}", inner, StringComparison.Ordinal)
                .Replace("{{Year}}", DateTime.UtcNow.Year.ToString(), StringComparison.Ordinal);
        }

        private static string Html(string? value) =>
            WebUtility.HtmlEncode(value ?? string.Empty);

        private static string LoadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Email template resource '{name}' was not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
