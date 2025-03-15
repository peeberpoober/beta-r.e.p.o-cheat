public static void SpawnItem(Vector3 position, int value = 45000)
    {
        try
        {
            Debug.Log($"Spawning item at position: {position} with value: {value}");

            // Setup variables
            bool isMultiplayer = SemiFunc.IsMultiplayer();
            GameObject itemPrefab = AssetManager.instance.surplusValuableSmall;
            GameObject spawnedItem;

            // Prepare value data to pass during instantiation
            object[] itemData = new object[] { value };

            // Step 1: Instantiate the item based on game mode
            if (!isMultiplayer)
            {
                Debug.Log("Offline mode: Spawning locally.");
                spawnedItem = UnityEngine.Object.Instantiate<GameObject>(itemPrefab, position, Quaternion.identity);
                ConfigureSyncComponents(spawnedItem);
                EnsureItemVisibility(spawnedItem);
            }
            else
            {
                Debug.Log("Multiplayer mode: Spawning via Photon.");

                // Get the PhotonNetwork type as it is a static class
                var photonNetworkType = typeof(PhotonNetwork);

                // Get the NetworkInstantiate method which is internal and static
                // Specify the exact parameter types to avoid ambiguity
                var instantiateMethod = photonNetworkType.GetMethod("NetworkInstantiate",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new Type[] { typeof(InstantiateParameters), typeof(bool), typeof(bool) },
                    null);

                if (instantiateMethod == null)
                {
                    Debug.LogError("NetworkInstantiate method not found");
                    return;
                }

                // Get the value of the currentLevelPrefix field
                var currentLevelPrefixField = photonNetworkType.GetField("currentLevelPrefix",
                    BindingFlags.NonPublic | BindingFlags.Static);

                if (currentLevelPrefixField == null)
                {
                    Debug.LogError("currentLevelPrefix field not found");
                    return;
                }

                var currentLevelPrefix = currentLevelPrefixField.GetValue(null);

                // Create the instantiation parameters - PASSING ITEM DATA WITH VALUE
                var parameters = new InstantiateParameters("Valuables/" + itemPrefab.name,
                    position,
                    Quaternion.identity,
                    0,
                    itemData, // Pass the value as instantiation data
                    (byte)currentLevelPrefix,
                    null,
                    PhotonNetwork.LocalPlayer,
                    PhotonNetwork.ServerTimestamp);

                // Invoke the method (pass null as the first parameter because it is a static method)
                spawnedItem = (GameObject)instantiateMethod.Invoke(null, new object[] { parameters, true, false });

                // Configure sync components
                ConfigureSyncComponents(spawnedItem);
            }

            // Step 2: Configure the valuable object component
            // Since we're now passing the value via InstantiationData, we need to ensure
            // it's properly set in ValuableObject even in singleplayer mode or as a fallback
            var valuableComponent = spawnedItem.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableComponent != null)
            {
                Debug.Log("ValuableObject component found");

                // Set dollar values directly using reflection
                var dollarValueOverride = valuableComponent.GetType().GetField("dollarValueOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                if (dollarValueOverride != null)
                    dollarValueOverride.SetValue(valuableComponent, value);

                var dollarValueOriginal = valuableComponent.GetType().GetField("dollarValueOriginal", BindingFlags.Public | BindingFlags.Instance);
                if (dollarValueOriginal != null)
                    dollarValueOriginal.SetValue(valuableComponent, (float)value);

                var dollarValueCurrent = valuableComponent.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                if (dollarValueCurrent != null)
                    dollarValueCurrent.SetValue(valuableComponent, (float)value);

                var dollarValueSet = valuableComponent.GetType().GetField("dollarValueSet", BindingFlags.NonPublic | BindingFlags.Instance);
                if (dollarValueSet != null)
                    dollarValueSet.SetValue(valuableComponent, true);

                // Use RPC method for network synchronization
                var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
                if (dollarValueRPC != null)
                {
                    dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });

                    // Synchronize with network clients if in multiplayer
                    if (isMultiplayer)
                    {
                        var photonView = valuableComponent.GetComponent<PhotonView>();
                        if (photonView != null)
                        {
                            photonView.RPC("DollarValueSetRPC", RpcTarget.Others, (float)value);
                            photonView.RPC("DiscoverRPC", RpcTarget.All, Array.Empty<object>());
                            photonView.RPC("AddToDollarHaulListRPC", RpcTarget.All, Array.Empty<object>());
                        }
                    }
                }
                else
                {
                    Debug.LogError("DollarValueSetRPC method not found");
                }
            }
            else
            {
                Debug.LogError("ValuableObject component not found");
            }

            // Step 3: Configure physics properties
            var physComponent = spawnedItem.GetComponent(Type.GetType("PhysGrabObject, Assembly-CSharp"));
            if (physComponent != null)
            {
                Debug.Log("PhysGrabObject component found");

                // Set random torque
                FieldInfo spawnTorqueField = physComponent.GetType().GetField("spawnTorque",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                if (spawnTorqueField != null)
                {
                    Vector3 randomTorque = UnityEngine.Random.insideUnitSphere * 0.05f;
                    spawnTorqueField.SetValue(physComponent, randomTorque);
                    Debug.Log($"Set spawnTorque to {randomTorque}");

                    // Sync position in multiplayer
                    if (isMultiplayer)
                    {
                        var photonView = spawnedItem.GetComponent<PhotonView>();
                        if (photonView != null)
                        {
                            photonView.RPC("SetPositionRPC", RpcTarget.MasterClient,
                                new object[] { position, Quaternion.identity });
                        }
                    }
                }
                else
                {
                    Debug.LogError("spawnTorque field not found");
                }
            }
            else
            {
                Debug.LogError("PhysGrabObject component not found");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in SpawnItem: {ex.Message}\n{ex.StackTrace}");
        }
    }
