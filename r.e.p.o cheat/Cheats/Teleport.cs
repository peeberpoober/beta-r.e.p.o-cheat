using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Reflection;
using System.Collections;

namespace r.e.p.o_cheat
{
    public static class Teleport
    {
        public static void TeleportPlayerToMe(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                DLog.Log("Invalid player index!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                DLog.Log("Selected player is null!");
                return;
            }

            try
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                {
                    DLog.Log("Local player not found!");
                    return;
                }

                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    DLog.Log("PhotonViewField not found on selected player!");
                    return;
                }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null)
                {
                    DLog.Log("PhotonView is not valid!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    DLog.Log("selectedPlayer is not a MonoBehaviour!");
                    return;
                }

                var transform = playerMono.transform;
                if (transform == null)
                {
                    DLog.Log("Transform of selected player is null!");
                    return;
                }

                Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                transform.position = targetPosition;
                DLog.Log($"Player {playerNames[selectedPlayerIndex]} locally teleported to {targetPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, transform.rotation });
                    DLog.Log($"RPC 'SpawnRPC' sent to all with position: {targetPosition}");
                }
                else
                {
                    DLog.Log("Not connected to Photon, local teleport only.");
                }
            }
            catch (Exception e)
            {
                DLog.Log($"Error teleporting {playerNames[selectedPlayerIndex]} to you: {e.Message}");
            }
        }

        public static void TeleportMeToPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                DLog.Log("Invalid player index!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                DLog.Log("Selected player is null!");
                return;
            }

            try
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                {
                    DLog.Log("Local player not found!");
                    return;
                }

                var localPhotonViewField = localPlayer.GetComponent<PhotonView>();
                if (localPhotonViewField == null)
                {
                    DLog.Log("PhotonViewField not found on local player!");
                    return;
                }
                var localPhotonView = localPhotonViewField;
                if (localPhotonView == null)
                {
                    DLog.Log("Local PhotonView is not valid!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    DLog.Log("selectedPlayer is not a MonoBehaviour!");
                    return;
                }

                var targetTransform = playerMono.transform;
                if (targetTransform == null)
                {
                    DLog.Log("Transform of selected player is null!");
                    return;
                }

                Vector3 targetPosition = targetTransform.position + Vector3.up * 1.5f;
                localPlayer.transform.position = targetPosition;
                DLog.Log($"You were locally teleported to {playerNames[selectedPlayerIndex]} at {targetPosition}");

                if (PhotonNetwork.IsConnected && localPhotonView != null)
                {
                    localPhotonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, localPlayer.transform.rotation });
                    DLog.Log($"RPC 'SpawnRPC' sent to all with position: {targetPosition}");
                }
                else
                {
                    DLog.Log("Not connected to Photon, local teleport only.");
                }
            }
            catch (Exception e)
            {
                DLog.Log($"Error teleporting you to {playerNames[selectedPlayerIndex]}: {e.Message}");
            }
        }

        public static void SendSelectedPlayerToVoid(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                DLog.Log("Invalid player index!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                DLog.Log("Selected player is null!");
                return;
            }

            try
            {
                DLog.Log($"Attempting to send {playerNames[selectedPlayerIndex]} to the void | MasterClient: {PhotonNetwork.IsMasterClient}");

                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    DLog.Log("PhotonViewField not found!");
                    return;
                }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null)
                {
                    DLog.Log("PhotonView is not valid!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    DLog.Log("selectedPlayer is not a MonoBehaviour!");
                    return;
                }

                var transform = playerMono.transform;
                if (transform == null)
                {
                    DLog.Log("Transform is null!");
                    return;
                }

                Vector3 voidPosition = new Vector3(0, -10, 0);
                transform.position = voidPosition;
                DLog.Log($"Player {playerNames[selectedPlayerIndex]} sent locally to the void: {voidPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { voidPosition, transform.rotation });
                    DLog.Log($"RPC 'SpawnRPC' sent to all with position: {voidPosition}");
                }
                else
                {
                    DLog.Log("Not connected to Photon, teleport is only local.");
                }
            }
            catch (Exception e)
            {
                DLog.Log($"Error sending {playerNames[selectedPlayerIndex]} to the void: {e.Message}");
            }
        }
        public static void ExecuteTeleportWithIndices(int sourceIndex, int destIndex, string[] teleportOptions, List<object> playerList)
        {
            try
            {
                if (sourceIndex == destIndex) // Don't teleport if source and destination are the same
                {
                    DLog.Log("Source and destination are the same. Teleport canceled.");
                    return;
                }
                object sourceObject = null; // Determine source object
                string sourceName = teleportOptions[sourceIndex];
                int playerIndex = sourceIndex; // Player index in the list
                if (sourceIndex < teleportOptions.Length - 1) // If it's not the void option (which would be the last option)
                {
                    if (playerIndex >= 0 && playerIndex < playerList.Count)
                    {
                        sourceObject = playerList[playerIndex];
                    }
                }
                if (sourceObject == null)
                {
                    DLog.Log("Invalid source for teleport!");
                    return;
                }
                Vector3 destPosition; // Determine destination position
                string destName = teleportOptions[destIndex];
                if (destIndex == teleportOptions.Length - 1) // The Void
                {
                    destPosition = new Vector3(0, -10, 0);
                    destName = "The Void";
                }
                else // A player
                {
                    int destPlayerIndex = destIndex;
                    if (destPlayerIndex < 0 || destPlayerIndex >= playerList.Count)
                    {
                        DLog.Log("Invalid destination player index!");
                        return;
                    }
                    var destPlayer = playerList[destPlayerIndex] as MonoBehaviour;
                    if (destPlayer == null)
                    {
                        DLog.Log("Destination player is not a MonoBehaviour!");
                        return;
                    }
                    destPosition = destPlayer.transform.position + Vector3.up * 1.5f;
                }

                var playerObj = sourceObject; // Execute the teleport based on source type
                bool isLocalPlayer = false;
                if (playerObj is GameObject playerGameObj) // If player is a GameObject, compare directly with local player
                {
                    isLocalPlayer = playerGameObj == DebugCheats.GetLocalPlayer();
                }
                else if (playerObj is MonoBehaviour playerMono) // If player is a MonoBehaviour, compare its gameObject
                {
                    isLocalPlayer = playerMono.gameObject == DebugCheats.GetLocalPlayer();
                }
                if (isLocalPlayer) // Local player teleporting
                {
                    GameObject localPlayer = DebugCheats.GetLocalPlayer();
                    var localPhotonView = localPlayer.GetComponent<PhotonView>();
                    localPlayer.transform.position = destPosition;
                    DLog.Log($"You were locally teleported to {destName} at {destPosition}");
                    if (PhotonNetwork.IsConnected && localPhotonView != null)
                    {
                        localPhotonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { destPosition, localPlayer.transform.rotation });
                        DLog.Log($"RPC 'SpawnRPC' sent to all with position: {destPosition}");
                    }
                    else
                    {
                        DLog.Log("Not connected to Photon, local teleport only.");
                    }
                }
                else // Other player teleporting
                {
                    var photonViewField = playerObj.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (photonViewField == null)
                    {
                        DLog.Log("PhotonViewField not found on source player!");
                        return;
                    }
                    var photonView = photonViewField.GetValue(playerObj) as PhotonView;
                    if (photonView == null)
                    {
                        DLog.Log("PhotonView is not valid!");
                        return;
                    }
                    var playerMono = playerObj as MonoBehaviour;
                    if (playerMono == null)
                    {
                        DLog.Log("Source player is not a MonoBehaviour!");
                        return;
                    }
                    var transform = playerMono.transform;
                    if (transform == null)
                    {
                        DLog.Log("Transform of source player is null!");
                        return;
                    }
                    transform.position = destPosition;
                    DLog.Log($"{sourceName} locally teleported to {destName} at {destPosition}");
                    if (PhotonNetwork.IsConnected && photonView != null)
                    {
                        photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { destPosition, transform.rotation });
                        DLog.Log($"RPC 'SpawnRPC' sent to all with position: {destPosition}");
                    }
                    else
                    {
                        DLog.Log("Not connected to Photon, local teleport only.");
                    }
                }
            }
            catch (Exception e)
            {
                DLog.Log($"Error during teleport: {e.Message}");
            }
        }
        public static void ExecuteTeleportWithSeparateOptions(
            int sourceIndex,
            int destIndex,
            string[] sourceOptions,
            string[] destOptions,
            List<object> playerList)
        {
            try
            {
                if (sourceIndex < 0 || sourceIndex >= sourceOptions.Length) // Validate indices
                {
                    DLog.Log("Invalid source player index!");
                    return;
                }

                if (destIndex < 0 || destIndex >= destOptions.Length)
                {
                    DLog.Log("Invalid destination index!");
                    return;
                }
                object sourceObject = null; // Get source player (which can only be a player)
                string sourceName = sourceOptions[sourceIndex];
                if (sourceIndex < playerList.Count) // Source is always a player from playerList
                {
                    sourceObject = playerList[sourceIndex];
                }

                if (sourceObject == null)
                {
                    DLog.Log("Invalid source player for teleport!");
                    return;
                }
                Vector3 destPosition; // Determine destination position
                string destName = destOptions[destIndex];
                if (destIndex == destOptions.Length - 1) // The Void (last option)
                {
                    destPosition = new Vector3(0, -10, 0);
                    destName = "The Void";
                }
                else // A player
                {
                    int destPlayerIndex = destIndex;
                    if (destPlayerIndex < 0 || destPlayerIndex >= playerList.Count)
                    {
                        DLog.Log("Invalid destination player index!");
                        return;
                    }
                    var destPlayer = playerList[destPlayerIndex] as MonoBehaviour;
                    if (destPlayer == null)
                    {
                        DLog.Log("Destination player is not a MonoBehaviour!");
                        return;
                    }
                    destPosition = destPlayer.transform.position + Vector3.up * 1.5f;
                }
                var playerObj = sourceObject; // Execute the teleport based on source type
                GameObject localPlayer = DebugCheats.GetLocalPlayer(); // Check if this is the local player
                bool isLocalPlayer = false;
                if (playerObj is GameObject playerGameObj) // If player is a GameObject, compare directly
                {
                    isLocalPlayer = playerGameObj == localPlayer;
                }
                else if (playerObj is MonoBehaviour playerMono) // If player is a MonoBehaviour, compare its gameObject
                {
                    isLocalPlayer = playerMono.gameObject == localPlayer;
                }
                if (isLocalPlayer) // Local player teleporting
                {
                    var localPhotonView = localPlayer.GetComponent<PhotonView>();
                    localPlayer.transform.position = destPosition;
                    DLog.Log($"You were locally teleported to {destName} at {destPosition}");
                    if (PhotonNetwork.IsConnected && localPhotonView != null)
                    {
                        localPhotonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { destPosition, localPlayer.transform.rotation });
                        DLog.Log($"RPC 'SpawnRPC' sent to all with position: {destPosition}");
                    }
                    else
                    {
                        DLog.Log("Not connected to Photon, local teleport only.");
                    }
                }
                else // Other player teleporting
                {
                    var photonViewField = playerObj.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (photonViewField == null)
                    {
                        DLog.Log("PhotonViewField not found on source player!");
                        return;
                    }
                    var photonView = photonViewField.GetValue(playerObj) as PhotonView;
                    if (photonView == null)
                    {
                        DLog.Log("PhotonView is not valid!");
                        return;
                    }
                    var playerMono = playerObj as MonoBehaviour;
                    if (playerMono == null)
                    {
                        DLog.Log("Source player is not a MonoBehaviour!");
                        return;
                    }
                    var transform = playerMono.transform;
                    if (transform == null)
                    {
                        DLog.Log("Transform of source player is null!");
                        return;
                    }
                    transform.position = destPosition;
                    DLog.Log($"{sourceName} locally teleported to {destName} at {destPosition}");
                    if (PhotonNetwork.IsConnected && photonView != null)
                    {
                        photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { destPosition, transform.rotation });
                        DLog.Log($"RPC 'SpawnRPC' sent to all with position: {destPosition}");
                    }
                    else
                    {
                        DLog.Log("Not connected to Photon, local teleport only.");
                    }
                }
            }
            catch (System.Exception e)
            {
                DLog.Log($"Error during teleport: {e.Message}");
            }
        }
    }
}
