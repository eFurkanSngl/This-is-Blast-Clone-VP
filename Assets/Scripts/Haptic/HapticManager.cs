using UnityEngine;

public static class HapticManager
{
    public enum HapticType
    {
        Light,
        Medium,
        Heavy
    }

    public static void PlayHaptic(HapticType type)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        long duration = type switch
        {
            HapticType.Light => 20,
            HapticType.Medium => 50,
            HapticType.Heavy => 100,
            _ => 50
        };

        Vibrate(duration);
#else
        Debug.Log($"[Haptic] {type} titreşimi (Editor simülasyonu)");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void Vibrate(long milliseconds)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator != null)
                {
                    if (AndroidVersion() >= 26)
                    {
                        // API 26+ için VibrationEffect kullan
                        AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                        AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                            "createOneShot", milliseconds, vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE")
                        );
                        vibrator.Call("vibrate", effect);
                    }
                    else
                    {
                        vibrator.Call("vibrate", milliseconds);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"HapticManager exception: {e.Message}");
        }
    }

    private static int AndroidVersion()
    {
        using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return versionClass.GetStatic<int>("SDK_INT");
        }
    }
#endif
}
