using System;
using System.IO;
using GourmetClient.Utils;

namespace GourmetClient.Utils;

/// <summary>
/// WPF-specific implementation of IFilePathProvider
/// </summary>
public class WpfFilePathProvider : IFilePathProvider
{
    public string LocalAppDataPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GourmetClient");
}