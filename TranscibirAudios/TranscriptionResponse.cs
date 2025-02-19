using System.Text.Json.Serialization;

/// <summary>
/// Represents the response returned by the transcription service (e.g., the Whisper API).
/// </summary>
public class TranscriptionResponse
{
    /// <summary>
    /// Gets or sets the transcribed text from the audio.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
