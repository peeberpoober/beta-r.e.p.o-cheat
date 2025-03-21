using UnityEngine;
using Photon.Pun;
using System.Reflection;
using System;

namespace dark_cheat
{
    public class PlayerCheatSync : MonoBehaviourPunCallbacks
    {
        public PhotonView photonView;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                photonView = gameObject.AddComponent<PhotonView>();
                photonView.ViewID = PhotonNetwork.AllocateViewID(0);  // Pass 0 for local player
                photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
                photonView.ObservedComponents = new System.Collections.Generic.List<Component> { this };
            }
        }

        [PunRPC]
        public void SpawnItemRPC(string itemName, Vector3 position, int value)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ItemSpawner.SpawnItem(itemName, position, value);
                DLog.Log($"Master Client received RPC and spawned {itemName} at {position} with value {value}");
            }
        }

        [PunRPC]
        public void SpawnItemMirrorRPC(string itemName, Vector3 position, int value, int requestingClientId)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                try
                {
                    Vector3 slightlyOffsetPosition = position + new Vector3(0, 0.001f, 0);

                    GameObject spawnedItem = null;
                    string prefabPath = ItemSpawner.GetPrefabPath(itemName);

                    spawnedItem = PhotonNetwork.InstantiateRoomObject(prefabPath, slightlyOffsetPosition, Quaternion.identity, 0, null);

                    if (spawnedItem != null)
                    {
                        var valuableComponent = spawnedItem.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
                        if (valuableComponent != null && value > 0)
                        {
                            var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC",
                                BindingFlags.Public | BindingFlags.Instance);
                            if (dollarValueRPC != null)
                            {
                                dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                            }
                        }

                        spawnedItem.AddComponent<MirroredItemMarker>();

                        photonView.RPC("SetItemVisibilityRPC", PhotonNetwork.LocalPlayer.GetNext(), spawnedItem.GetPhotonView().ViewID, false);

                        DLog.Log($"Host spawned network-synced item for client {requestingClientId}");
                    }
                }
                catch (Exception ex)
                {
                    DLog.LogError($"Error spawning mirror item: {ex.Message}");
                }
            }
        }

        [PunRPC]
        public void SetItemVisibilityRPC(int viewID, bool visible)
        {
            var view = PhotonView.Find(viewID);
            if (view != null)
            {
                foreach (Renderer renderer in view.gameObject.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = visible;
                }
            }
        }

        public class MirroredItemMarker : MonoBehaviour { }
    }
}