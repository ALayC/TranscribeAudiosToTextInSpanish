using RestSharp;

/// <summary>
/// Provides functionality to transcribe audio files using the OpenAI Whisper API.
/// </summary>
public class WhisperService
{
    private readonly string _openAiApiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperService"/> class.
    /// </summary>
    /// <param name="openAiApiKey">The API key for OpenAI, used for authenticating requests.</param>
    public WhisperService(string openAiApiKey)
    {
        _openAiApiKey = openAiApiKey;
    }

    /// <summary>
    /// Transcribes an audio file using the OpenAI Whisper API.
    /// </summary>
    /// <param name="filePath">The local file path of the audio file to transcribe.</param>
    /// <returns>
    /// A string containing the transcription result (usually in JSON format), or an error message if the transcription fails.
    /// </returns>
    public string TranscribeAudio(string filePath)
    {
        // Create a RestClient for the OpenAI Whisper API endpoint.
        var client = new RestClient("https://api.openai.com/v1/audio/transcriptions");

        // Create a POST request.
        var request = new RestRequest("", Method.Post);

        // Add required headers for authentication and content type.
        request.AddHeader("Authorization", $"Bearer {_openAiApiKey}");
        request.AddHeader("Content-Type", "multipart/form-data");

        // Attach the audio file to the request.
        request.AddFile("file", filePath);

        // Specify the model parameter for the transcription.
        request.AddParameter("model", "whisper-1");

        // Execute the request and return the response content.
        var response = client.Execute(request);
        return response.Content;
    }
}
