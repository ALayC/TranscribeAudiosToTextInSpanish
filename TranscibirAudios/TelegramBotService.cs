using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;

/// <summary>
/// Provides a service for managing the Telegram bot, including receiving updates,
/// processing voice messages, transcribing audio via the WhisperService, summarizing text via the GptService,
/// and sending responses back to users.
/// </summary>
public class TelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly WhisperService _whisperService;
    private readonly GptService _gptService;
    private readonly string _telegramToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramBotService"/> class.
    /// </summary>
    /// <param name="telegramToken">The token for the Telegram bot.</param>
    /// <param name="whisperService">An instance of <see cref="WhisperService"/> for transcribing audio.</param>
    /// <param name="gptService">An instance of <see cref="GptService"/> for summarizing text.</param>
    public TelegramBotService(string telegramToken, WhisperService whisperService, GptService gptService)
    {
        _telegramToken = telegramToken;
        _botClient = new TelegramBotClient(telegramToken);
        _whisperService = whisperService;
        _gptService = gptService;
    }

    /// <summary>
    /// Starts receiving updates from Telegram and processes them.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken
        );

        Console.WriteLine("The bot has started.");

        // Espera indefinidamente para mantener el proceso en ejecución.
        await Task.Delay(-1, cancellationToken);
    }

    /// <summary>
    /// Handles incoming updates from Telegram.
    /// Processes voice messages by downloading the audio, transcribing it with WhisperService,
    /// summarizing the transcription with GptService, and sending the results back to the user.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="update">The update received from Telegram.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Voice)
        {
            Console.WriteLine("Voice message received...");
            var voice = update.Message.Voice;
            var file = await botClient.GetFileAsync(voice.FileId, cancellationToken);
            string fileUrl = $"https://api.telegram.org/file/bot{_telegramToken}/{file.FilePath}";

            // Generate a unique file name and download the audio file
            string localFilePath = FileHelper.DownloadFile(fileUrl);
            if (localFilePath != null)
            {
                // Transcribe the audio using WhisperService
                string responseContent = _whisperService.TranscribeAudio(localFilePath);

                // Deserialize the transcription result, ignoring case differences
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var transcriptionResult = JsonSerializer.Deserialize<TranscriptionResponse>(responseContent, options);

                // Check that the transcription was successful
                if (transcriptionResult != null && !string.IsNullOrWhiteSpace(transcriptionResult.Text))
                {
                    // Send the transcription to the user
                    string transcriptionMessage = $"Transcripcion:\n\n{transcriptionResult.Text}";
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, transcriptionMessage, cancellationToken: cancellationToken);

                    // Generate a summary using GptService and send it as another message
                    string summary = _gptService.SummarizeText(transcriptionResult.Text);
                    string summaryMessage = $"Resumen:\n\n{summary}";
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, summaryMessage, cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "The audio could not be transcribed properly.", cancellationToken: cancellationToken);
                }

                // Delete the downloaded audio file to free resources
                FileHelper.DeleteFile(localFilePath);
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Error downloading the audio.", cancellationToken: cancellationToken);
            }
        }
        else if (update.Type == UpdateType.Message)
        {
            // If the message is not a voice message, prompt the user to send one
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Please send me a voice message to transcribe.", cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Handles errors that occur while processing updates.
    /// </summary>
    /// <param name="botClient">The Telegram bot client.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}
