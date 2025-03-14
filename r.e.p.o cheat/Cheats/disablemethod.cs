using System;
using System.Reflection;

namespace r.e.p.o_cheat
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
                Hax2.Log1("PlayerTumble type not found.");
                return;
            }

            playerTumbleInstance = GameHelper.FindObjectOfType(playerTumbleType);
            if (playerTumbleInstance == null)
            {
                Hax2.Log1("PlayerTumble instance not found in the scene.");
            }
            else
            {
                Hax2.Log1("PlayerTumble instance updated successfully.");
            }
        }

        public static void DisableMethod(string methodName)
        {
            if (methodName == "Update" || methodName == "Setup")
            {
                Hax2.Log1($"Skipping disable for critical method: {methodName}");
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
                    Hax2.Log1($"Cannot modify method {methodName} because PlayerTumble instance is null.");
                    return;
                }
            }

            MethodInfo method = playerTumbleType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Hax2.Log1($"Method {methodName} not found in PlayerTumble.");
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

            Hax2.Log1($"Modified method: {methodName} (Patched with {BitConverter.ToString(patch)})");
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