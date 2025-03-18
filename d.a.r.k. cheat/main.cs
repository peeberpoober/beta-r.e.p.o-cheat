using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace dark_cheat
{

    public class Loader
    {
        [HarmonyPatch(typeof(SpectateCamera), "PlayerSwitch")]
        public static class SpectateCamera_PlayerSwitch_Patch
        {
            static bool Prefix(bool _next)
            {
                if (Hax2.showMenu)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButtonUp", new Type[] { typeof(int) })]
        public class Patch_Input_GetMouseButtonUp
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Input), "GetMouseButtonDown", new Type[] { typeof(int) })]
        public class Patch_Input_GetMouseButtonDown
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButton", new Type[] { typeof(int) })]
        public class Patch_Input_GetMouseButton
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        public static void Init()
        {
            Loader.Load = new GameObject();
            Loader.Load.AddComponent<Hax2>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.Load);

            var Harmony = new Harmony("dark_cheat");
            try
            {
                Harmony.PatchAll();
                DLog.Log("Harmony patches applied successfully!");
            }
            catch (Exception ex)
            {
                DLog.Log($"Harmony patches failed to apply: {ex}");
            }
        }

        private static GameObject Load;

        public static void UnloadCheat()
        {
            UnityEngine.Object.Destroy(Loader.Load);
            System.GC.Collect();

            var Harmony = new Harmony("dark_cheat");
            Harmony.UnpatchAll();
        }
    }
}
