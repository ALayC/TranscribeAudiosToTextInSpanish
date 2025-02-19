using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The main entry point of the application.
/// This program initializes the configuration, retrieves required credentials,
/// and starts the Telegram bot service.
/// </summary>
class Program
{
    /// <summary>
    /// Main method that builds the configuration from various sources, 
    /// retrieves the necessary tokens, and starts the bot service.
    /// </summary>
    static async Task Main()
    {
        // Build the configuration from appsettings.json, User Secrets, and environment variables.
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>() // Load User Secrets for local development
            .AddEnvironmentVariables()  // Allow overriding with environment variables
            .Build();

        // Retrieve the Telegram token and OpenAI API key from the configuration.
        string telegramToken = config["TELEGRAM_TOKEN"];
        string openAiApiKey = config["OPENAI_API_KEY"];

        // Check if the required credentials are available.
        if (string.IsNullOrEmpty(telegramToken) || string.IsNullOrEmpty(openAiApiKey))
        {
            Console.WriteLine("Credentials are not configured properly.");
            return;
        }

        // Initialize the necessary services for the bot.
        var whisperService = new WhisperService(openAiApiKey);
        var gptService = new GptService(openAiApiKey);
        var botService = new TelegramBotService(telegramToken, whisperService, gptService);

        // Create a cancellation token and start the bot service.
        using var cts = new CancellationTokenSource();
        await botService.StartAsync(cts.Token);
    }
}
