using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using GourmetClient.Serialization;

namespace GourmetClient.Settings;

public class GourmetSettingsService
{
    private readonly string _settingsFileName;

    private GourmetClientSettings? _currentSettings;

    public event EventHandler? SettingsSaved;

    public GourmetSettingsService()
    {
        _settingsFileName = Path.Combine(App.LocalAppDataPath, "GourmetClientSettings.json");
    }

    public UserSettings GetCurrentUserSettings()
    {
        return GetCurrentSettings().UserSettings;
    }

    public void SaveUserSettings(UserSettings userSettings)
    {
        var settings = GetCurrentSettings();
        settings.UserSettings = userSettings;

        SaveSettings(settings);
    }

    public WindowSettings? GetCurrentWindowSettings()
    {
        return GetCurrentSettings().WindowSettings;
    }

    public void SaveWindowSettings(WindowSettings windowSettings)
    {
        var settings = GetCurrentSettings();
        settings.WindowSettings = windowSettings;

        SaveSettings(settings);
    }

    public UpdateSettings GetCurrentUpdateSettings()
    {
        return GetCurrentSettings().UpdateSettings;
    }

    public void SaveUpdateSettings(UpdateSettings updateSettings)
    {
        var settings = GetCurrentSettings();
        settings.UpdateSettings = updateSettings;

        SaveSettings(settings);
    }

    private GourmetClientSettings GetCurrentSettings()
    {
        if (_currentSettings is null)
        {
            _currentSettings = ReadSettingsFromFile();
        }

        return _currentSettings;
    }

    private GourmetClientSettings ReadSettingsFromFile()
    {
        if (!File.Exists(_settingsFileName))
        {
            return new GourmetClientSettings();
        }

        SerializableGourmetClientSettings? serializedSettings = null;
        GourmetClientSettings? settings = null;

        try
        {
            using var fileStream = new FileStream(_settingsFileName, FileMode.Open, FileAccess.Read, FileShare.None);
            serializedSettings = JsonSerializer.Deserialize<SerializableGourmetClientSettings>(fileStream);
        }
        catch (Exception exception) when (exception is IOException || exception is JsonException)
        {
        }

        try
        {
            settings = serializedSettings?.ToGourmetSettings();
        }
        catch (InvalidOperationException)
        {
        }

        return settings ?? new GourmetClientSettings();
    }

    private void SaveSettings(GourmetClientSettings settings)
    {
        var serializedSettings = SerializableGourmetClientSettings.FromGourmetClientSettings(settings);

        try
        {
            var settingsDirectory = Path.GetDirectoryName(_settingsFileName);
            Debug.Assert(settingsDirectory is not null);

            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            using var fileStream = new FileStream(_settingsFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(fileStream, serializedSettings, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (IOException)
        {
        }

        SettingsSaved?.Invoke(this, EventArgs.Empty);
    }
}