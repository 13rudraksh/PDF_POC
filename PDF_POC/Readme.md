# AWS Lambda Empty Function Project

This starter project consists of:
* Function.cs - class file containing a class with a single function handler method
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS

You may also have a test project depending on the options selected.

The generated function handler is a simple method accepting a string argument that returns the uppercase equivalent of the input string. Replace the body of this method, and parameters, to suit your needs. 

## Here are some steps to follow from Visual Studio:

To deploy your function to AWS Lambda, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed function open its Function View window by double-clicking the function name shown beneath the AWS Lambda node in the AWS Explorer tree.

To perform testing against your deployed function use the Test Invoke tab in the opened Function View window.

To configure event sources for your deployed function, for example to have your function invoked when an object is created in an Amazon S3 bucket, use the Event Sources tab in the opened Function View window.

To update the runtime configuration of your deployed function use the Configuration tab in the opened Function View window.

To view execution logs of invocations of your function use the Logs tab in the opened Function View window.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "PDF_POC/test/PDF_POC.Tests"
    dotnet test
```

Deploy function to AWS Lambda
```
    cd "PDF_POC/src/PDF_POC"
    dotnet lambda deploy-function
```

## PDF_POC: A3 Landscape Generator
This project is a Proof of Concept (POC) for generating high-fidelity, license-free PDF documents using .NET 10 and wkhtmltopdf. It is designed specifically for AWS Lambda deployment but includes a full local testing suite for Windows.

### 📂 Project Structure Explained
Assets/Fonts/: Contains ArianaVioleta.ttf. We use this via a CSS @font-face alias to ensure branding is consistent across Windows (Local) and Linux (Lambda).

Binaries/: Contains the Windows executables (wkhtmltopdf.exe and wkhtmltox.dll). In production, these are replaced by a Linux Lambda Layer.

Models/PdfRequest.cs: The data contract for the Lambda function.

Function.cs: The core logic that manages the memory streams and the conversion process.

TheGreatWallOfTables.html: A static stress-test file. If no HTML is provided in the request, the function defaults to this file to generate a massive, multi-table report.

### 🛠️ Local Setup (Mandatory)
The Windows "Printer" Fix:
wkhtmltopdf on Windows occasionally hangs with a "Waiting for printer" dialog. To fix this for local development:

Open Printers & Scanners in Windows Settings.

Uncheck "Let Windows manage my default printer".

Set "Microsoft Print to PDF" as your Default Printer.

### 🚀 How to Run the POC
Build the Project: Open the solution in Visual Studio 2026 and Build (Ctrl+Shift+B).

Launch Mock Tool: Run the Mock Lambda Test Tool-10.0 from the Debug menu.

Generate the "Big" Report:

Leave the HtmlBody empty in the JSON input.

Click Execute.

The function will pull data from TheGreatWallOfTables.html and generate an A3 Landscape PDF in your bin folder.

Sample JSON Input:

JSON

{
  "FileName": "Engineering_Audit_Report.pdf"
}
### ☁️ Deployment to AWS Lambda
Linux Pathing: The code automatically detects the OS. On Lambda, it looks for the binary at /opt/bin/wkhtmltopdf.

S3 Integration: For the final implementation, the File.WriteAllBytes logic should be swapped for an S3 Upload call using the AWS SDK.

License: This implementation uses LGPLv3, making it free for commercial use without the high costs of libraries like IronPDF or Aspose.
