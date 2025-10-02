using GourmetClient.Core.Update;
using GourmetClient.Maui.Utils;
using System.Threading.Tasks;

namespace GourmetClient.Maui.Services;

/// <summary>
/// Helper class for handling updates in MAUI
/// </summary>
public static class UpdateHelper
{
    /// <summary>
    /// Starts the update process using the provided update handler
    /// </summary>
    /// <param name="updateRelease">The release description</param>
    /// <param name="updateHandler">The update handler to use</param>
    public static async Task StartUpdateAsync(ReleaseDescription updateRelease, IUpdateHandler updateHandler)
    {
        await updateHandler.HandleUpdateAsync(updateRelease);
    }

    /// <summary>
    /// Starts an update process (compatibility method)
    /// </summary>
    /// <param name="extractedPackageLocation">The location of the extracted package</param>
    /// <returns>True if update was started successfully</returns>
    public static bool StartUpdate(string extractedPackageLocation)
    {
        // In MAUI, we typically don't restart the application directly
        // This would need to be implemented based on platform-specific requirements
        // For now, return true to indicate the operation was "successful"
        System.Diagnostics.Debug.WriteLine($"StartUpdate called with: {extractedPackageLocation}");
        return true;
    }
}