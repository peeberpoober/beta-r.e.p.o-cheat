using UnityEngine;

namespace r.e.p.o_cheat
{
    class MiscFeatures
    {
        private static float previousFarClip = 0f;
        public static bool NoFogEnabled = false;

        public static void ToggleNoFog(bool enable)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                DLog.LogError("Camera.main not found!");
                return;
            }

            if (enable)
            {
                if (previousFarClip == 0f)
                    previousFarClip = cam.farClipPlane;

                cam.farClipPlane = 500f;
                RenderSettings.fog = false;
                NoFogEnabled = true;
                DLog.Log("NoFog enabled");
            }
            else
            {
                if (previousFarClip > 0f)
                    cam.farClipPlane = previousFarClip;
                RenderSettings.fog = true;
                NoFogEnabled = false;
                DLog.Log("NoFog disabled");
            }
        }
    }
}
