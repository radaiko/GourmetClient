using System;
using Microsoft.Extensions.DependencyInjection;

namespace GC.ViewModels.Services;

/// <summary>
/// Static holder for the application service provider.
/// Allows access to DI services throughout the application.
/// </summary>
public static class ServiceProviderHolder {
  private static IServiceProvider? _serviceProvider;

  public static void Initialize(IServiceProvider serviceProvider) {
    _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
  }

  public static IServiceProvider Services => _serviceProvider
                                             ?? throw new InvalidOperationException("ServiceProvider not initialized. Call Initialize() first.");
}