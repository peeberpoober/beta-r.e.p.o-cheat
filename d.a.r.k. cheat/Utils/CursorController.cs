/* // commented out, not using harmony.
using HarmonyLib;
using System.Collections;
using UnityEngine;
public static class CursorController
{
    public static bool cheatMenuOpen = false;
    public static bool overrideCursorSetting = false;
    private static bool currentlySettingCursor = false;
    private static CursorLockMode lastLockState = Cursor.lockState;
    private static bool lastCursorVisible = Cursor.visible;
    private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    public static void Init()
    {
        var harmony = new Harmony("com.mycompany.mycheat.cursorcontroller");
        harmony.PatchAll();
    }
    public static void UpdateCursorState()
    {
        try
        {
            currentlySettingCursor = true;
            if (cheatMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = lastLockState;
                Cursor.visible = lastCursorVisible;
            }
            currentlySettingCursor = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CursorController.UpdateCursorState error: " + ex);
        }
    }
    public static IEnumerator UnlockCoroutine()
    {
        while (true)
        {
            yield return waitForEndOfFrame;
            UpdateCursorState();
        }
    }
    [HarmonyPatch(typeof(Cursor), "set_lockState")]
    public class SetLockStatePatch
    {
        static void Prefix(ref CursorLockMode value)
        {
            if (!currentlySettingCursor)
            {
                lastLockState = value;
                if (cheatMenuOpen)
                {
                    value = CursorLockMode.None;
                }
            }
        }
    }
    [HarmonyPatch(typeof(Cursor), "set_visible")]
    public class SetVisiblePatch
    {
        static void Prefix(ref bool value)
        {
            if (!currentlySettingCursor)
            {
                lastCursorVisible = value;
                if (cheatMenuOpen)
                {
                    value = true;
                }
            }
        }
    }
}
*/
