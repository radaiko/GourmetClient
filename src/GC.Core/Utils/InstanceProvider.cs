using System;
using GC.Core.Network;
using GC.Core.Notifications;
using GC.Core.Settings;
using GC.Core.Update;

namespace GC.Core.Utils;

public static class InstanceProvider
{
    private static IFilePathProvider? _filePathProvider;
    private static GourmetWebClient? _gourmetWebClient;
    private static VentopayWebClient? _ventopayWebClient;
    private static GourmetCacheService? _gourmetCacheService;
    private static NotificationService? _notificationService;
    private static BillingCacheService? _billingCacheService;
    private static GourmetSettingsService? _settingsService;
    private static UpdateService? _updateService;

    public static void Initialize(IFilePathProvider filePathProvider)
    {
        _filePathProvider = filePathProvider;

        // Reset all services to ensure they use the new file path provider
        _gourmetCacheService = null;
        _settingsService = null;
        _updateService = null;
        _billingCacheService = null;
    }

    internal static IFilePathProvider FilePathProvider
    {
        get
        {
            if (_filePathProvider is null)
                throw new InvalidOperationException("InstanceProvider must be initialized with a FilePathProvider before use");
            return _filePathProvider;
        }
    }

    public static GourmetWebClient GourmetWebClient => _gourmetWebClient ??= new GourmetWebClient();

    public static VentopayWebClient VentopayWebClient => _ventopayWebClient ??= new VentopayWebClient();

    public static GourmetCacheService GourmetCacheService => _gourmetCacheService ??= new GourmetCacheService(FilePathProvider);

    public static NotificationService NotificationService => _notificationService ??= new NotificationService();

    public static BillingCacheService BillingCacheService => _billingCacheService ??= new BillingCacheService();

    public static GourmetSettingsService SettingsService => _settingsService ??= new GourmetSettingsService(FilePathProvider);

    public static UpdateService UpdateService => _updateService ??= new UpdateService(FilePathProvider);
}