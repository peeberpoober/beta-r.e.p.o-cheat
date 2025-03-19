using HarmonyLib;
using UnityEngine;

namespace dark_cheat
{
    public class Loader
    {
        private static Harmony harmonyInstance;
        private static GameObject Load;

        public static void Init()
        {
            Load = new GameObject();
            Load.AddComponent<Hax2>();
            Object.DontDestroyOnLoad(Load);

            harmonyInstance = new Harmony("dark_cheat");
            harmonyInstance.PatchAll();

            DLog.Log("Cheat loader initialized successfully!");
        }

        public static void UnloadCheat()
        {
            Object.Destroy(Load);
            if (harmonyInstance != null)
            {
                // Do not use UnpatchAll, cause it can break mods
                harmonyInstance.UnpatchSelf();
            }

            System.GC.Collect();
        }
    }
}
