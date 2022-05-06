using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Azure;
using Azure.AI.TextAnalytics;
using System.IO;

namespace TextSummarization
{
    class Program
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("3430760e509643b285878d0ef9eb7d9d");
        private static readonly Uri endpoint = new Uri("https://text-analytics-api-test.cognitiveservices.azure.com/");

        static async Task TextSummarization(TextAnalyticsClient client, string text)
        {
            // Prepare analyze operation input. You can add multiple documents to this list and perform the same
            // operation to all of them.
            var batchInput = new List<string> { text };
            var actions = new TextAnalyticsActions()
            {
                // Add the desired TextAnalytics API actions here
                ExtractSummaryActions = new List<ExtractSummaryAction>() { new ExtractSummaryAction() },
            };

            // Start analysis process.
            Console.WriteLine("\nREADING...");
            var response = await client.StartAnalyzeActionsAsync(batchInput, actions);
            await response.WaitForCompletionAsync();

            // Cycle through the different TextAnalytics API actions          
            await foreach (var action in response.Value)
            {
                foreach (var actionResult in action.ExtractSummaryResults)
                {
                    foreach (var summary in actionResult.DocumentsResults)
                    {
                        Console.WriteLine("\nSUMMARY:\n");
                        string textSummary = "";
                        foreach (var sentence in summary.Sentences)
                        {
                            Console.WriteLine(sentence.Text);
                            textSummary += sentence.Text + "\n";
                        }

                        var res = await client.ExtractKeyPhrasesAsync(textSummary);
                        Console.WriteLine("\nKEY WORDS:\n");
                        foreach (var keyWord in res.Value)
                        {                          
                            Console.Write(keyWord + " | ");
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("INSERT TEXT TO SUMMARIZE:\n");
            string text = Console.ReadLine();

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("No text was inserted.");
            }
            else
            {
                var client = new TextAnalyticsClient(endpoint, credentials);
                await TextSummarization(client, text);
            }
        }
    }
}
