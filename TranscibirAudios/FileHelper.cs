using System;
using System.Net;
using System.IO;

/// <summary>
/// Provides helper methods for file operations, such as downloading and deleting files.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Downloads a file from the specified URL and saves it locally with a unique filename.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <returns>
    /// The local file path of the downloaded file, or <c>null</c> if an error occurs.
    /// </returns>
    public static string DownloadFile(string url)
    {
        // Generate a unique file name using a GUID
        string filePath = $"audio_{Guid.NewGuid()}.ogg";
        try
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, filePath);
            }
            return filePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deletes the specified file from the local filesystem.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    public static void DeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
    }
}
