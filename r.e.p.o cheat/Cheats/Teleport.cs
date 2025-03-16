using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Reflection;

namespace r.e.p.o_cheat
{
    public static class Teleport
    {
        public static void TeleportPlayerToMe(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                DLog.Log("Índice de jogador inválido!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                DLog.Log("Jogador selecionado é nulo!");
                return;
            }

            try
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                {
                    DLog.Log("Jogador local não encontrado!");
                    return;
                }

                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    DLog.Log("PhotonViewField não encontrado no jogador selecionado!");
                    return;
                }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null)
                {
                    DLog.Log("PhotonView não é válido!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    DLog.Log("selectedPlayer não é um MonoBehaviour!");
                    return;
                }

                var transform = playerMono.transform;
                if (transform == null)
                {
                    DLog.Log("Transform do jogador selecionado é nulo!");
                    return;
                }

                Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                transform.position = targetPosition;
                DLog.Log($"Jogador {playerNames[selectedPlayerIndex]} teleportado localmente para {targetPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, transform.rotation });
                    DLog.Log($"RPC 'SpawnRPC' enviado para todos com posição: {targetPosition}");
                }
                else
                {
                    DLog.Log("Não conectado ao Photon, teleporte apenas local.");
                }
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao teleportar {playerNames[selectedPlayerIndex]} até você: {e.Message}");
            }
        }

        public static void TeleportMeToPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                DLog.Log("Índice de jogador inválido!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                DLog.Log("Jogador selecionado é nulo!");
                return;
            }

            try
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                {
                    DLog.Log("Jogador local não encontrado!");
                    return;
                }

                var localPhotonViewField = localPlayer.GetComponent<PhotonView>();
                if (localPhotonViewField == null)
                {
                    DLog.Log("PhotonViewField não encontrado no jogador local!");
                    return;
                }
                var localPhotonView = localPhotonViewField;
                if (localPhotonView == null)
                {
                    DLog.Log("PhotonView local não é válido!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    DLog.Log("selectedPlayer não é um MonoBehaviour!");
                    return;
                }

                var targetTransform = playerMono.transform;
                if (targetTransform == null)
                {
                    DLog.Log("Transform do jogador selecionado é nulo!");
                    return;
                }

                Vector3 targetPosition = targetTransform.position + Vector3.up * 1.5f;
                localPlayer.transform.position = targetPosition;
                DLog.Log($"Você foi teleportado localmente para {playerNames[selectedPlayerIndex]} em {targetPosition}");

                if (PhotonNetwork.IsConnected && localPhotonView != null)
                {
                    localPhotonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, localPlayer.transform.rotation });
                    DLog.Log($"RPC 'SpawnRPC' enviado para todos com posição: {targetPosition}");
                }
                else
                {
                    DLog.Log("Não conectado ao Photon, teleporte apenas local.");
                }
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao teleportar você até {playerNames[selectedPlayerIndex]}: {e.Message}");
            }
        }
    }
}