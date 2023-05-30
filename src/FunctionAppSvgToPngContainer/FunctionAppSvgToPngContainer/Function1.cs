using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Svg;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace FunctionAppSvgToPngContainer
{
    public static class Function1
    {
        // http://localhost:33400/api/Function1
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("SvgToPng")]
        public static async Task<IActionResult> SvgToPngAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("SvgToPng recieved a request.");

            using var svgMemoryStream = new MemoryStream();
            await req.Body.CopyToAsync(svgMemoryStream);
            byte[] svg = svgMemoryStream.ToArray();

            if (svg != null && svg.Length != 0)
            {
                log.LogInformation(svg.Length.ToString());

                int pixelHeight = 200;
                int pixelWidth = 200;
                int viewBoxWidth = 200;
                int viewBoxHeight = 200;
                var svgContent = System.Text.Encoding.UTF8.GetString(svg);
                var mySvg = SvgDocument.FromSvg<SvgDocument>(svgContent);

                mySvg.Height = new SvgUnit(SvgUnitType.Pixel, pixelHeight);
                mySvg.Width = new SvgUnit(SvgUnitType.Pixel, pixelWidth);
                mySvg.ViewBox = new SvgViewBox(0, 0, viewBoxWidth, viewBoxHeight);

                Bitmap bmpSquare = mySvg.Draw(pixelWidth, pixelHeight);

                byte[] image = ImageToByte(bmpSquare);

                return new FileContentResult(image, "image/png");

                //using (var rasterImage = new MagickImage(svg))
                //{
                //    using var pngMemorySream = new MemoryStream();
                //    rasterImage.Write(pngMemorySream);
                //    log.LogInformation("image returned.");
                //    return new FileContentResult(pngMemorySream.ToArray(), "image/png");
                //}
            }
            else
            {
                log.LogInformation("No data parameter provided.");
                return new BadRequestResult();
            }
        }

        public static byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}