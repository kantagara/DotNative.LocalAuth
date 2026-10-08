using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNative.LocalAuth;

public static class LocalAuthServiceProviderExtensions
{
#if NET10_0_OR_GREATER
    extension(IServiceProvider services)
    {
        /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
        public ILocalAuth LocalAuth => services.GetRequiredService<ILocalAuth>();
    }
#else
    /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
    public static ILocalAuth LocalAuth(this IServiceProvider services) =>
        services.GetRequiredService<ILocalAuth>();
#endif
}
