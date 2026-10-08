using DotNative.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.LocalAuth;

public interface ILocalAuth
{
    Task<bool> CanAuthenticateAsync(
        bool allowDeviceCredential = true,
        CancellationToken cancellationToken = default
    );
    Task<bool> AuthenticateAsync(
        string reason,
        bool allowDeviceCredential = true,
        CancellationToken cancellationToken = default
    );
}

public static class LocalAuthServices
{
    public static IServiceCollection AddLocalAuth(this IServiceCollection services)
    {
        services.TryAddSingleton<ILocalAuth, ChannelLocalAuth>();
        return services;
    }
}

internal sealed class ChannelLocalAuth(IPlatformChannels channels) : ILocalAuth
{
    private MethodChannel Channel => channels.Get("dotnative.local-auth");

    public Task<bool> CanAuthenticateAsync(
        bool allowDeviceCredential = true,
        CancellationToken cancellationToken = default
    ) =>
        InvokeBool(
            "canAuthenticate",
            new Dictionary<string, object?> { ["allowDeviceCredential"] = allowDeviceCredential },
            cancellationToken
        );

    public async Task<bool> AuthenticateAsync(
        string reason,
        bool allowDeviceCredential = true,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (reason.Length > 512)
            throw new ArgumentOutOfRangeException(nameof(reason));
        return await InvokeBool(
                "authenticate",
                new Dictionary<string, object?>
                {
                    ["reason"] = reason,
                    ["allowDeviceCredential"] = allowDeviceCredential,
                },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private async Task<bool> InvokeBool(string method, object args, CancellationToken token)
    {
        var value = await Channel.InvokeAsync(method, args, token).ConfigureAwait(false);
        return value is bool result
            ? result
            : throw new InvalidDataException($"Invalid {method} response.");
    }
}
