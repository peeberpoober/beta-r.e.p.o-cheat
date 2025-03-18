using System;
using System.Reflection;

namespace dark_cheat
{
    static class PlayerTumbleManager
    {
        private static Type playerTumbleType = Type.GetType("PlayerTumble, Assembly-CSharp");
        private static object playerTumbleInstance;

        private static readonly byte[] disableBytes = { 0xC3 };
        private static readonly byte[] enableBytes = { 0x55 };

        public static void Initialize()
        {
            if (playerTumbleType == null)
            {
                DLog.Log("PlayerTumble type not found.");
                return;
            }

            playerTumbleInstance = GameHelper.FindObjectOfType(playerTumbleType);
            if (playerTumbleInstance == null)
            {
                DLog.Log("PlayerTumble instance not found in the scene.");
            }
            else
            {
                DLog.Log("PlayerTumble instance updated successfully.");
            }
        }

        public static void DisableMethod(string methodName)
        {
            if (methodName == "Update" || methodName == "Setup")
            {
                DLog.Log($"Skipping disable for critical method: {methodName}");
                return;
            }
            ModifyMethod(methodName, disableBytes);
        }
        public static void EnableMethod(string methodName)
        {
            ModifyMethod(methodName, enableBytes);
        }

        private static void ModifyMethod(string methodName, byte[] patch)
        {
            if (playerTumbleType == null || playerTumbleInstance == null)
            {
                Initialize();
                if (playerTumbleInstance == null)
                {
                    DLog.Log($"Cannot modify method {methodName} because PlayerTumble instance is null.");
                    return;
                }
            }

            MethodInfo method = playerTumbleType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                DLog.Log($"Method {methodName} not found in PlayerTumble.");
                return;
            }

            // Get method pointer and apply patch
            IntPtr methodPtr = method.MethodHandle.GetFunctionPointer();
            unsafe
            {
                byte* ptr = (byte*)methodPtr.ToPointer();
                for (int i = 0; i < patch.Length; i++)
                {
                    ptr[i] = patch[i]; // Overwrite bytes
                }
            }

            DLog.Log($"Modified method: {methodName} (Patched with {BitConverter.ToString(patch)})");
        }

        public static void DisableAll()
        {
            DisableMethod("ImpactHurtSet");
            DisableMethod("ImpactHurtSetRPC");
            DisableMethod("Update");
            DisableMethod("TumbleSet");
            DisableMethod("Setup");
        }

        public static void EnableAll()
        {
            EnableMethod("ImpactHurtSet");
            EnableMethod("ImpactHurtSetRPC");
            EnableMethod("Update");
            EnableMethod("TumbleSet");
            EnableMethod("Setup");
        }
    }
}