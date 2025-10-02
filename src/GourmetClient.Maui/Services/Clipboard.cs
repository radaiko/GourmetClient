using System;
using System.Threading.Tasks;

namespace GourmetClient.Maui.Services;

/// <summary>
/// MAUI-compatible clipboard service
/// </summary>
public static class Clipboard
{
    /// <summary>
    /// Sets text to the clipboard
    /// </summary>
    /// <param name="text">The text to set</param>
    public static async Task SetText(string text)
    {
        try
        {
            await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set clipboard text: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets text from the clipboard
    /// </summary>
    /// <returns>The clipboard text or empty string if failed</returns>
    public static async Task<string> GetText()
    {
        try
        {
            return await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.GetTextAsync() ?? string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get clipboard text: {ex.Message}");
            return string.Empty;
        }
    }
}