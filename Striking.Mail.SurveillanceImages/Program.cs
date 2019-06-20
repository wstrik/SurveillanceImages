using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Striking.Mail.SurveillanceImages.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;
using Striking.Mail.SurveillanceImages.Properties;

namespace Striking.Mail.SurveillanceImages
{
    class Program
    {
        private static DateTime LastRun
        {
            get { return Settings.Default.LastRun; }
            set
            {
                Settings.Default.LastRun = value;
            }
        }

        static void Main(string[] args)
        {
            var inputPaths = ConfigurationManager.AppSettings["FindInPath"].Split(new[] { ';' }, options: StringSplitOptions.RemoveEmptyEntries);
            foreach (var inputPath in inputPaths)
            {
                var files = Directory.EnumerateFiles(inputPath, "*.mkv", SearchOption.AllDirectories);
                long totalBytes = 0;
                var attachments = new List<Attachment>();
                var html = new StringBuilder();
                var subject = String.Empty;
                foreach (var filePath in files)
                {
                    subject = String.Concat("IPCAM ", inputPath.Split(new[] { '\\' }, options: StringSplitOptions.RemoveEmptyEntries).Last(), " ", Directory.GetCreationTime(filePath).ToString("ddd dd MMM"));

                    if (File.GetCreationTime(filePath) > LastRun)
                    {
                        var bytes = VideoHelper.GrabThumbnail(filePath, int.Parse(ConfigurationManager.AppSettings["TakeThumbAddMiliseconds"]));
                        totalBytes += bytes.Length;

                        attachments.Add(new Attachment(new MemoryStream(bytes), Path.GetFileNameWithoutExtension(filePath) + ".jpg") { ContentId = "attachment" + attachments.Count });

                        using (var m = new MemoryStream())
                            html.AppendFormat("<a href='https://photos.google.com/u/1/search/{0}' target='_blank'><img src='cid:{1}' style='margin:1%; width:48%' /></a>", Path.GetFileNameWithoutExtension(filePath).Replace("_", "__"), attachments.Last().ContentId);
                    }

                    if (totalBytes > 20000000) // meer dan 20mb 
                    {
                        MailHelper.SendMail("wietzestrik@gmail.com", "", ConfigurationManager.AppSettings["EmailAddress"], "", html.ToString(), subject, attachments.ToArray());
                        totalBytes = 0;
                        attachments.Clear();
                        html.Clear();
                    }
                }

                MailHelper.SendMail("wietzestrik@gmail.com", "", ConfigurationManager.AppSettings["EmailAddress"], "", html.ToString(), subject, attachments.ToArray());
                //File.WriteAllText(@"C:\Users\wietz\Desktop\output.html", html.ToString());
            }

            LastRun = DateTime.Today;
        }
    }
}
