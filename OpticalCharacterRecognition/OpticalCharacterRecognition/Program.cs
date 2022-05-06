using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpticalCharacterRecognition
{
    class Program
    {
        // Computer Vision subscription key and endpoint
        static string subscriptionKey = "44c59a3b22f44f14abd9e906dbb8383d";
        static string endpoint = "https://vision-ocr-api-test.cognitiveservices.azure.com/";
        static void Main(string[] args)
        {
            var client = Authenticate(endpoint, subscriptionKey);

            Console.WriteLine("Insert the image's URL: ");
            string urlFile = Console.ReadLine();

            if (Uri.IsWellFormedUriString(urlFile, UriKind.Absolute))
                ReadFileUrl(client, urlFile).Wait();
            else
                Console.WriteLine("\nThe inserted URL is not valid.");
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
        }

        public static async Task ReadFileUrl(ComputerVisionClient client, string urlFile)
        {
            Console.WriteLine("\nReading...");

            // Read text from URL
            var textHeaders = await client.ReadAsync(urlFile);
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            Console.WriteLine("\nExtracting text from image...");
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while (results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted);

            // Save results to .txt file
            Console.WriteLine("\nWriting text to file...");

            var lines = new List<string>();
            foreach (var line in results.AnalyzeResult.ReadResults[0].Lines)
                lines.Add(line.Text);

            File.WriteAllLines("results.txt", lines);
        }

    }
}
