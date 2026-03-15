using Amazon.Lambda.Core;
using PDF_POC.Models;
using System.Diagnostics;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PDF_POC;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public string FunctionHandler(PdfRequest request, ILambdaContext? context = null)
    {
        // Null-safe base directory
        string baseDir = Path.GetDirectoryName(typeof(Function).Assembly.Location)
                         ?? AppContext.BaseDirectory;

        // 1. Resolve Paths
        bool isWindows = OperatingSystem.IsWindows();
        string wkhtmlPath = isWindows
            ? Path.Combine(baseDir, "Binaries", "wkhtmltopdf.exe")
            : "/opt/bin/wkhtmltopdf";

        string fontPath = isWindows
            ? Path.Combine(baseDir, "Assets", "Fonts", "ArianaVioleta.ttf")
            : "/opt/fonts/ArianaVioleta.ttf";

        string cssFontPath = fontPath.Replace("\\", "/");

        // 2. Load the Static "Big" File
        string staticFileName = "TheGreatWallOfTables.html";
        string staticFilePath = Path.Combine(baseDir, staticFileName);

        if (!File.Exists(staticFilePath))
            return $"Error: Static file '{staticFileName}' not found in {baseDir}";

        string rawHtmlContent = File.ReadAllText(staticFilePath);

        // 3. Work Files
        string tempHtmlPath = Path.Combine(baseDir, $"{Guid.NewGuid()}.html");
        string tempPdfPath = Path.Combine(baseDir, $"{Guid.NewGuid()}.pdf");

        try
        {
            // Inject dynamic style wrapper (ensures Font works for every team member)
            string fullHtml = $@"
            <html>
            <head>
                <style>
                    @font-face {{ 
                        font-family: 'BrandFont'; 
                        src: url('file:///{cssFontPath}'); 
                    }}
                    body {{ 
                        font-family: 'BrandFont', sans-serif; 
                        margin: 40px; 
                        -webkit-print-color-adjust: exact; 
                    }}
                    table {{ width: 100%; border-collapse: collapse; page-break-inside: auto; }}
                    th {{ background-color: #004a99; color: white; padding: 12px; border: 1px solid #333; text-align: left; }}
                    td {{ padding: 10px; border: 1px solid #ccc; font-size: 14px; }}
                    h2 {{ color: #004a99; border-bottom: 2px solid #004a99; padding-bottom: 5px; }}
                </style>
            </head>
            <body>
                {rawHtmlContent}
            </body>
            </html>";

            File.WriteAllText(tempHtmlPath, fullHtml);

            // 4. Execution (Forced A3 Landscape)
            var startInfo = new ProcessStartInfo
            {
                FileName = wkhtmlPath,
                Arguments = $"-q -s A3 -O Landscape --enable-local-file-access \"{tempHtmlPath}\" \"{tempPdfPath}\"",
                WorkingDirectory = baseDir,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();

            if (process?.ExitCode != 0)
                return $"Binary Error: {process?.StandardError.ReadToEnd()}";

            // 5. Final Output Save
            byte[] pdfBytes = File.ReadAllBytes(tempPdfPath);
            string outputFileName = string.IsNullOrWhiteSpace(request.FileName) ? "Audit_Report_A3.pdf" : request.FileName;
            string outputPath = Path.Combine(baseDir, outputFileName);

            File.WriteAllBytes(outputPath, pdfBytes);

            return $"Success! PDF generated at: {outputPath}";
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
        finally
        {
            if (File.Exists(tempHtmlPath)) File.Delete(tempHtmlPath);
            if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath);
        }
    }

}
