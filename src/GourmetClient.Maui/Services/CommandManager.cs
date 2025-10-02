namespace GourmetClient.Maui.Services;

/// <summary>
/// MAUI equivalent of WPF's CommandManager for invalidating command states
/// </summary>
public static class CommandManager
{
    /// <summary>
    /// In MAUI, command invalidation is typically handled automatically by data binding.
    /// This method exists for compatibility but doesn't need to perform any action.
    /// </summary>
    public static void InvalidateRequerySuggested()
    {
        // In MAUI, command CanExecute is typically handled automatically
        // through property change notifications and data binding
        // No action needed here, but we keep the method for compatibility
    }
}