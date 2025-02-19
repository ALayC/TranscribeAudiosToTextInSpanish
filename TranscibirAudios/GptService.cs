using System;
using System.Text.Json;
using RestSharp;

/// <summary>
/// Provides methods for interacting with the OpenAI GPT API to generate summaries of text.
/// </summary>
public class GptService
{
    private readonly string _openAiApiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="GptService"/> class.
    /// </summary>
    /// <param name="openAiApiKey">The OpenAI API key used for authentication.</param>
    public GptService(string openAiApiKey)
    {
        _openAiApiKey = openAiApiKey;
    }

    /// <summary>
    /// Summarizes the provided text using the OpenAI GPT API in Spanish.
    /// </summary>
    /// <param name="text">The text to summarize.</param>
    /// <returns>A concise summary of the text in Spanish, or an error message if the summary could not be generated.</returns>
    public string SummarizeText(string text)
    {
        // Create a RestClient to connect to the GPT API endpoint.
        var client = new RestClient("https://api.openai.com/v1/chat/completions");

        // Create a POST request.
        var request = new RestRequest("", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_openAiApiKey}");
        request.AddHeader("Content-Type", "application/json");

        // Define the payload for the GPT API call in Spanish.
        var payload = new
        {
            model = "gpt-3.5-turbo",
            messages = new object[]
            {
                new { role = "system", content = "Eres un asistente que resume textos de manera clara y concisa en español." },
                new { role = "user", content = $"Resume el siguiente texto en pocas oraciones:\n\n{text}" }
            },
            max_tokens = 150,
            temperature = 0.7
        };

        // Serialize the payload to JSON.
        string jsonBody = JsonSerializer.Serialize(payload);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

        // Execute the request.
        var response = client.Execute(request);

        // Check if the request was unsuccessful.
        if (!response.IsSuccessful)
        {
            Console.WriteLine($"Error generating summary: {response.ErrorMessage}");
            return "Unable to generate summary.";
        }

        try
        {
            // Parse the JSON response and extract the summary text.
            using (JsonDocument doc = JsonDocument.Parse(response.Content))
            {
                // Expected structure: { "choices": [ { "message": { "content": "The summary..." } } ] }
                var summary = doc.RootElement
                                 .GetProperty("choices")[0]
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();
                return summary.Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing summary: {ex.Message}");
            return "Unable to generate summary.";
        }
    }
}
