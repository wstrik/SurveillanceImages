using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Striking.Mail.SurveillanceImages.Helpers
{
    public static class VideoHelper
    {
        public static void GrabThumbnail(string videoFilename, string outputFilename, long atMiliseconds = 0, int? width = null, int? height = null)
        {
            var inputFile = new MediaFile(videoFilename);
            var outputFile = new MediaFile { Filename = outputFilename };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
                if (inputFile.Metadata.Duration.Milliseconds < atMiliseconds)
                    atMiliseconds = inputFile.Metadata.Duration.Milliseconds;
                // Saves the frame located on the 15th second of the video.
                var options = new ConversionOptions { Seek = TimeSpan.FromMilliseconds(atMiliseconds), CustomWidth = width, CustomHeight = height };
                engine.GetThumbnail(inputFile, outputFile, options);

                // resize
                if (width.HasValue || height.HasValue)
                {
                    int orgWidth, orgHeight;
                    GetVideoSize(videoFilename, out orgWidth, out orgHeight);
                    if (width.HasValue && !height.HasValue)
                        height = (int)(width.Value / (decimal)orgWidth * orgHeight);
                    if (!width.HasValue && height.HasValue)
                        width = (int)(height.Value / (decimal)orgHeight * orgWidth);
                    var img = Image.FromFile(outputFilename);
                    var thumb = img.GetThumbnailImage(width.Value, height.Value, null, IntPtr.Zero);
                    img.Dispose();
                    thumb.Save(outputFilename);
                    thumb.Dispose();
                }

            }
        }

        public static byte[] GrabThumbnail(string videoFilename, long atMiliseconds = 0, int? width = null, int? height = null)
        {
            var outputFilename = Path.GetTempFileName().Replace(".tmp", ".jpg");
            GrabThumbnail(videoFilename, outputFilename, atMiliseconds, width, height);

            var result = new byte[] { };
            if (File.Exists(outputFilename))
            {
                result = File.ReadAllBytes(outputFilename);
                File.Delete(outputFilename);
            }
            return result;
        }

        public static void GetVideoSize(string videoFilename, out int width, out int height)
        {
            var inputFile = new MediaFile { Filename = videoFilename };

            using (var engine = new Engine())
                engine.GetMetadata(inputFile);
            width = int.Parse(inputFile.Metadata.VideoData.FrameSize.Split('x')[0]);
            height = int.Parse(inputFile.Metadata.VideoData.FrameSize.Split('x')[1]);
        }
    }
}
