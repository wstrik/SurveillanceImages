using System.Net.Mail;

namespace Striking.Mail.SurveillanceImages.Helpers
{
    public class MailHelper
    {
        public static void SendMail(string fromEmail, string fromName, string toEmail, string toName, string content, string subject, Attachment[] attachments = null)
        {
            SendMail(new MailAddress(fromEmail, fromName), new MailAddress(toEmail, toName), content, subject, attachments );
        }

        public static void SendMail(string fromEmail, string fromName, string toEmail, string toName, string replyToEmail, string replyToName, string content, string subject, Attachment[] attachments = null)
        {
            SendMail(new MailAddress(fromEmail, fromName), new MailAddress(toEmail, toName), new MailAddress(replyToEmail, replyToName), content, subject, attachments);
        }

        public static void SendMail(MailAddress fromAddress, MailAddress toAddress, string content, string subject, Attachment[] attachments = null)
        {
            SendMail(fromAddress, toAddress, fromAddress, content, subject, attachments);
        }

        public static void SendMail(MailAddress fromAddress, MailAddress toAddress, MailAddress replyToAddress, string content, string subject, Attachment[] attachments = null)
        {
            var mail = new MailMessage(fromAddress, toAddress);
            mail.ReplyToList.Add(replyToAddress);
            //mail.Bcc.Add(fromAddress);

            foreach (var att in attachments)
                mail.Attachments.Add(att);

            if (!string.IsNullOrEmpty(mail.Headers.Get("Return-Path")))
                mail.Headers["Return-Path"] = fromAddress.Address;
            else
                mail.Headers.Add("Return-Path", fromAddress.Address);

            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = content;

            using (var client = new SmtpClient())
                client.Send(mail);
        }
    }
}
