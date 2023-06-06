using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Svg;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace FunctionAppSvgToPngContainer
{
    public static class Function1
    {
        // use post man with
        // URL http://localhost:33400/api/SvgToPng
        // Verb: Post
        // body
        //        <svg version = "1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"
        //  xmlns:xml="http://www.w3.org/XML/1998/namespace" viewBox="0, 0, 240, 80">
        //  <style>
        //    .small {
        //    font: italic 13px Liberation Serif;
        //    }
        //    .heavy {
        //    font: bold 30px Segoe Pro Bold;
        //}

        //    /* Note that the color of the text is set with the *
        //    * fill property, the color property is for HTML only */
        //    .Rrrrr {
        //    font: italic 40px Segoe Pro;
        //fill: red;
        //    }
        //  </ style >
        //  < text x = "20" y = "35" font - size = "13px" font - style = "italic" class= "small" > My </ text >
        //  < text x = "40" y = "35" font - size = "30px" font - weight = "bold" class= "heavy" > cat </ text >
        //  < text x = "55" y = "55" font - size = "13px" font - style = "italic" class= "small" >is </ text >
        //  < text x = "65" y = "55" font - size = "40px" font - style = "italic" class= "Rrrrr" style = "fill:red;" > Grumpy! </ text >
        //</ svg >

        [FunctionName("SvgToPng")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("SvgToPng received a request.");

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