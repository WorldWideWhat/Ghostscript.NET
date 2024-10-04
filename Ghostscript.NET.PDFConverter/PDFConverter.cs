using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using Microsoft.Extensions.FileProviders.Embedded;
using Microsoft.Extensions.FileProviders;

using Ghostscript.NET.FacturX.ZUGFeRD;
/// <summary>

/// ''' Klasse um per Ghostscript.net/Ghostscript beliebige PDFs in PDF/A-3 Dateien zu konvertieren

/// ''' 

/// ''' </summary>
namespace Ghostscript.NET.PDFConverter
{
    public class PDFConverter
    {
        private string? file_GSDLL_DLL = null;
        private readonly string? file_AdobeRGB1998_ICC = null;
        private readonly string? file_BIGSCRIPT_PS = null;

        private readonly string resourceDir = Path.GetTempPath();
        private GhostscriptVersionInfo? gsVersion = null;

        protected string mPDFInFile = ""; // 08.06.20
        protected string mPDFOutFile = "";
        protected string? mXMLOutFile = null;
        protected string FXVersion = "1.0";
        protected Profile usedProfile = Profiles.getByName("EN16931");


        /// <summary>
        ///     ''' Konstruktor mit Angabe Datenbankdatei, intern-nummer, KlassenID (und Gruppenpositionen) wenn ZUGFeRD geschrieben werden soll
        ///     ''' </summary>
        ///     ''' <param name="pPDFInFile">PDF-Eingabedatei</param>
        ///     ''' <param name="pPDFOutFile">PDF-A/3 Ausgabedatei</param>
        public PDFConverter(string pPDFInFile, string pPDFOutFile)
        {
            file_AdobeRGB1998_ICC = this.resourceDir + "\\AdobeRGB1998.icc";
            file_BIGSCRIPT_PS = this.resourceDir + "\\pdfconvert.ps";

            mPDFInFile = pPDFInFile;
            mPDFOutFile = pPDFOutFile;
        }
        public PDFConverter setFXVersion(string pZFVersion) {
            FXVersion = pZFVersion;

            return this;
        }
        public PDFConverter setFXProfile(Profile p) {
            usedProfile = p;

            return this;
        }


        /// <summary>
        ///     ''' Erlaubt die Angabe einer einzubettenden XML-Datei. Die Ausgabe wird dann inkl. RDF-Metadaten und PDF/A-Schema Extension zur ZUGFeRD-Datei in der in 
        ///     ''' pZFVersion angegebenen Version (2p0 fr 2.0, 2.0.1 sowie 2.1, 2.1.1)
        ///     ''' </summary>
        ///     ''' <param name="pXMLOutFile"></param>
        ///     ''' <param name="pZFVersion"></param>
        public void EmbedXMLForZF(string pXMLOutFile, string pZFVersion)
        {
            mXMLOutFile = pXMLOutFile;
        }



        public void prepareICC()
        {
            string tempfilename = Path.GetTempPath() + "AdobeRGB1998.icc";
            StoreEmbeddedResourceLocally("assets\\AdobeCompat-v2.icc", tempfilename);
        } // !prepareICC()


        private byte[] LoadEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    return null;
                }                    

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        } // !LoadEmbeddedResource()


        private bool StoreEmbeddedResourceLocally(string resourcePath, string localPath)
        {
            byte[] data = LoadEmbeddedResource(resourcePath);
            if (data == null)
            {
                return false;
            }

            System.IO.File.WriteAllBytes(localPath, data);
            return true;
        } // !StoreEmbeddedResourceLocally()


        /// <summary>
        ///     ''' Schreibt eine Postscript-, genauer gesagt PDFMark-Datei die von Ghostscript interpretiert werden kann und mindestens in eine PDF-A/3 umwandelt
        ///     ''' 
        ///     ''' </summary>
        ///     ''' <param name="pEmbeddedAdobeRGB1998ICCFile">ICC-Farbprofildatei bspw von https://www.adobe.com/support/downloads/iccprofiles/iccprofiles_win.html </param>
        ///     '''
        protected void WritePDFMark(string pEmbeddedAdobeRGB1998ICCFile)
        {
            string EscapedEmbeddedXMLFile = "";

            if (mXMLOutFile != null)
            {
                if (!File.Exists(mXMLOutFile))
                {
                    throw new Exception("Datei " + mXMLOutFile + " existiert nicht");
                }
                EscapedEmbeddedXMLFile = mXMLOutFile.Replace(@"\", @"\\"); // in PDFMark werden \ zu \\ gequoted
            }



            if (!File.Exists(pEmbeddedAdobeRGB1998ICCFile))
            {
                throw new Exception("Datei " + file_AdobeRGB1998_ICC + " existiert nicht");
            }
            /*        if (!pEmbeddedAdobeRGB1998ICCFile.Contains(Directory.GetCurrentDirectory()))
                    {
                        throw new Exception("Datei " + file_AdobeRGB1998_ICC + " muss unterhalb des absolut angegebenen Applikationspfades " + Directory.GetCurrentDirectory() + " liegen.");
                    }
                    */
            string EscapedEmbeddedICCFile = file_AdobeRGB1998_ICC.Replace(Directory.GetCurrentDirectory(), @".\").Replace(@"\", @"\\");

            if (mPDFInFile == mPDFOutFile)
            {
                throw new Exception("Eingabedatei darf nicht Ausgabedatei sein");
            }

            string PDFmark = System.Text.Encoding.Default.GetString(LoadEmbeddedResource("Ghostscript.NET.PDFConverter.assets.pdfMarkA3.template"));
            PDFmark = PDFmark.Replace("{{EscapedEmbeddedICCFile}}", EscapedEmbeddedICCFile);

            if (mXMLOutFile != null)
            {
                // Dim myFile As New FileInfo(mXMLOutFile)
                // Dim sizeInBytes As Long = myFile.Length
                FileInfo fi = new FileInfo(mXMLOutFile);
                long sizeInBytes = fi.Length;

                string rdfFXProfile = "EN 16931";

                string PDFmarkZUGFeRD = System.Text.Encoding.Default.GetString(LoadEmbeddedResource("Ghostscript.NET.PDFConverter.assets.pdfMarkZUGFeRD.template"));
                PDFmarkZUGFeRD = PDFmarkZUGFeRD.Replace("{{Date}}", DateTime.Now.ToString("yyyyMMddHHmmssK").Replace(":", "'"));
                PDFmarkZUGFeRD = PDFmarkZUGFeRD.Replace("{{EscapedEmbeddedXMLFile}}", EscapedEmbeddedXMLFile);
                PDFmarkZUGFeRD = PDFmarkZUGFeRD.Replace("{{SizeInBytes}}", sizeInBytes.ToString());
                PDFmarkZUGFeRD = PDFmarkZUGFeRD.Replace("{{FXVersion}}", FXVersion);
                PDFmarkZUGFeRD = PDFmarkZUGFeRD.Replace("{{FXComformanceLevel}}", usedProfile.getXMPName());
            
                PDFmark += PDFmarkZUGFeRD;
            }



            UTF8Encoding utf8 = new UTF8Encoding(false); // do not use BOM s. https://docs.microsoft.com/de-de/dotnet/api/system.text.utf8encoding?view=netcore-3.1
            File.WriteAllBytes(file_BIGSCRIPT_PS, utf8.GetBytes(PDFmark));

        }

        // <summary>
        // Prft ob die Ausgabedatei beschrieben werden kann
        // </summary>
        public bool IsFileLocked(FileInfo file)
        {
            FileStream stream = (FileStream)null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException generatedExceptionName)
            {
                // handle the exception your way
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }


        /// <summary>
        ///     ''' Wandelt eine PDF-Datei (Dateiname in inputfile) in eine PDF A/3 (Outputfilename) um und hngt die aktuelle E-rechnung an 
        ///     ''' </summary>
        ///     ''' <returns>True wenn die Konvertierung funktioniert hat</returns>
        public bool ConvertToPDFA3(String gsdll)
        {

            file_GSDLL_DLL = gsdll;
            gsVersion = new GhostscriptVersionInfo(file_GSDLL_DLL);

            this.prepareICC();

            // based on https://github.com/jhabjan/Ghostscript.NET/blob/master/Ghostscript.NET.Samples/Samples/ProcessorSample1.cs
            if (!File.Exists(file_GSDLL_DLL))
            {
                throw new Exception("Ghostscript DLL " + file_GSDLL_DLL + " bitte ins Verzeichnis " + Directory.GetCurrentDirectory() + " kopieren");

            }

            if (!File.Exists(file_AdobeRGB1998_ICC))
            {
                throw new Exception("Farbpprofile " + file_AdobeRGB1998_ICC + " bitte ins Verzeichnis " + Directory.GetCurrentDirectory() + " kopieren");

            }
            if (!File.Exists(mPDFInFile))
            {
                throw new Exception("Eingabedatei " + mPDFInFile + " existiert nicht");

            }
            if (File.Exists(mPDFOutFile))
            {
                if (FileSystem.GetAttr(mPDFOutFile) == Constants.vbReadOnly)
                {
                    throw new Exception("Ausgabedatei " + mPDFOutFile + " keine Schreibrechte");


                }
                FileInfo fi = new FileInfo(mPDFOutFile);
                if (IsFileLocked(fi))
                {
                    throw new Exception("Ausgabedatei " + mPDFOutFile + " nicht beschreibbar (noch geffnet?)");

                }
            }

            // Info: braucht folgende DLLs:
            // BouncyCastle.Crypto.dll, Common.Logging.Core.dll, Common.Logging.dll
            // und: etliche, weitere der iText*.dll -> vorsichtshalber alle 9 kopiert
            /*            PdfDocument doc = new PdfDocument(new PdfReader(mPDFInFile));
                        PdfAConformanceLevel comLev = doc.GetReader().GetPdfAConformanceLevel();
                        if (comLev != null)
                        {
                        }

            */



            WritePDFMark(file_AdobeRGB1998_ICC);

            // MsgBox(gsVersion.ToString)
            GhostscriptLibrary gsL = null/* TODO Change to default(_) if this is not a reference type */;
            // Dim gsL As GhostscriptLibrary = New GhostscriptLibrary(File.ReadAllBytes(ClsCommon.JS.StartupDir & ClsEDocs_Base.FixedValues.FILENAME_ZF_GSDLL32))
            gsL = new GhostscriptLibrary(gsVersion);
            GhostscriptPipedOutput gsPipedOutput = new GhostscriptPipedOutput();

            List<string> switches = new List<string>();
            // works : "C:\Program Files (x86)\gs\gs9.52\bin\gswin32c.exe" -dPDFA=1 -dNOOUTERSAVE -sProcessColorModel=DeviceRGB -sDEVICE=pdfwrite -o RG_10690-pdfa.pdf -dPDFACompatibilityPolicy=1 "C:\Program Files (x86)\gs\gs9.52\lib\PDFA_def.ps" RG_10690.pdf
            // "C:\Program Files (x86)\gs\gs9.52\bin\gswin64c.exe" -dPDFA=1 -dNOOUTERSAVE -sProcessColorModel=DeviceRGB -sDEVICE=pdfwrite -o RG_10690-pdfa.pdf -dPDFACompatibilityPolicy=1 "C:\Program Files (x86)\gs\gs9.52\lib\PDFA_def.ps" RG_10690.pdf
            switches.Add(""); // ' Der allererste Parameter wird mitunter ignoriert weil EXEs da ihren eigenen Namen bergeben bekommen
            switches.Add("-P"); // Zugriff auf Ressourcen unterhalb des aktuellen Verzeichnisses erlauben
            switches.Add("-dPDFA=3"); // 'in A/3 umwandeln Teil 1 von 3
                                      // switches.Add("-dCompressStreams=false") ''hatten mal Probleme weil scheinbar auch die XMP Metadaten von ZUGFeRD komprimiert wurden, das hat sich mittlerweile erledigt
            switches.Add("-sColorConversionStrategy=RGB"); // ' muss fr PDF/A angegeben werden
            switches.Add("-sDEVICE=pdfwrite"); // ' ein Device muss frs Rastern angegeben werden
            switches.Add("-o" + mPDFOutFile); // ' Ausgabedatei
            switches.Add("-dPDFACompatibilityPolicy=1"); // 'in A/3 umwandeln Teil 2 von 3
            switches.Add("-dRenderIntent=3"); // 'in A/3 umwandeln Teil 3 von 3
            switches.Add("-sGenericResourceDir=\"" + resourceDir + "/\""); // ' hier kann ein zustzliches Verzeichnis angegeben werden in dem Ressourcen wie die icc-Datei liegen drfen
            switches.Add(file_BIGSCRIPT_PS); // ' die PDFMark-Programmdatei die interpretiert werden soll. Anders als die ICC und ggf. einzubettende XML-Datei ist das keine Ressourcendatei und die kann liegen wo sie will.
                                             // siehe https://www.adobe.com/content/dam/acom/en/devnet/acrobat/pdfs/pdfmark_reference.pdf
                                             // und https://gitlab.com/crossref/pdfmark
            switches.Add(mPDFInFile); // ' PDF-Eingabedatei

            bool success = false;
            using (GhostscriptProcessor gsProcessor = new GhostscriptProcessor(gsL))
            {
                VerboseMsgBoxOutput stdio = new VerboseMsgBoxOutput();
                // gsProcessor.StartProcessing(switches.ToArray(), stdio)
                gsProcessor.StartProcessing(switches.ToArray(), null/* TODO Change to default(_) if this is not a reference type */);

                // (erfolglose) Versuche, das Hngen zu vermeiden...
                gsProcessor.Dispose();
            }
            success = true;
            return success;
        }

        // <summary>
        // Zum Debuggen von Ghostscript ist es manchmal hilfreich, die Ausgabe in MsgBoxes umleiten zu knnen
        // Bekannte Fehlercodes -100: Datei nicht gefunden oder nicht beschreibbar 
        // </summary>
        public class VerboseMsgBoxOutput : GhostscriptStdIO
        {
            public VerboseMsgBoxOutput() : base(true, true, true)
            {
            }

            public override void StdOut(string output)
            {
                Console.Write("Out:" + output);
            }

            public override void StdError(string error)
            {
                Console.Write("Error:" + error);
            }

            public override void StdIn(out string input, int count)
            {
                input = "debug input";
            }
        }
    } }
