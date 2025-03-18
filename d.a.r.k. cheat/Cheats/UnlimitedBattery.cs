using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace dark_cheat
{
    public class UnlimitedBattery : MonoBehaviour
    {
        public bool unlimitedBatteryEnabled = false;
        private float updateInterval = 2f;
        private List<ItemBattery> batteries = new List<ItemBattery>();
        private float nextScanTime = 0f;
        private const float SCAN_INTERVAL = 2f;
        private Dictionary<System.Type, FieldInfo> batteryLifeIntCache = new Dictionary<System.Type, FieldInfo>();
        private Dictionary<System.Type, FieldInfo> batteryDrainRateCache = new Dictionary<System.Type, FieldInfo>();
        private Dictionary<System.Type, FieldInfo> drainTimerCache = new Dictionary<System.Type, FieldInfo>();
        private Dictionary<System.Type, MethodInfo> batteryFullPercentChangeCache = new Dictionary<System.Type, MethodInfo>();

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(BatteryUpdateCoroutine());
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        bool IsLocalPlayerHolding(ItemBattery battery)
        {
            if (battery == null) return false;

            var physGrabObject = battery.GetComponent<PhysGrabObject>();
            if (physGrabObject == null) return false;

            if (physGrabObject.playerGrabbing != null && physGrabObject.playerGrabbing.Count > 0)
            {
                foreach (var grabber in physGrabObject.playerGrabbing)
                {
                    if (grabber != null && grabber.isLocal)
                        return true;
                }
            }

            var equippable = battery.GetComponent<ItemEquippable>();
            if (equippable != null)
            {
                var isEquippedField = equippable.GetType().GetField("isEquipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (isEquippedField != null && (bool)isEquippedField.GetValue(equippable))
                {
                    var playerAvatarLocal = SemiFunc.PlayerAvatarLocal();
                    if (playerAvatarLocal != null)
                    {
                        var itemSlots = playerAvatarLocal.GetType().GetField("itemSlots", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (itemSlots != null)
                        {
                            var slots = itemSlots.GetValue(playerAvatarLocal) as System.Collections.IEnumerable;
                            if (slots != null)
                            {
                                foreach (var slot in slots)
                                {
                                    var itemField = slot.GetType().GetField("item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (itemField != null)
                                    {
                                        var item = itemField.GetValue(slot) as GameObject;
                                        if (item != null && item == battery.gameObject)
                                            return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        void UpdateBatteryCache()
        {
            if (Time.time >= nextScanTime)
            {
                batteries.RemoveAll(b => b == null);
                ItemBattery[] newBatteries = FindObjectsOfType<ItemBattery>();
                foreach (ItemBattery battery in newBatteries)
                {
                    if (!batteries.Contains(battery) && battery != null)
                        batteries.Add(battery);
                }
                nextScanTime = Time.time + SCAN_INTERVAL;
            }
        }

        IEnumerator BatteryUpdateCoroutine()
        {
            yield return new WaitForSeconds(1f);

            while (true)
            {
                if (unlimitedBatteryEnabled)
                {
                    UpdateBatteryCache();

                    for (int i = 0; i < batteries.Count; i++)
                    {
                        ItemBattery battery = batteries[i];
                        if (battery == null) continue;

                        if (IsLocalPlayerHolding(battery))
                        {
                            battery.batteryLife = 100f;
                            System.Type t = battery.GetType();

                            FieldInfo batteryLifeIntField;
                            if (!batteryLifeIntCache.TryGetValue(t, out batteryLifeIntField))
                            {
                                batteryLifeIntField = t.GetField("batteryLifeInt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                batteryLifeIntCache[t] = batteryLifeIntField;
                            }

                            if (batteryLifeIntField != null)
                            {
                                int currentLifeInt = (int)batteryLifeIntField.GetValue(battery);
                                if (currentLifeInt < 6)
                                {
                                    batteryLifeIntField.SetValue(battery, 6);
                                }
                            }

                            FieldInfo drainRateField;
                            if (!batteryDrainRateCache.TryGetValue(t, out drainRateField))
                            {
                                drainRateField = t.GetField("batteryDrainRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                batteryDrainRateCache[t] = drainRateField;
                            }

                            if (drainRateField != null)
                                drainRateField.SetValue(battery, 0f);

                            FieldInfo drainTimerField;
                            if (!drainTimerCache.TryGetValue(t, out drainTimerField))
                            {
                                drainTimerField = t.GetField("drainTimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                drainTimerCache[t] = drainTimerField;
                            }

                            if (drainTimerField != null)
                                drainTimerField.SetValue(battery, 0f);

                            MethodInfo updateVisuals;
                            if (!batteryFullPercentChangeCache.TryGetValue(t, out updateVisuals))
                            {
                                updateVisuals = t.GetMethod("BatteryFullPercentChange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                batteryFullPercentChangeCache[t] = updateVisuals;
                            }

                            if (updateVisuals != null)
                            {
                                int currentLifeInt = batteryLifeIntField != null ? (int)batteryLifeIntField.GetValue(battery) : 0;
                                if (currentLifeInt < 6)
                                {
                                    updateVisuals.Invoke(battery, new object[] { 6, true });
                                }
                            }
                        }

                        if ((i + 1) % 5 == 0)
                            yield return null;
                    }
                }

                yield return new WaitForSeconds(updateInterval);
            }
        }
    }
}