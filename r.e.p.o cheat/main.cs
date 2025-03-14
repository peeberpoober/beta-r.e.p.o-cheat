using System;
using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat
{

    public class Loader
    {
        public static void Init()
        {
            Loader.Load = new GameObject();
            Loader.Load.AddComponent<Hax2>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.Load);
        }

        private static GameObject Load;

        public static void UnloadCheat()
        {
            UnityEngine.Object.Destroy(Loader.Load);
            System.GC.Collect();
        }
    }
}
