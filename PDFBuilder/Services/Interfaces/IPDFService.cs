namespace PDFBuilder.Services.Interfaces
{
    /// <summary>
    /// Service for creating PDF documents using iText
    /// </summary>
    public interface IPDFService
    {
        /// <summary>
        /// Generate a PDF from an HTML string with optional header and footer values
        /// </summary>
        /// <param name="html">HTML and header/footer information</param>
        /// <returns><seealso cref="byte">byte</seealso> array that contains the rendered PDF document</returns>
        byte[] GeneratePDFFromHTML(string html);
    }
}
