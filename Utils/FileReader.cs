using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;
using Xceed.Words.NET;

namespace JeffPires.BacklogChatGPTAssistant.Utils
{
    /// <summary>
    /// Provides methods for reading files from the filesystem.
    /// </summary>
    public static class FileReader
    {
        /// <summary>
        /// Retrieves the text content from a DOCX file specified by the file path.
        /// </summary>
        /// <param name="filePath">The path to the DOCX file from which to extract text.</param>
        /// <returns>
        /// A string containing the text extracted from the DOCX file.
        /// </returns>
        public static string GetTextFromDocx(string filePath)
        {
            using (DocX document = DocX.Load(filePath))
            {
                return document.Text;
            }
        }

        /// <summary>
        /// Extracts text from a PDF file located at the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the PDF file from which to extract text.</param>
        /// <returns>A string containing the extracted text from the PDF.</returns>
        public static string GetTextFromPDF(string filePath)
        {
            StringBuilder text = new();

            using (PdfReader reader = new(filePath))
            {
                PdfDocument pdfDoc = new(reader);

                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
                }
            }

            return text.ToString();
        }
    }
}