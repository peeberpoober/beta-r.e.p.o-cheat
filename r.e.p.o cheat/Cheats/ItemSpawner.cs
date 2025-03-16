using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace r.e.p.o_cheat
{
    public class ItemSpawner : MonoBehaviourPunCallbacks
    {
        public static void SpawnItem(Vector3 position, int value = 45000)
        {
            try
            {
                DLog.Log($"Spawning item at position: {position} with value: {value}");

                // Setup variables
                bool isMultiplayer = SemiFunc.IsMultiplayer();
                GameObject itemPrefab = AssetManager.instance.surplusValuableSmall;
                GameObject spawnedItem;

                // Prepare value data to pass during instantiation
                object[] itemData = new object[] { value };

                // Step 1: Instantiate the item based on game mode
                if (!isMultiplayer)
                {
                    DLog.Log("Offline mode: Spawning locally.");
                    spawnedItem = Instantiate(itemPrefab, position, Quaternion.identity);
                    ConfigureSyncComponents(spawnedItem);
                    EnsureItemVisibility(spawnedItem);
                }
                else
                {
                    DLog.Log("Multiplayer mode: Spawning via Photon.");

                    var photonNetworkType = typeof(PhotonNetwork);
                    var instantiateMethod = photonNetworkType.GetMethod("NetworkInstantiate", BindingFlags.NonPublic | BindingFlags.Static, null,
                        new Type[] { typeof(InstantiateParameters), typeof(bool), typeof(bool) }, null);

                    if (instantiateMethod == null)
                    {
                        DLog.LogError("NetworkInstantiate method not found");
                        return;
                    }

                    var currentLevelPrefixField = photonNetworkType.GetField("currentLevelPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                    if (currentLevelPrefixField == null)
                    {
                        DLog.LogError("currentLevelPrefix field not found");
                        return;
                    }

                    var currentLevelPrefix = currentLevelPrefixField.GetValue(null);

                    var parameters = new InstantiateParameters(
                        "Valuables/" + itemPrefab.name,
                        position,
                        Quaternion.identity,
                        0,
                        itemData,
                        (byte)currentLevelPrefix,
                        null,
                        PhotonNetwork.LocalPlayer,
                        PhotonNetwork.ServerTimestamp);

                    spawnedItem = (GameObject)instantiateMethod.Invoke(null, new object[] { parameters, true, false });
                    ConfigureSyncComponents(spawnedItem);
                }

                ConfigureValuableObject(spawnedItem, value, isMultiplayer);
                ConfigurePhysicsProperties(spawnedItem, position, isMultiplayer);
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error in SpawnItem: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void ConfigureSyncComponents(GameObject item)
        {
            PhotonView pv = item.GetComponent<PhotonView>() ?? item.AddComponent<PhotonView>();
            pv.ViewID = PhotonNetwork.AllocateViewID(0);
            DLog.Log("PhotonView added to item: " + pv.ViewID);

            PhotonTransformView transformView = item.GetComponent<PhotonTransformView>() ?? item.AddComponent<PhotonTransformView>();
            DLog.Log("PhotonTransformView added to item");

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                PhotonRigidbodyView rigidbodyView = item.GetComponent<PhotonRigidbodyView>() ?? item.AddComponent<PhotonRigidbodyView>();
                rigidbodyView.m_SynchronizeVelocity = true;
                rigidbodyView.m_SynchronizeAngularVelocity = true;
                DLog.Log("PhotonRigidbodyView added and configured on item");
            }

            if (item.GetComponent<ItemSync>() == null)
            {
                item.AddComponent<ItemSync>();
            }

            pv.ObservedComponents = new List<Component> { transformView };
            if (rb != null)
            {
                PhotonRigidbodyView rigidbodyView = item.GetComponent<PhotonRigidbodyView>();
                if (rigidbodyView != null)
                {
                    pv.ObservedComponents.Add(rigidbodyView);
                }
            }
            pv.Synchronization = ViewSynchronization.ReliableDeltaCompressed;
            EnsureItemVisibility(item);
        }

        private static void ConfigureValuableObject(GameObject spawnedItem, int value, bool isMultiplayer)
        {
            var valuableComponent = spawnedItem.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableComponent == null)
            {
                DLog.LogError("ValuableObject component not found");
                return;
            }

            DLog.Log("ValuableObject component found");
            SetFieldValue(valuableComponent, "dollarValueOverride", value);
            SetFieldValue(valuableComponent, "dollarValueOriginal", (float)value);
            SetFieldValue(valuableComponent, "dollarValueCurrent", (float)value);
            SetFieldValue(valuableComponent, "dollarValueSet", true);

            var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
            if (dollarValueRPC != null)
            {
                dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                if (isMultiplayer)
                {
                    var photonView = valuableComponent.GetComponent<PhotonView>();
                    if (photonView != null)
                    {
                        photonView.RequestOwnership();
                        photonView.RPC("DollarValueSetRPC", RpcTarget.Others, (float)value);
                        photonView.RPC("DiscoverRPC", RpcTarget.All);
                        photonView.RPC("AddToDollarHaulListRPC", RpcTarget.All);
                    }
                }
            }
            else
            {
                DLog.LogError("DollarValueSetRPC method not found");
            }
        }

        private static void ConfigurePhysicsProperties(GameObject spawnedItem, Vector3 position, bool isMultiplayer)
        {
            var physComponent = spawnedItem.GetComponent(Type.GetType("PhysGrabObject, Assembly-CSharp"));
            if (physComponent == null)
            {
                DLog.LogError("PhysGrabObject component not found");
                return;
            }

            DLog.Log("PhysGrabObject component found");
            SetFieldValue(physComponent, "spawnTorque", UnityEngine.Random.insideUnitSphere * 0.05f);
            if (isMultiplayer)
            {
                var photonView = spawnedItem.GetComponent<PhotonView>();
                photonView?.RPC("SetPositionRPC", RpcTarget.MasterClient, position, Quaternion.identity);
            }
        }

        private static void SetFieldValue(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                DLog.LogError($"{fieldName} field not found");
            }
        }

        private static void EnsureItemVisibility(GameObject item)
        {
            item.SetActive(true);
            foreach (var renderer in item.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = true;
            }
            item.layer = LayerMask.NameToLayer("Default");
        }
    }
}