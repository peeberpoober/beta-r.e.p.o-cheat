using dark_cheat;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace dark_cheat
{
    public class Loader
    {
        public static void Init()
        {
            Loader.Load = new GameObject();
            Loader.Load.AddComponent<Hax2>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.Load);

            // Previous Harmony patching code removed
            DLog.Log("Cheat loader initialized successfully!");
        }

        private static GameObject Load;

        public static void UnloadCheat()
        {
            UnityEngine.Object.Destroy(Loader.Load);
            System.GC.Collect();

            // Previous Harmony unpatching code removed
        }
    }
}
