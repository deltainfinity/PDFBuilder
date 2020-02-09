using System.IO;
using iText.Html2pdf;
using iText.StyledXmlParser.Css.Media;
using Microsoft.AspNetCore.Mvc;
using PDFBuilder.Models;
using Serilog;

namespace PDFBuilder.Controllers
{
    /// <summary>
    /// PDF Controller for creating PDF documents
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class PDFController : ControllerBase
    {
        private static readonly ILogger Logger = Log.ForContext<PDFController>();

        /// <summary>
        /// Create a PDF from HTML string
        /// </summary>
        /// <param name="data">HTML to PDF information</param>
        /// <returns>Byte array containing the converted HTML as a PDF</returns>
        [HttpPost("CreateFromHtml")]
        public ActionResult<byte[]> CreateFromHtml([FromBody]PDFFromHTML data)
        {
            Logger.Information("Create PDF From HTML endpoint called");
            var properties = new ConverterProperties()
                .SetMediaDeviceDescription(new MediaDeviceDescription(MediaType.PRINT));

            using var memoryStream = new MemoryStream();
            {
                HtmlConverter.ConvertToPdf(data.HTML, memoryStream, properties);

                return Ok(memoryStream.ToArray());
            }
        }
    }
}