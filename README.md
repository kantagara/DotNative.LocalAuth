# DotNative.LocalAuth

Requests local user authentication through Android and Apple system prompts.

```csharp
builder.Services.AddLocalAuth();
var auth = services.LocalAuth;
if (await auth.CanAuthenticateAsync())
{
    var accepted = await auth.AuthenticateAsync("Confirm this action");
}
```

On iOS/macOS, LocalAuthentication uses the system's device-owner policy and can
use biometrics or the OS credential, depending on the device and the
`allowDeviceCredential` option. On Android, this release uses the system device
credential confirmation screen (PIN/pattern/password). It does not include the
AndroidX BiometricPrompt dependency, so Android biometric-only mode is unavailable
and `allowDeviceCredential: false` returns a clear `not_available` error.

| Platform | Status |
| --- | --- |
| Android | System device credential screen |
| iOS | LocalAuthentication |
| macOS | LocalAuthentication |
| Windows | Not implemented |
| Linux | Not implemented |

The OS can reject authentication when no passcode or secure lock screen is set.
User cancellation returns `false`; unavailable or failed system calls raise a
`PluginException`. The independent DotNative implementation is MIT licensed.

## Service access

Import `DotNative.LocalAuth` to access the plugin through `IServiceProvider`:

```csharp
using DotNative.LocalAuth;

var plugin = services.LocalAuth;
```

The getter calls `GetRequiredService<ILocalAuth>()` on every access, preserving
DI lifetimes and the usual missing-registration error. Register the plugin with
`AddLocalAuth(...)` before building the provider.

A `net10.0` application uses the property syntax with C# 14 or later. A
`net9.0` application uses only the method equivalent:

```csharp
var plugin = services.LocalAuth();
```

The package contains separate `net9.0` and `net10.0` assemblies. NuGet selects
the assembly matching the application target framework. `NET10_0_OR_GREATER`
selects the property; the `#else` branch selects the method.

Build and pack both targets with .NET 10 SDK. A source build using .NET 9 SDK
builds only `net9.0`; it does not produce the .NET 10 assembly.
