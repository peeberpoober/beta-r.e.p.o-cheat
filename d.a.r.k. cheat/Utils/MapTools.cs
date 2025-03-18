using System;using UnityEngine;

namespace dark_cheat
{
    public static class MapTools
    {
        public static bool showMapTweaks = false;
        public static bool mapDisableHiddenOverlayCheckboxActive = false;
        public static bool mapDisableHiddenOverlayActive = false;
        public static bool mapCleanModeActive = false;

        public static void ClearMapValuables()
        {
            MapValuable[] valuables = (MapValuable[])UnityEngine.Object.FindObjectsOfType(
                Type.GetType("MapValuable, Assembly-CSharp")
            );

            foreach (MapValuable valuable in valuables)
            {
                UnityEngine.Object.Destroy(valuable.gameObject);
            }
        }

        public static void changeOverlayStatus(bool status)
        {
            if (status == mapDisableHiddenOverlayActive) return;

            MapModule[] modules = UnityEngine.Object.FindObjectsOfType<MapModule>(includeInactive: true);
            // DLog.Log(modules.Length.ToString() + " " + status + mapDisableHiddenOverlayActive + " " + mapDisableHiddenOverlayCheckboxActive);

            if (modules.Length == 0)
            {
                mapDisableHiddenOverlayActive = false;
                return;
            }

            mapDisableHiddenOverlayActive = status;

            foreach (MapModule module in modules)
            {
                GameObject go = module.gameObject;
                go.SetActive(!status);
            }
        }

        public static void DiscoveryMapValuables()
        {
            foreach (object obj in DebugCheats.valuableObjects)
            {
                ValuableObject valuable = obj as ValuableObject;
                if (valuable != null)
                {
                    Map.Instance.AddValuable(valuable);
                }
            }
        }
    }
}

