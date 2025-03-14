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
                Hax2.Log1("Índice de jogador inválido!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                Hax2.Log1("Jogador selecionado é nulo!");
                return;
            }

            try
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                {
                    Hax2.Log1("Jogador local não encontrado!");
                    return;
                }

                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    Hax2.Log1("PhotonViewField não encontrado no jogador selecionado!");
                    return;
                }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null)
                {
                    Hax2.Log1("PhotonView não é válido!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    Hax2.Log1("selectedPlayer não é um MonoBehaviour!");
                    return;
                }

                var transform = playerMono.transform;
                if (transform == null)
                {
                    Hax2.Log1("Transform do jogador selecionado é nulo!");
                    return;
                }

                Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                transform.position = targetPosition;
                Hax2.Log1($"Jogador {playerNames[selectedPlayerIndex]} teleportado localmente para {targetPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, transform.rotation });
                    Hax2.Log1($"RPC 'SpawnRPC' enviado para todos com posição: {targetPosition}");
                }
                else
                {
                    Hax2.Log1("Não conectado ao Photon, teleporte apenas local.");
                }
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao teleportar {playerNames[selectedPlayerIndex]} até você: {e.Message}");
            }
        }

        public static void TeleportMeToPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                Hax2.Log1("Índice de jogador inválido!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                Hax2.Log1("Jogador selecionado é nulo!");
                return;
            }

            try
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                {
                    Hax2.Log1("Jogador local não encontrado!");
                    return;
                }

                var localPhotonViewField = localPlayer.GetComponent<PhotonView>();
                if (localPhotonViewField == null)
                {
                    Hax2.Log1("PhotonViewField não encontrado no jogador local!");
                    return;
                }
                var localPhotonView = localPhotonViewField;
                if (localPhotonView == null)
                {
                    Hax2.Log1("PhotonView local não é válido!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    Hax2.Log1("selectedPlayer não é um MonoBehaviour!");
                    return;
                }

                var targetTransform = playerMono.transform;
                if (targetTransform == null)
                {
                    Hax2.Log1("Transform do jogador selecionado é nulo!");
                    return;
                }

                Vector3 targetPosition = targetTransform.position + Vector3.up * 1.5f;
                localPlayer.transform.position = targetPosition;
                Hax2.Log1($"Você foi teleportado localmente para {playerNames[selectedPlayerIndex]} em {targetPosition}");

                if (PhotonNetwork.IsConnected && localPhotonView != null)
                {
                    localPhotonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, localPlayer.transform.rotation });
                    Hax2.Log1($"RPC 'SpawnRPC' enviado para todos com posição: {targetPosition}");
                }
                else
                {
                    Hax2.Log1("Não conectado ao Photon, teleporte apenas local.");
                }
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao teleportar você até {playerNames[selectedPlayerIndex]}: {e.Message}");
            }
        }
    }
}