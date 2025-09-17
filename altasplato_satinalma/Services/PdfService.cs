using DinkToPdf.Contracts;
using DinkToPdf;

namespace altasplato_satinalma.Services
{
    public class PdfService
    {
        private IConverter _converter;

        public PdfService()
        {
            // IConverter nesnesini başlat
            _converter = new SynchronizedConverter(new PdfTools());
        }

        public void ConvertHtmlToPdf(string htmlContent, string outputPdfPath)
        {
            var document = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Out = outputPdfPath
            },
                Objects = {
                new ObjectSettings() {
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
            };

            // HTML'yi PDF'ye dönüştür
            _converter.Convert(document);
        }
    }
}
