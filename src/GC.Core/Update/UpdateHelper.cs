using System.Threading.Tasks;

namespace GC.Core.Update;

public static class UpdateHelper {
  // Starts the update process for the given release. UI logic should be handled by the platform-specific project.
  public static async Task StartUpdateAsync(ReleaseDescription updateRelease, IUpdateHandler updateHandler) {
    await updateHandler.HandleUpdateAsync(updateRelease);
  }
}

// Interface for platform-specific update handling (UI, notifications, etc.)
public interface IUpdateHandler {
  Task HandleUpdateAsync(ReleaseDescription updateRelease);
}