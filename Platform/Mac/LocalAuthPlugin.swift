import Foundation
import LocalAuthentication

@MainActor final class LocalAuthPlugin {
    private static var instance: LocalAuthPlugin?
    static func register() {
        if instance == nil {
            instance = LocalAuthPlugin()
        }
    }

    private init() {
        let channel = NativeChannels.channel("dotnative.local-auth")
        channel.handle("canAuthenticate") {
            args, reply in
            let context = LAContext()
            var error: NSError?
            let allowCredential = self.allowCredential(args)
            let policy: LAPolicy =
                allowCredential
                ? .deviceOwnerAuthentication : .deviceOwnerAuthenticationWithBiometrics
            reply.success(.bool(context.canEvaluatePolicy(policy, error: &error)))
        }
        channel.handle("authenticate") {
            args, reply in
            let fields = args.fields
            guard let reason = fields["reason"]?.string, !reason.isEmpty else {
                reply.failure("invalid_argument", "An authentication reason is required")
                return
            }
            let allowCredential = self.allowCredential(args)
            let policy: LAPolicy =
                allowCredential
                ? .deviceOwnerAuthentication : .deviceOwnerAuthenticationWithBiometrics
            let context = LAContext()
            var error: NSError?
            guard context.canEvaluatePolicy(policy, error: &error) else {
                reply.failure(
                    "not_available",
                    error?.localizedDescription ?? "No authentication method is available")
                return
            }
            reply.onCancel = {
                context.invalidate()
            }
            context.evaluatePolicy(policy, localizedReason: reason) {
                success, evaluationError in
                Task {
                    @MainActor in
                    if let evaluationError {
                        if (evaluationError as NSError).code == LAError.userCancel.rawValue {
                            reply.success(.bool(false))
                        } else {
                            reply.failure(
                                "authentication_failed", evaluationError.localizedDescription)
                        }
                    } else {
                        reply.success(.bool(success))
                    }
                }
            }
        }
    }

    private func allowCredential(_ arguments: PluginValue) -> Bool {
        if case .bool(let value)? = arguments.fields["allowDeviceCredential"] {
            return value
        }
        return true
    }
}
