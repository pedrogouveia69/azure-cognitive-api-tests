using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System;
using System.Media;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechTranslation
{
    class Program
    {
        public static async Task TranslationContinuousRecognitionAsync()
        {
            string subscriptionKey = "";
            // Creates an instance of a speech translation config with specified subscription key and service region.
            var config = SpeechTranslationConfig.FromSubscription(subscriptionKey, "eastus");

            // Sets source and target languages.
            config.SpeechRecognitionLanguage = "pt-PT";
            config.AddTargetLanguage("en");

            // Sets voice name of synthesis output.
            const string VoiceName = "en-US-AriaNeural";
            config.VoiceName = VoiceName;

            // Creates a translation recognizer using microphone as audio input.
            using var recognizer = new TranslationRecognizer(config);
            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.TranslatedSpeech)
                {
                    if (e.Result.Text.Length > 0)
                    {
                        Console.WriteLine("\nYou said: " + e.Result.Text);
                        // Working with a single target language
                        Console.WriteLine("Translation: " + e.Result.Translations.First().Value);
                    }
                    else
                    {
                        Console.WriteLine("\nPlease say something...");
                    }
                }
            };

            recognizer.Synthesizing += (s, e) =>
            {
                var audio = e.Result.GetAudio();
                if (audio.Length > 0)
                {
                    // Construct the sound player
                    using var player = new SoundPlayer(new MemoryStream(audio));
                    player.Play();
                }
            };

            // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
            Console.WriteLine("Listening...");
            await recognizer.StartContinuousRecognitionAsync();

            do
            {
                Console.WriteLine("Press 'Esc' to STOP");
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            // Stops continuous recognition.
            await recognizer.StopContinuousRecognitionAsync();
        }

        static async Task Main(string[] args)
        {
            await TranslationContinuousRecognitionAsync();
        }
    }
}
