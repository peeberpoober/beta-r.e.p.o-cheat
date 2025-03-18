using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public static class CursorController
{
    public static bool cheatMenuOpen = false;
    public static bool overrideCursorSetting = false;
    internal static bool currentlySettingCursor = false;
    internal static object lastLockState;
    internal static bool lastCursorVisible;
    private static object waitForEndOfFrame;

    private static Type cursorType;
    private static Type monoBehaviourType;
    private static Type gameObjectType;
    private static Type waitForEndOfFrameType;
    private static Type debugType;

    private static PropertyInfo lockStateProperty;
    private static PropertyInfo visibleProperty;
    private static MethodInfo dontDestroyOnLoadMethod;
    private static MethodInfo startCoroutineMethod;
    private static MethodInfo logErrorMethod;

    public static void Init()
    {
        try
        {
            Assembly unityEngineAssembly = Assembly.Load("UnityEngine.CoreModule");
            cursorType = unityEngineAssembly.GetType("UnityEngine.Cursor");
            gameObjectType = unityEngineAssembly.GetType("UnityEngine.GameObject");
            monoBehaviourType = unityEngineAssembly.GetType("UnityEngine.MonoBehaviour");
            debugType = unityEngineAssembly.GetType("UnityEngine.Debug");

            waitForEndOfFrameType = Assembly.Load("UnityEngine.CoreModule").GetType("UnityEngine.WaitForEndOfFrame");
            waitForEndOfFrame = Activator.CreateInstance(waitForEndOfFrameType);

            lockStateProperty = cursorType.GetProperty("lockState");
            visibleProperty = cursorType.GetProperty("visible");
            dontDestroyOnLoadMethod = unityEngineAssembly.GetType("UnityEngine.Object").GetMethod("DontDestroyOnLoad");
            logErrorMethod = debugType.GetMethod("LogError", new Type[] { typeof(object) });

            lastLockState = lockStateProperty.GetValue(null);
            lastCursorVisible = (bool)visibleProperty.GetValue(null);

            object cursorManagerGameObject = Activator.CreateInstance(gameObjectType, new object[] { "CursorManager" });
            
            MethodInfo addComponentMethod = gameObjectType.GetMethod("AddComponent", new Type[] { typeof(Type) });
            
            object cursorManager = addComponentMethod.Invoke(cursorManagerGameObject, new object[] { monoBehaviourType });
            
            dontDestroyOnLoadMethod.Invoke(null, new object[] { cursorManagerGameObject });
            
            Type iEnumeratorType = typeof(IEnumerator);
            startCoroutineMethod = monoBehaviourType.GetMethod("StartCoroutine", new Type[] { iEnumeratorType });
            
            startCoroutineMethod.Invoke(cursorManager, new object[] { UnlockCoroutine() });

            var harmony = new Harmony("com.mycompany.mycheat.cursorcontroller");
            harmony.PatchAll();
        }
        catch (Exception ex)
        {
            Console.WriteLine("CursorController Init error: " + ex.ToString());
        }
    }

    public static void SetMenuOpen(bool open)
    {
        cheatMenuOpen = open;
        UpdateCursorState();
    }

    public static void UpdateCursorState()
    {
        try
        {
            currentlySettingCursor = true;
            if (cheatMenuOpen)
            {
                Type cursorLockModeType = Assembly.Load("UnityEngine.CoreModule").GetType("UnityEngine.CursorLockMode");
                object noneLockMode = Enum.Parse(cursorLockModeType, "None");
                
                lockStateProperty.SetValue(null, noneLockMode);
                visibleProperty.SetValue(null, true);
            }
            else
            {
                lockStateProperty.SetValue(null, lastLockState);
                visibleProperty.SetValue(null, lastCursorVisible);
            }
            currentlySettingCursor = false;
        }
        catch (Exception ex)
        {
            if (logErrorMethod != null)
            {
                logErrorMethod.Invoke(null, new object[] { "CursorController.UpdateCursorState error: " + ex.ToString() });
            }
            else
            {
                Console.WriteLine("CursorController.UpdateCursorState error: " + ex.ToString());
            }
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
}

[HarmonyPatch]
public class CursorPatches
{
    [HarmonyPatch]
    public static bool CursorLockStatePrepare(MethodBase original, ref HarmonyMethod prefix)
    {
        try
        {
            Type cursorType = Assembly.Load("UnityEngine.CoreModule").GetType("UnityEngine.Cursor");
            if (original.DeclaringType == cursorType && original.Name == "set_lockState")
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
        return false;
    }
    
    [HarmonyPrefix]
    public static void CursorLockStatePrefix(ref object __0)
    {
        if (!CursorController.currentlySettingCursor)
        {
            CursorController.lastLockState = __0;
            if (CursorController.cheatMenuOpen)
            {
                Type cursorLockModeType = Assembly.Load("UnityEngine.CoreModule").GetType("UnityEngine.CursorLockMode");
                __0 = Enum.Parse(cursorLockModeType, "None");
            }
        }
    }
    
    [HarmonyPatch]
    public static bool CursorVisiblePrepare(MethodBase original, ref HarmonyMethod prefix)
    {
        try
        {
            Type cursorType = Assembly.Load("UnityEngine.CoreModule").GetType("UnityEngine.Cursor");
            if (original.DeclaringType == cursorType && original.Name == "set_visible")
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
        return false;
    }
    
    [HarmonyPrefix]
    public static void CursorVisiblePrefix(ref bool __0)
    {
        if (!CursorController.currentlySettingCursor)
        {
            CursorController.lastCursorVisible = __0;
            if (CursorController.cheatMenuOpen)
            {
                __0 = true;
            }
        }
    }
}
