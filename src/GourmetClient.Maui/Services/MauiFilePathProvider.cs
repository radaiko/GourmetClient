using GourmetClient.Core.Utils;
using Microsoft.Maui.Storage;

namespace GourmetClient.Maui.Services;

/// <summary>
/// MAUI-specific implementation of IFilePathProvider
/// </summary>
internal class MauiFilePathProvider : IFilePathProvider
{
    public string LocalAppDataPath { get; } = FileSystem.AppDataDirectory;
}