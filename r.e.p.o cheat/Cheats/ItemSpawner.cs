using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviourPunCallbacks
{
    public static void SpawnItem(Vector3 position)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Offline mode: Spawning locally.");
            var localItem = Object.Instantiate(AssetManager.instance.surplusValuableSmall, position, Quaternion.identity);
            if (localItem != null)
            {
                EnsureItemVisibility(localItem);
            }
            return;
        }

        object[] instantiationData = new object[] { position.x, position.y, position.z };
        var spawnedItem = PhotonNetwork.Instantiate("Valuables/" + AssetManager.instance.surplusValuableSmall.name, position, Quaternion.identity, 0, instantiationData);

        if (spawnedItem != null)
        {
            Debug.Log("Item spawned at: " + position);
            ConfigureSyncComponents(spawnedItem);
        }
        else
        {
            Debug.LogError("Item spawn failed!");
        }
    }

    private static void ConfigureSyncComponents(GameObject item)
    {
        PhotonView pv = item.GetComponent<PhotonView>();
        if (pv == null)
        {
            pv = item.AddComponent<PhotonView>();
            pv.ViewID = PhotonNetwork.AllocateViewID(0);
            Debug.Log("PhotonView adicionado ao item: " + pv.ViewID);
        }

        PhotonTransformView transformView = item.GetComponent<PhotonTransformView>();
        if (transformView == null)
        {
            transformView = item.AddComponent<PhotonTransformView>();
            Debug.Log("PhotonTransformView adicionado ao item");
        }

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            PhotonRigidbodyView rigidbodyView = item.GetComponent<PhotonRigidbodyView>();
            if (rigidbodyView == null)
            {
                rigidbodyView = item.AddComponent<PhotonRigidbodyView>();
                rigidbodyView.m_SynchronizeVelocity = true;
                rigidbodyView.m_SynchronizeAngularVelocity = true;
                Debug.Log("PhotonRigidbodyView adicionado e configurado no item");
            }
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