using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace dark_cheat
{
    public class ItemSpawner : MonoBehaviourPunCallbacks
    {
        private static Dictionary<string, GameObject> itemPrefabCache = new Dictionary<string, GameObject>();
        private static List<string> availableItems = new List<string>();

        public static List<string> GetAvailableItems()
        {
            if (availableItems.Count == 0)
            {
                availableItems.Add("Item Cart Medium");
                availableItems.Add("Item Cart Small");
                availableItems.Add("Item Drone Battery");
                availableItems.Add("Item Drone Feather");
                availableItems.Add("Item Drone Indestructible");
                availableItems.Add("Item Drone Torque");
                availableItems.Add("Item Drone Zero Gravity");
                availableItems.Add("Item Extraction Tracker");
                availableItems.Add("Item Grenade Duct Taped");
                availableItems.Add("Item Grenade Explosive");
                availableItems.Add("Item Grenade Human");
                availableItems.Add("Item Grenade Shockwave");
                availableItems.Add("Item Grenade Stun");
                availableItems.Add("Item Gun Handgun");
                availableItems.Add("Item Gun Shotgun");
                availableItems.Add("Item Gun Tranq");
                availableItems.Add("Item Health Pack Large");
                availableItems.Add("Item Health Pack Medium");
                availableItems.Add("Item Health Pack Small");
                availableItems.Add("Item Melee Baseball Bat");
                availableItems.Add("Item Melee Frying Pan");
                availableItems.Add("Item Melee Inflatable Hammer");
                availableItems.Add("Item Melee Sledge Hammer");
                availableItems.Add("Item Melee Sword");
                availableItems.Add("Item Mine Explosive");
                availableItems.Add("Item Mine Shockwave");
                availableItems.Add("Item Mine Stun");
                availableItems.Add("Item Orb Zero Gravity");
                availableItems.Add("Item Power Crystal");
                availableItems.Add("Item Rubber Duck");
                availableItems.Add("Item Upgrade Map Player Count");
                availableItems.Add("Item Upgrade Player Energy");
                availableItems.Add("Item Upgrade Player Extra Jump");
                availableItems.Add("Item Upgrade Player Grab Range");
                availableItems.Add("Item Upgrade Player Grab Strength");
                availableItems.Add("Item Upgrade Player Health");
                availableItems.Add("Item Upgrade Player Sprint Speed");
                availableItems.Add("Item Upgrade Player Tumble Launch");
                availableItems.Add("Item Valuable Tracker");
                availableItems.Add("Valuable Small");
                availableItems.Add("Valuable Medium");
                availableItems.Add("Valuable Large");
            }
            return availableItems;
        }

        public static void SpawnMoney(Vector3 position, int value = 45000)
        {
            try
            {
                SpawnItem("Valuable Small", position, value);
            }
            catch (Exception ex)
            {
                DLog.Log($"Error spawning money via Valuable Small: {ex.Message}");
                try
                {
                    DLog.Log("Attempting direct money spawn...");
                    CreateMoneyDirectly(position, value);
                }
                catch (Exception ex2)
                {
                    DLog.LogError($"Failed to spawn money via fallback method: {ex2.Message}");
                }
            }
        }

        private static void CreateMoneyDirectly(Vector3 position, int value)
        {
            bool isMultiplayer = SemiFunc.IsMultiplayer();
            GameObject moneyObj = new GameObject("Valuable_Spawned");
            moneyObj.transform.position = position;
            Rigidbody rb = moneyObj.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 0.1f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            BoxCollider collider = moneyObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.2f, 0.2f, 0.2f);
            collider.center = Vector3.zero;
            var physGrabObj = moneyObj.AddComponent(Type.GetType("PhysGrabObject, Assembly-CSharp"));
            var valuableObj = moneyObj.AddComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableObj != null)
            {
                var dollarValueCurrentField = valuableObj.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                var dollarValueOriginalField = valuableObj.GetType().GetField("dollarValueOriginal", BindingFlags.Public | BindingFlags.Instance);
                var dollarValueSetField = valuableObj.GetType().GetField("dollarValueSet", BindingFlags.Public | BindingFlags.Instance);
                if (dollarValueCurrentField != null) dollarValueCurrentField.SetValue(valuableObj, (float)value);
                if (dollarValueOriginalField != null) dollarValueOriginalField.SetValue(valuableObj, (float)value);
                if (dollarValueSetField != null) dollarValueSetField.SetValue(valuableObj, true);
            }
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(moneyObj.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.84f, 0f);
                renderer.material.SetFloat("_Metallic", 1f);
                renderer.material.SetFloat("_Glossiness", 0.8f);
            }
            if (isMultiplayer)
            {
                ConfigureSyncComponents(moneyObj);
            }
            DLog.Log($"Created money object directly with value: {value}");
        }

        public static void SpawnItem(string itemName, Vector3 position, int value = 0)
        {
            try
            {
                DLog.Log($"Spawning item: {itemName} at position: {position}");
                bool isMultiplayer = SemiFunc.IsMultiplayer();

                if (isMultiplayer && !PhotonNetwork.IsMasterClient)
                {
                    SpawnItemNonHost(itemName, position, value);
                    return;
                }

                GameObject itemPrefab = GetItemPrefab(itemName);
                if (itemPrefab == null && itemName.Contains("Valuable"))
                {
                    DLog.Log($"Item prefab not found for: {itemName}, trying direct valuable creation");
                    if (CreateValuableDirectly(itemName, position, value))
                    {
                        return;
                    }
                }
                else if (itemPrefab == null)
                {
                    DLog.LogError($"Item prefab not found for: {itemName}");
                    return;
                }

                GameObject spawnedItem;
                object[] itemData = null;
                if (itemName.Contains("Valuable") && value > 0)
                {
                    itemData = new object[] { value };
                }

                if (!isMultiplayer)
                {
                    DLog.Log("Offline mode: Spawning locally.");
                    spawnedItem = UnityEngine.Object.Instantiate(itemPrefab, position, Quaternion.identity);
                    ConfigureSyncComponents(spawnedItem);
                    EnsureItemVisibility(spawnedItem);
                }
                else
                {
                    DLog.Log("Multiplayer mode: Spawning via Photon.");
                    string prefabPath = GetPrefabPath(itemName);
                    if (string.IsNullOrEmpty(prefabPath))
                    {
                        DLog.LogError($"Could not determine prefab path for: {itemName}");
                        return;
                    }
                    try
                    {
                        spawnedItem = PhotonNetwork.Instantiate(prefabPath, position, Quaternion.identity, 0, itemData);
                    }
                    catch (Exception ex)
                    {
                        DLog.LogError($"Photon instantiate failed: {ex.Message}. Falling back to local instantiate.");
                        spawnedItem = UnityEngine.Object.Instantiate(itemPrefab, position, Quaternion.identity);
                        ConfigureSyncComponents(spawnedItem);
                    }
                }

                if (itemName.Contains("Valuable") && value > 0)
                {
                    ConfigureValuableObject(spawnedItem, value, isMultiplayer);
                }

                ConfigurePhysicsProperties(spawnedItem, position, isMultiplayer);
                DLog.Log($"Successfully spawned {itemName}");
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error in SpawnItem: {ex.Message}\n{ex.StackTrace}");
                if (itemName.Contains("Valuable"))
                {
                    DLog.Log($"Attempting direct creation for valuable after failure");
                    CreateValuableDirectly(itemName, position, value);
                }
            }
        }

        private static void SpawnItemNonHost(string itemName, Vector3 position, int value)
        {
            try
            {
                DLog.Log($"Non-host spawning item: {itemName} at position: {position}");

                GameObject itemPrefab = GetItemPrefab(itemName);
                if (itemPrefab == null)
                {
                    DLog.LogError($"Item prefab not found for: {itemName}");
                    return;
                }

                var photonNetworkType = typeof(PhotonNetwork);

                var instantiateMethod = photonNetworkType.GetMethod("NetworkInstantiate",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new Type[] { typeof(InstantiateParameters), typeof(bool), typeof(bool) },
                    null);

                if (instantiateMethod == null)
                {
                    DLog.LogError("NetworkInstantiate method not found");
                    return;
                }

                var currentLevelPrefixField = photonNetworkType.GetField("currentLevelPrefix",
                    BindingFlags.NonPublic | BindingFlags.Static);

                if (currentLevelPrefixField == null)
                {
                    DLog.LogError("currentLevelPrefix field not found");
                    return;
                }

                var currentLevelPrefix = currentLevelPrefixField.GetValue(null);
                string prefabPath = GetPrefabPath(itemName);

                // Pre-load the resources to ensure they're available
                Resources.Load<GameObject>(prefabPath);

                var parameters = new InstantiateParameters(prefabPath,
                    position,
                    Quaternion.identity,
                    0,
                    null,
                    (byte)currentLevelPrefix,
                    null,
                    PhotonNetwork.LocalPlayer,
                    PhotonNetwork.ServerTimestamp);

                GameObject spawnedItem = (GameObject)instantiateMethod.Invoke(null, new object[] { parameters, true, false });

                if (spawnedItem != null)
                {
                    // Request host to spawn a synced version
                    PlayerCheatSync cheatSync = FindOrCreateCheatSync();
                    if (cheatSync != null && cheatSync.photonView != null)
                    {
                        cheatSync.photonView.RPC("SpawnItemMirrorRPC", RpcTarget.MasterClient,
                            itemName, position, value, PhotonNetwork.LocalPlayer.ActorNumber);
                    }

                    // Force model rebuild
                    foreach (Renderer renderer in spawnedItem.GetComponentsInChildren<Renderer>(true))
                    {
                        if (renderer != null)
                        {
                            // Try to refresh each renderer
                            renderer.enabled = false;
                            renderer.enabled = true;
                        }
                    }

                    if (itemName.Contains("Valuable") && value > 0)
                    {
                        var valuableComponent = spawnedItem.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
                        if (valuableComponent != null)
                        {
                            SetFieldValue(valuableComponent, "dollarValueCurrent", (float)value);
                            SetFieldValue(valuableComponent, "dollarValueOriginal", (float)value);
                            SetFieldValue(valuableComponent, "dollarValueSet", true);

                            var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
                            if (dollarValueRPC != null)
                            {
                                dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                            }
                        }
                    }

                    var physComponent = spawnedItem.GetComponent(Type.GetType("PhysGrabObject, Assembly-CSharp"));
                    if (physComponent != null)
                    {
                        SetFieldValue(physComponent, "spawnTorque", UnityEngine.Random.insideUnitSphere * 0.05f);
                    }

                    // Recreate any missing renderers from prefab
                    CopyRenderersFromPrefab(spawnedItem, itemPrefab);

                    EnsureItemVisibility(spawnedItem);
                    DLog.Log($"Successfully spawned {itemName} as non-host");
                }
                else
                {
                    DLog.LogError("NetworkInstantiate returned null");
                }
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error in SpawnItemNonHost: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void CopyRenderersFromPrefab(GameObject target, GameObject prefab)
        {
            if (target == null || prefab == null) return;

            // Check if target has any renderers
            var targetRenderers = target.GetComponentsInChildren<Renderer>(true);
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                // No renderers, need to copy from prefab
                var prefabRenderers = prefab.GetComponentsInChildren<Renderer>(true);
                foreach (var prefabRenderer in prefabRenderers)
                {
                    // Find the corresponding child object in the target
                    Transform targetChild = FindCorrespondingChild(target.transform, prefabRenderer.transform);
                    if (targetChild == null)
                    {
                        // Create the child if it doesn't exist
                        targetChild = new GameObject(prefabRenderer.name).transform;
                        targetChild.SetParent(target.transform);
                        targetChild.localPosition = prefabRenderer.transform.localPosition;
                        targetChild.localRotation = prefabRenderer.transform.localRotation;
                        targetChild.localScale = prefabRenderer.transform.localScale;
                    }

                    // Add renderer component if missing
                    Renderer targetRenderer = targetChild.GetComponent<Renderer>();
                    if (targetRenderer == null)
                    {
                        // Copy renderer type
                        if (prefabRenderer is MeshRenderer)
                        {
                            targetRenderer = targetChild.gameObject.AddComponent<MeshRenderer>();
                            // Add mesh filter if needed
                            MeshFilter prefabMeshFilter = prefabRenderer.GetComponent<MeshFilter>();
                            if (prefabMeshFilter != null && prefabMeshFilter.sharedMesh != null)
                            {
                                MeshFilter targetMeshFilter = targetChild.GetComponent<MeshFilter>();
                                if (targetMeshFilter == null)
                                {
                                    targetMeshFilter = targetChild.gameObject.AddComponent<MeshFilter>();
                                }
                                targetMeshFilter.sharedMesh = prefabMeshFilter.sharedMesh;
                            }
                        }
                        else if (prefabRenderer is SkinnedMeshRenderer)
                        {
                            SkinnedMeshRenderer smr = targetChild.gameObject.AddComponent<SkinnedMeshRenderer>();
                            SkinnedMeshRenderer prefabSMR = prefabRenderer as SkinnedMeshRenderer;
                            if (prefabSMR.sharedMesh != null)
                            {
                                smr.sharedMesh = prefabSMR.sharedMesh;
                            }
                        }
                    }

                    // Copy materials
                    if (targetRenderer != null && prefabRenderer.sharedMaterials.Length > 0)
                    {
                        targetRenderer.sharedMaterials = prefabRenderer.sharedMaterials;
                    }
                }
            }
        }

        private static Transform FindCorrespondingChild(Transform parent, Transform prefabChild)
        {
            // Try to find by name
            Transform child = parent.Find(prefabChild.name);
            if (child != null) return child;

            // Try to find by hierarchy path
            string path = GetPathRelativeToRoot(prefabChild);
            string[] pathParts = path.Split('/');

            Transform current = parent;
            for (int i = 0; i < pathParts.Length; i++)
            {
                Transform next = current.Find(pathParts[i]);
                if (next == null) return null;
                current = next;
            }

            return current;
        }

        private static string GetPathRelativeToRoot(Transform transform)
        {
            List<string> pathParts = new List<string>();
            Transform current = transform;
            while (current.parent != null)
            {
                pathParts.Add(current.name);
                current = current.parent;
            }

            pathParts.Reverse();
            return string.Join("/", pathParts);
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

        private static PlayerCheatSync FindOrCreateCheatSync()
        {
            PlayerCheatSync cheatSync = UnityEngine.Object.FindObjectOfType<PlayerCheatSync>();
            if (cheatSync == null)
            {
                GameObject cheatSyncObj = new GameObject("CheatSync");
                cheatSync = cheatSyncObj.AddComponent<PlayerCheatSync>();
                UnityEngine.Object.DontDestroyOnLoad(cheatSyncObj);
            }
            return cheatSync;
        }

        private static bool CreateValuableDirectly(string itemName, Vector3 position, int value)
        {
            try
            {
                DLog.Log($"Creating valuable directly: {itemName} with value {value}");
                float sizeMultiplier = 1.0f;
                if (itemName.Contains("Medium")) sizeMultiplier = 1.5f;
                else if (itemName.Contains("Large")) sizeMultiplier = 2.0f;
                GameObject valuableObj = new GameObject(itemName + "_Spawned");
                valuableObj.transform.position = position;
                Rigidbody rb = valuableObj.AddComponent<Rigidbody>();
                rb.mass = 1f * sizeMultiplier;
                rb.drag = 0.1f;
                rb.angularDrag = 0.05f;
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                BoxCollider collider = valuableObj.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.2f, 0.2f, 0.2f) * sizeMultiplier;
                collider.center = Vector3.zero;
                var physGrabObjType = Type.GetType("PhysGrabObject, Assembly-CSharp");
                if (physGrabObjType != null)
                {
                    var physGrabObj = valuableObj.AddComponent(physGrabObjType);
                    var midPointField = physGrabObjType.GetField("midPoint", BindingFlags.Public | BindingFlags.Instance);
                    if (midPointField != null) midPointField.SetValue(physGrabObj, position);
                    var targetPositionField = physGrabObjType.GetField("targetPosition", BindingFlags.Public | BindingFlags.Instance);
                    if (targetPositionField != null) targetPositionField.SetValue(physGrabObj, position);
                }
                var valuableObjType = Type.GetType("ValuableObject, Assembly-CSharp");
                if (valuableObjType != null)
                {
                    var valuableComponent = valuableObj.AddComponent(valuableObjType);
                    var dollarValueCurrentField = valuableObjType.GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                    var dollarValueOriginalField = valuableObjType.GetField("dollarValueOriginal", BindingFlags.Public | BindingFlags.Instance);
                    var dollarValueSetField = valuableObjType.GetField("dollarValueSet", BindingFlags.Public | BindingFlags.Instance);
                    if (dollarValueCurrentField != null) dollarValueCurrentField.SetValue(valuableComponent, (float)value);
                    if (dollarValueOriginalField != null) dollarValueOriginalField.SetValue(valuableComponent, (float)value);
                    if (dollarValueSetField != null) dollarValueSetField.SetValue(valuableComponent, true);
                    var excludeFromExtractionField = valuableObjType.GetField("excludeFromExtraction", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (excludeFromExtractionField != null)
                    {
                        excludeFromExtractionField.SetValue(valuableComponent, true);
                    }
                    var discoveredField = valuableObjType.GetField("discovered", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (discoveredField != null)
                    {
                        discoveredField.SetValue(valuableComponent, true);
                    }
                    var addedToHaulListField = valuableObjType.GetField("addedToDollarHaulList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (addedToHaulListField != null)
                    {
                        addedToHaulListField.SetValue(valuableComponent, true);
                    }
                    var dollarValueRPC = valuableObjType.GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
                    if (dollarValueRPC != null)
                    {
                        dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                    }
                }
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.SetParent(valuableObj.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f) * sizeMultiplier;
                Renderer renderer = visual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(1f, 0.84f, 0f);
                    renderer.material.SetFloat("_Metallic", 1f);
                    renderer.material.SetFloat("_Glossiness", 0.8f);
                }
                if (SemiFunc.IsMultiplayer())
                {
                    ConfigureSyncComponents(valuableObj);
                }
                valuableObj.tag = "SpawnedValuable";
                var mapInstance = typeof(Map).GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (mapInstance != null)
                {
                    valuableObj.AddComponent<ExcludeFromMapTracking>();
                }
                DLog.Log($"Created valuable object directly: {itemName} with value: {value}");
                return true;
            }
            catch (Exception ex)
            {
                DLog.LogError($"Failed to create valuable directly: {ex.Message}");
                return false;
            }
        }

        private class ExcludeFromMapTracking : MonoBehaviour
        {
            void Start()
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        public static string GetPrefabPath(string itemName)
        {
            if (itemName.Contains("Valuable"))
            {
                return "Valuables/" + itemName;
            }
            else if (itemName.StartsWith("Item "))
            {
                return "Items/" + itemName;
            }
            return "Items/" + itemName;
        }

        private static GameObject GetItemPrefab(string itemName)
        {
            if (itemPrefabCache.ContainsKey(itemName) && itemPrefabCache[itemName] != null)
            {
                return itemPrefabCache[itemName];
            }
            GameObject prefab = null;
            if (itemName.Contains("Valuable"))
            {
                if (itemName.Contains("Valuable"))
                {
                    if (itemName == "Valuable Small" && AssetManager.instance.surplusValuableSmall != null)
                    {
                        prefab = AssetManager.instance.surplusValuableSmall;
                    }
                    else
                    {
                        string fieldName = null;
                        if (itemName == "Valuable Small") fieldName = "surplusValuableSmall";
                        else if (itemName == "Valuable Medium") fieldName = "surplusValuableMedium";
                        else if (itemName == "Valuable Large") fieldName = "surplusValuableLarge";
                        if (fieldName != null && AssetManager.instance != null)
                        {
                            var field = AssetManager.instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (field != null)
                            {
                                prefab = field.GetValue(AssetManager.instance) as GameObject;
                            }
                        }
                    }
                    if (prefab == null)
                    {
                        DLog.Log($"Trying to find an existing valuable in the scene for: {itemName}");
                        var valuableObjects = UnityEngine.Object.FindObjectsOfType(Type.GetType("ValuableObject, Assembly-CSharp"));
                        if (valuableObjects != null && valuableObjects.Length > 0)
                        {
                            foreach (var obj in valuableObjects)
                            {
                                GameObject go = (obj as MonoBehaviour)?.gameObject;
                                if (go != null)
                                {
                                    prefab = go;
                                    DLog.Log($"Using existing valuable object as template: {go.name}");
                                    break;
                                }
                            }
                        }
                    }
                    if (prefab == null)
                    {
                        string[] resourcePaths = {
                            "Valuables/Valuable",
                            "Valuables/ValuableSmall",
                            "Valuables/ValuableMedium",
                            "Valuables/ValuableLarge",
                            "Prefabs/Valuable",
                            "Prefabs/ValuableSmall"
                        };
                        foreach (string path in resourcePaths)
                        {
                            prefab = Resources.Load<GameObject>(path);
                            if (prefab != null)
                            {
                                DLog.Log($"Found valuable prefab at Resources path: {path}");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    prefab = Resources.Load<GameObject>("Valuables/" + itemName);
                }
            }
            else
            {
                var statsManager = StatsManager.instance;
                if (statsManager != null)
                {
                    var itemDictionaryField = statsManager.GetType().GetField("itemDictionary", BindingFlags.Public | BindingFlags.Instance);
                    if (itemDictionaryField != null)
                    {
                        var itemDictionary = itemDictionaryField.GetValue(statsManager) as Dictionary<string, Item>;
                        if (itemDictionary != null && itemDictionary.ContainsKey(itemName))
                        {
                            var item = itemDictionary[itemName];
                            if (item != null && item.prefab != null)
                            {
                                prefab = item.prefab;
                            }
                        }
                    }
                }
                if (prefab == null)
                {
                    prefab = Resources.Load<GameObject>("Items/" + itemName);
                }
            }
            if (prefab != null)
            {
                itemPrefabCache[itemName] = prefab;
            }
            else
            {
                DLog.LogError($"Could not find prefab for item: {itemName}");
            }
            return prefab;
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
            SetFieldValue(valuableComponent, "excludeFromExtraction", true);
            var dollarHaulListField = valuableComponent.GetType().GetField("addedToDollarHaulList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (dollarHaulListField != null)
            {
                dollarHaulListField.SetValue(valuableComponent, true);
            }
            var discoveredField = valuableComponent.GetType().GetField("discovered", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (discoveredField != null)
            {
                discoveredField.SetValue(valuableComponent, true);
            }
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
                    }
                }
            }
            else
            {
                DLog.LogError("DollarValueSetRPC method not found");
            }
            try
            {
                var statsManager = StatsManager.instance;
                if (statsManager != null)
                {
                    string valuableIdentifier = "";
                    var instanceNameField = spawnedItem.GetComponent(Type.GetType("ItemAttributes, Assembly-CSharp"))?.GetType().GetField("instanceName");
                    if (instanceNameField != null)
                    {
                        valuableIdentifier = instanceNameField.GetValue(spawnedItem.GetComponent(Type.GetType("ItemAttributes, Assembly-CSharp"))) as string;
                    }
                    if (!string.IsNullOrEmpty(valuableIdentifier))
                    {
                        var removeValuableMethod = statsManager.GetType().GetMethod("RemoveValuableFromHaul", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (removeValuableMethod != null)
                        {
                            removeValuableMethod.Invoke(statsManager, new object[] { valuableIdentifier });
                            DLog.Log($"Removed valuable {valuableIdentifier} from extraction goal tracking");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DLog.Log($"Error trying to remove valuable from tracking: {ex.Message}");
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
