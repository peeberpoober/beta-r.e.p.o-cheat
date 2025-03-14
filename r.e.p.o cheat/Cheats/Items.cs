using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;

namespace r.e.p.o_cheat
{
    public static class ItemTeleport
    {
        public class GameItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public object ItemObject { get; set; }

            public GameItem(string name, int value, object itemObject = null)
            {
                Name = name;
                Value = value;
                ItemObject = itemObject;
            }
        }

        public static void SetItemValue(GameItem selectedItem, int newValue)
        {
            if (selectedItem == null || selectedItem.ItemObject == null)
            {
                Hax2.Log1("Error: Selected item or ItemObject is null!");
                return;
            }

            try
            {
                var itemType = selectedItem.ItemObject.GetType();

                var valueField = itemType.GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);


                if (valueField == null)
                {
                    Hax2.Log1($"Error: Could not find 'dollarValueCurrent' field in {selectedItem.Name}");
                    return;
                }

                valueField.SetValue(selectedItem.ItemObject, newValue);
                selectedItem.Value = newValue;

                Hax2.Log1($"Successfully set '{selectedItem.Name}' value to ${newValue}");
            }
            catch (Exception e)
            {
                Hax2.Log1($"Error setting value for '{selectedItem.Name}': {e.Message}");
            }
        }
        private static PhotonView punManagerPhotonView;

        private static void InitializePunManager()
        {
            if (punManagerPhotonView == null)
            {
                var punManagerType = Type.GetType("PunManager, Assembly-CSharp");
                var punManagerInstance = GameHelper.FindObjectOfType(punManagerType);
                if (punManagerInstance != null)
                {
                    punManagerPhotonView = (PhotonView)punManagerType.GetField("photonView", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(punManagerInstance);
                    if (punManagerPhotonView == null) { Hax2.Log1("PhotonView not found in PunManager."); }
                }
                else { Hax2.Log1("PunManager instance not found."); }
            }
        }

        public static List<GameItem> GetItemList()
        {
            List<GameItem> itemList = new List<GameItem>();

            foreach (var valuableObject in DebugCheats.valuableObjects)
            {
                if (valuableObject == null) continue;

                var transformProperty = valuableObject.GetType().GetProperty("transform", BindingFlags.Public | BindingFlags.Instance);
                if (transformProperty == null)
                {
                    Hax2.Log1($"Warning: Object '{valuableObject.GetType().Name}' does not have a 'transform' property. Skipping.");
                    continue;
                }

                var transform = transformProperty.GetValue(valuableObject) as Transform;
                if (transform == null || !transform.gameObject.activeInHierarchy)
                {
                    Hax2.Log1($"Warning: Object '{valuableObject.GetType().Name}' has an inactive or null transform. Skipping.");
                    continue;
                }

                string itemName;
                try
                {
                    itemName = valuableObject.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance)?.GetValue(valuableObject) as string;
                    if (string.IsNullOrEmpty(itemName))
                    {
                        itemName = (valuableObject as UnityEngine.Object)?.name ?? "Unknown";
                    }
                }
                catch (Exception e)
                {
                    itemName = (valuableObject as UnityEngine.Object)?.name ?? "Unknown";
                    Hax2.Log1($"Error accessing 'name' of item: {e.Message}. Using GameObject name: {itemName}");
                }

                if (itemName.StartsWith("Valuable", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = itemName.Substring("Valuable".Length).Trim();
                }
                if (itemName.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = itemName.Substring(0, itemName.Length - "(Clone)".Length).Trim();
                }

                int itemValue = 0;

                // Only check dollarValueCurrent if it's NOT PlayerDeathHead
                if (valuableObject.GetType().Name != "PlayerDeathHead")
                {
                    var valueField = valuableObject.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                    if (valueField != null)
                    {
                        try
                        {
                            itemValue = Convert.ToInt32(valueField.GetValue(valuableObject));
                        }
                        catch (Exception e)
                        {
                            Hax2.Log1($"Error reading 'dollarValueCurrent' for '{itemName}': {e.Message}. Defaulting to 0.");
                        }
                    }
                    else
                    {
                        Hax2.Log1($"Info: '{itemName}' does not have 'dollarValueCurrent', assuming value 0.");
                    }
                }

                // Always add the item to the list, even if it's PlayerDeathHead
                itemList.Add(new GameItem(itemName, itemValue, valuableObject));
            }

            // Ensure there's at least one entry if nothing was found
            if (itemList.Count == 0)
            {
                itemList.Add(new GameItem("No items found", 0));
            }

            return itemList;
        }
        
        public static void TeleportItemToMe(GameItem selectedItem)
        {
            if (selectedItem == null || selectedItem.ItemObject == null)
            {
                Hax2.Log1("Item selecionado ou ItemObject é nulo!");
                return;
            }

            PerformTeleport(selectedItem);
        }

        public static void TeleportAllItemsToMe()
        {
            try
            {
                GameObject player = DebugCheats.GetLocalPlayer();
                if (player == null)
                {
                    Hax2.Log1("Jogador local não encontrado!");
                    return;
                }

                Vector3 targetPosition = player.transform.position + player.transform.forward * 1f + Vector3.up * 1.5f;
                Hax2.Log1($"Target position for teleporting all items: {targetPosition}");

                List<GameItem> itemList = GetItemList();
                int itemsTeleported = 0;

                foreach (var item in itemList)
                {
                    if (item.ItemObject == null) continue;
                    PerformTeleport(item);
                    itemsTeleported++;
                }

                Hax2.Log1($"Teleporte de todos os itens concluído. Total de itens teleportados: {itemsTeleported}");
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao teleportar todos os itens: {e.Message}");
            }
        }

        public static void TeleportSelectedItemToMe(GameItem selectedItem)
        {
            if (selectedItem == null || selectedItem.ItemObject == null)
            {
                Hax2.Log1("Item selecionado ou ItemObject é nulo!");
                return;
            }

            PerformTeleport(selectedItem);
        }
        private static void PerformTeleport(GameItem item)
        {
            try
            {
                GameObject player = DebugCheats.GetLocalPlayer();
                if (player == null)
                {
                    Hax2.Log1("Jogador local não encontrado!");
                    return;
                }

                Vector3 targetPosition = player.transform.position + player.transform.forward * 1f + Vector3.up * 1.5f;
                Hax2.Log1($"Target position for teleport of '{item.Name}': {targetPosition}");

                Transform itemTransform = null;
                var itemObjectType = item.ItemObject.GetType();
                var transformProperty = itemObjectType.GetProperty("transform", BindingFlags.Public | BindingFlags.Instance);
                if (transformProperty != null)
                {
                    itemTransform = transformProperty.GetValue(item.ItemObject) as Transform;
                }
                else
                {
                    var itemMono = item.ItemObject as MonoBehaviour;
                    if (itemMono != null)
                    {
                        itemTransform = itemMono.transform;
                    }
                }

                if (itemTransform == null)
                {
                    Hax2.Log1($"Não foi possível obter o Transform do item '{item.Name}'!");
                    return;
                }

                PhotonView itemPhotonView = itemTransform.GetComponent<PhotonView>();
                if (itemPhotonView == null)
                {
                    Hax2.Log1($"Item '{item.Name}' não tem PhotonView, teleporte apenas local.");
                    itemTransform.position = targetPosition;
                    return;
                }

                if (PhotonNetwork.IsConnected && !itemPhotonView.IsMine)
                {
                    itemPhotonView.RequestOwnership();
                    Hax2.Log1($"Solicitada posse do item '{item.Name}' (ViewID: {itemPhotonView.ViewID})");
                }

                var transformView = itemTransform.GetComponent<PhotonTransformView>();
                bool wasTransformViewActive = false;

                if (transformView != null && transformView.enabled)
                {
                    wasTransformViewActive = true;
                    transformView.enabled = false;
                    Hax2.Log1($"PhotonTransformView desativado temporariamente no item '{item.Name}'");
                }

                Rigidbody rb = itemTransform.GetComponent<Rigidbody>();
                bool wasRbActive = false;
                if (rb != null)
                {
                    wasRbActive = !rb.isKinematic;
                    rb.isKinematic = true;
                    Hax2.Log1($"Rigidbody do item '{item.Name}' desativado temporariamente");
                }

                itemTransform.position = targetPosition;
                Hax2.Log1($"Item '{item.Name}' teleportado localmente para {targetPosition}");
                Vector3 currentPosition = itemTransform.position;
                Hax2.Log1($"Posição atual do item '{item.Name}' após teleporte: {currentPosition}");
                if (PhotonNetwork.IsConnected && itemPhotonView != null)
                {
                    itemPhotonView.RPC("TeleportItemRPC", RpcTarget.AllBuffered, targetPosition);
                    Hax2.Log1($"Enviado RPC 'TeleportItemRPC' para todos para item '{item.Name}'");
                }
                if (wasTransformViewActive || wasRbActive)
                {
                    itemTransform.gameObject.AddComponent<DelayedPhysicsReset>().Setup(rb, transformView);
                }
                var itemGO = itemTransform.gameObject;
                if (itemGO != null)
                {
                    itemGO.SetActive(false);
                    itemGO.SetActive(true);
                    Hax2.Log1($"Item '{item.Name}' reativado para forçar renderização.");
                }

                Hax2.Log1($"Teleporte do item '{item.Name}' concluído.");
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao teleportar item '{item.Name}': {e.Message}");
            }
        }
    }
    public class ItemTeleportComponent : MonoBehaviour, IPunOwnershipCallbacks
    {
        private PhotonView photonView;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                Hax2.Log1($"PhotonView não encontrado no item '{gameObject.name}', adicionando um novo.");
                photonView = gameObject.AddComponent<PhotonView>();
            }
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        [PunRPC]
        private void TeleportItemRPC(Vector3 targetPosition)
        {
            transform.position = targetPosition;
            Hax2.Log1($"Item '{gameObject.name}' sincronizado para {targetPosition} via RPC");
        }

        public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            if (targetView == photonView)
            {
                Hax2.Log1($"Ownership requested for '{gameObject.name}' by player {requestingPlayer.ActorNumber}");
            }
        }

        public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            if (targetView == photonView)
            {
                Hax2.Log1($"Ownership of '{gameObject.name}' transferred from {previousOwner?.ActorNumber} to {targetView.OwnerActorNr}");
            }
        }

        public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            if (targetView == photonView)
            {
                Hax2.Log1($"Ownership transfer failed for '{gameObject.name}' by player {senderOfFailedRequest.ActorNumber}");
            }
        }
    }
    public class DelayedPhysicsReset : MonoBehaviour
    {
        private Rigidbody rb;
        private PhotonTransformView transformView;
        private float delay = 1f;

        public void Setup(Rigidbody rigidbody, PhotonTransformView tView = null)
        {
            rb = rigidbody;
            transformView = tView;
            Invoke(nameof(ResetPhysics), delay);
        }

        private void ResetPhysics()
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                Hax2.Log1($"Physics reativada para '{gameObject.name}' após teleporte");
            }
            if (transformView != null)
            {
                transformView.enabled = true;
                Hax2.Log1($"PhotonTransformView reativado para '{gameObject.name}' após teleporte");
            }
            Destroy(this);
        }
    }
}
