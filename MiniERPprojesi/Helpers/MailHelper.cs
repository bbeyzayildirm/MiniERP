using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Hosting;

public static class MailHelper
{
    public static void Send(string to, string subject, string body, bool isHtml = false)
    {
        // TLS güvenliği
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

        string host = GetSetting("SmtpHost") ?? "smtp.gmail.com";
        int port = int.TryParse(GetSetting("SmtpPort"), out var p) ? p : 587;
        bool enableSsl = bool.TryParse(GetSetting("SmtpEnableSsl"), out var ssl) ? ssl : true;

        // Kimlik bilgileri: ENV -> local -> Web.config
        string user = Environment.GetEnvironmentVariable("SMTP_USER")
                      ?? GetLocalSetting("SmtpUser")
                      ?? GetSetting("SmtpUser");

        string pass = Environment.GetEnvironmentVariable("SMTP_PASS")
                      ?? GetLocalSetting("SmtpPass")
                      ?? GetSetting("SmtpPass");

        // From boşsa Gmail için user'ı kullan
        string from = GetSetting("MailFrom");
        if (string.IsNullOrWhiteSpace(from)) from = user;

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            throw new InvalidOperationException("SMTP kullanıcı adı/şifresi bulunamadı (SmtpUser/SmtpPass).");

        // Alıcı/adres doğrulaması (erken hata için)
        var fromAddr = new MailAddress(from);
        var toAddr = new MailAddress(to);

        using (var mail = new MailMessage(fromAddr, toAddr))
        {
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = isHtml;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.EnableSsl = enableSsl;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(user, pass);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(mail);
            }
        }
    }

    private static string GetSetting(string key) =>
        ConfigurationManager.AppSettings[key];

    private static string GetLocalSetting(string key)
    {
        try
        {
            var localPath = HostingEnvironment.MapPath("~/web.config.local");
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath)) return null;

            var map = new ExeConfigurationFileMap { ExeConfigFilename = localPath };
            var cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            return cfg.AppSettings.Settings[key]?.Value;
        }
        catch { return null; }
    }
}
