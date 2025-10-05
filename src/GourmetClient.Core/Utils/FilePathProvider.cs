using System;
using System.IO;

namespace GourmetClient.Core.Utils;

/// <summary>
/// Standard file path provider for GourmetClient applications
/// </summary>
public class FilePathProvider : IFilePathProvider
{
    public string LocalAppDataPath
    {
        get
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var gourmetFolder = Path.Combine(appDataFolder, "GourmetClient");
            Directory.CreateDirectory(gourmetFolder);
            return gourmetFolder;
        }
    }
}
