using ImageMagick;
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

namespace FunctionAppSvgToPng
{
    public static class Function1
    {
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

            int pixelHeight = 200;
            int pixelWidth = 200;
            int viewBoxWidth = 0;
            int viewBoxHeight = 0;
            var svgContent = File.ReadAllText("C:\\Users\\Stefan\\Desktop\\SvgAzureFunctionPoc\\svg\\Example.svg");
            var mySvg = SvgDocument.FromSvg<SvgDocument>(svgContent);

            mySvg.Height = new SvgUnit(SvgUnitType.Pixel, pixelHeight);
            mySvg.Width = new SvgUnit(SvgUnitType.Pixel, pixelWidth);
            mySvg.ViewBox = new SvgViewBox(0, 0, viewBoxWidth, viewBoxHeight);

            Bitmap bmpSquare = mySvg.Draw(pixelWidth, pixelHeight);
            bmpSquare.Save("C:\\Users\\Stefan\\Desktop\\SvgAzureFunctionPoc\\svg\\Example.png");

            var readSettings = new MagickReadSettings()
            {
                Font = "C:\\Users\\Stefan\\Desktop\\SvgAzureFunctionPoc\\svg\\segoe-pro-cufonfonts-webfont\\SegoePro-Bold.woff"
            };

            using (var image = new MagickImage("C:\\Users\\Stefan\\Desktop\\SvgAzureFunctionPoc\\svg\\Example.svg", readSettings))
            {
                image.Write("C:\\Users\\Stefan\\Desktop\\SvgAzureFunctionPoc\\svg\\MagickImage.png");
            }

            return new OkObjectResult(responseMessage);
        }
    }
}