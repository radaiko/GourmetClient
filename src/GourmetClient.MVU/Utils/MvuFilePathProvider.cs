using System;
using System.IO;
using GourmetClient.Core.Utils;

namespace GourmetClient.MVU.Utils;

public class MvuFilePathProvider : IFilePathProvider
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
