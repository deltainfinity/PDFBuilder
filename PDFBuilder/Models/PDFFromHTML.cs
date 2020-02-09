namespace PDFBuilder.Models
{
    /// <summary>
    /// Input information to create a PDF using the supplied HTML string and
    /// optional header and footer information.
    /// </summary>
    public class PDFFromHTML
    {
        /// <summary>
        /// Header 
        /// </summary>
        public Header PDFHeader { get; set; }

        /// <summary>
        /// HTML to convert to a PDF
        /// </summary>
        public string HTML { get; set; }

        /// <summary>
        /// Footer
        /// </summary>
        public Footer PDFFooter { get; set; }
    }
}
