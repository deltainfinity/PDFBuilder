using System.IO;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.IO.Source;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
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
        /// <param name="html">HTML string</param>
        /// <returns></returns>
        [HttpPost("CreateFromHtml")]
        public ActionResult<byte[]> CreateFromHtml([FromBody]string html)
        {
            Logger.Information("Create PDF From HTML endpoint called");
            using var memoryStream = new MemoryStream();
            {
                HtmlConverter.ConvertToPdf(html, memoryStream);

                return Ok(memoryStream.ToArray());
            }
        }
    }
}