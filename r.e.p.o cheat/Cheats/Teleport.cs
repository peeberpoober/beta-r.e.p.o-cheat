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
    }
}