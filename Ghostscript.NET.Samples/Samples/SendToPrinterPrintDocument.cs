using Ghostscript.NET.Rasterizer;
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace Ghostscript.NET.Samples.Samples
{
    public class SendToPrinterPrintDocument : ISample
    {
        private int _currentPage = 1;
        public void Start()
        {
            string printerName = "Microsoft Print to PDF";
            string inputFile = @"C:\workspace\idpeople\DevOps\Residence Persmit\Docs\Carrier\Merged\STD-SI-20250313-22413_20250313121350.pdf";
            try
            {
                // Check if the printer is valid
                PrinterSettings printerSettings = new PrinterSettings
                {
                    PrinterName = printerName,
                    Copies = 1,

                };
                printerSettings.DefaultPageSettings.PaperSize.RawKind = (int)PaperKind.A4;
                printerSettings.DefaultPageSettings.Margins.Top = 0;
                printerSettings.DefaultPageSettings.Margins.Left = 0;
                printerSettings.DefaultPageSettings.Margins.Right = 0;
                printerSettings.DefaultPageSettings.Margins.Bottom = 0;

                if (!printerSettings.IsValid)
                {
                    throw new Exception("The specified printer is not valid.");
                }

                // Use Ghostscript to print the PDF
                using (var ghostscriptRasterizer = new GhostscriptRasterizer())
                {
                    ghostscriptRasterizer.Open(inputFile);
                    _currentPage = 1;
                    using (var printDocument = new PrintDocument())
                    {
                        printDocument.PrinterSettings = printerSettings;
                        printDocument.PrintPage += (sender, e) =>
                        {
                            // Render each page of the PDF
                            var pdfPage = ghostscriptRasterizer.GetPage(300, _currentPage);
                            RectangleF printableArea = e.MarginBounds;
                            RectangleF destinationRect = new RectangleF(
                                printableArea.Left,
                                printableArea.Top,
                                printableArea.Width,
                                printableArea.Height
                            );

                            // Draw the PDF page stretched to fit the printable area
                            e.Graphics?.DrawImage(pdfPage, destinationRect);
                            //e.Graphics.DrawImage(page, e.MarginBounds);
                            e.HasMorePages = ghostscriptRasterizer.PageCount > _currentPage;
                            _currentPage++;
                        };

                        printDocument.EndPrint += (sender, e) =>
                        {
                            if (e.PrintAction == PrintAction.PrintToPrinter)
                            {
                                Console.WriteLine("Printing completed successfully.");
                            }
                            else
                            {
                                throw new Exception("An error occurred while printing.");
                            }
                        };

                        printDocument.Print();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
