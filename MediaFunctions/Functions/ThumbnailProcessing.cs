using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.ProjectOxford.Face;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Functions
{
    public class ThumbnailProcessItem
    {
        public string id { get; set; }

    }
    public static class ThumbnailProcessing
    {
        public static int RetryCountOnQuotaLimitError = 6;
        public static int RetryDelayOnQuotaLimitError = 500;

        //GENERATE 320 and 160 THUMBNAIL
        [FunctionName("ThumbnailProcessing")]
        [return:Queue("thumbnails160")]
        public static ThumbnailProcessItem Thumbnail(
            [QueueTrigger("thumbnails", Connection = "Storage")] ThumbnailProcessItem myQueueItem,
            [Blob("thumbs1000/{id}", FileAccess.Read)] Stream image, 
            [Blob("thumbs320/{id}", FileAccess.Write)] Stream image320,
            ILogger log)
        {
            string name = myQueueItem.id;

            string visionKey = Environment.GetEnvironmentVariable("visionKey");

            //CREATE THUMBNAIL 320
            log.LogInformation($"Blob Name:{name} Size: {image.Length} Bytes");
            ComputerVisionClient computerVision = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(visionKey),
                new System.Net.Http.DelegatingHandler[] { });
            // Specify the Azure region
            computerVision.Endpoint = "https://northeurope.api.cognitive.microsoft.com/";

            Stream newThumb;

            int retriesLeft = ThumbnailProcessing.RetryCountOnQuotaLimitError;
            int delay = ThumbnailProcessing.RetryDelayOnQuotaLimitError;

            while (true)
            {
                try
                {
                    Task<Stream> tnewThumb = GenThumb(image, 320, 320, computerVision);
                    tnewThumb.Wait();
                    newThumb = tnewThumb.Result;
                    newThumb.CopyTo(image320);
                    break;
                }
                catch (FaceAPIException exception) when (exception.HttpStatus == (System.Net.HttpStatusCode)429 && retriesLeft > 0)
                {
                    log.LogTrace(exception, "Custom Vision throttling error");

                    Task T = Task.Delay(delay);
                    T.Wait();
                    //await Task.Delay(delay);

                    retriesLeft--;
                    delay *= 2;
                    continue;
                }
                catch (Exception e)
                {
                    log.LogInformation("could not resize:" + name + ", error:" + e.Message);
                    throw;
                }
            }

            try
            {
                Stream newThumb2 = new MemoryStream();
                newThumb.CopyTo(newThumb2);

                return new ThumbnailProcessItem() { id = name };
            }
            catch (Exception e)
            {
                log.LogInformation("could not resize:" + name + ", error:" + e.Message);
                throw;
            }
        }
        private static async Task<Stream> GenThumb(Stream image, int thumbnailWidth, int thumbnailHeight, ComputerVisionClient computerVision)
        {
            return await computerVision.GenerateThumbnailInStreamAsync(thumbnailWidth, thumbnailHeight, image, true);
        }

        [FunctionName("ThumbnailProcessing160")]
        public static void Thumbnail160(
    [QueueTrigger("thumbnails160", Connection = "Storage")] ThumbnailProcessItem myQueueItem,
    [Blob("thumbs320/{id}", FileAccess.Read)] Stream image320,
    [Blob("thumbs160/{id}", FileAccess.Write)] Stream image160,
    ILogger log)
        {
            string name = myQueueItem.id;

            //CREATE THUMBNAIL 160
            log.LogInformation($"Blob Name:{name} Size: {image320.Length} Bytes");
            try
            {
                Stream newThumb = resizeImage(image320, 160, 160);
                newThumb.CopyTo(image160);
            }
            catch (Exception e)
            {
                log.LogInformation("could not resize:" + name + ", error:" + e.Message);
                throw;
            }
        }

        private static Stream resizeImage(Stream img, int width, int height)
        {
            var image = Image.Load(img);
            Stream result = new MemoryStream();
            image.Mutate(x => x
                .Resize(width, height));
			image.SaveAsJpeg(result);
    		result.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}
