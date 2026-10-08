package com.dotnative.plugins

import android.app.Activity
import android.app.KeyguardManager
import android.content.Intent

class LocalAuthPlugin(private val activity: Activity) {

    init {

        val channel = NativeChannels.channel("dotnative.local-auth")
        channel.handle("canAuthenticate") { args, reply ->
            val allowCredential =
                (args as? Map<*, *>)?.get("allowDeviceCredential") as? Boolean ?: true
            reply.success(allowCredential && keyguard().isDeviceSecure)
        }
        channel.handle("authenticate") { args, reply ->
            val fields = args as? Map<*, *>
            val allowCredential = fields?.get("allowDeviceCredential") as? Boolean ?: true
            val reason = fields?.get("reason") as? String
            if (reason.isNullOrBlank()) {

                reply.failure("invalid_argument", "An authentication reason is required")
                return@handle
            }
            if (!allowCredential) {

                reply.failure(
                    "not_available",
                    "This Android backend uses the device credential flow",
                )
                return@handle
            }
            if (!keyguard().isDeviceSecure) {

                reply.failure("not_available", "No secure device credential is configured")
                return@handle
            }
            try {

                val intent: Intent? =
                    keyguard().createConfirmDeviceCredentialIntent("Authenticate", reason)
                if (intent == null) {

                    reply.failure("not_available", "The system credential screen is unavailable")
                    return@handle
                }
                val code =
                    NativeChannels.launch(activity, intent) { result, _ ->
                        reply.success(result == Activity.RESULT_OK)
                    }
                reply.onCancel = {
                    NativeChannels.cancelResult(code)
                    runCatching {
                        activity.finishActivity(code)
                    }
                }
            } catch (error: Exception) {

                reply.failure(
                    "authentication_failed",
                    error.message ?: "Could not start device authentication",
                )
            }
        }
    }

    private fun keyguard() =
        activity.getSystemService(KeyguardManager::class.java)
            ?: error("Keyguard service is unavailable")
}
