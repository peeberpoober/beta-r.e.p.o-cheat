using System;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

namespace r.e.p.o_cheat
{
    internal class playerColor
    {
        public static bool isRandomizing = false;
        private static float lastColorChangeTime = 0f;
        private static float changeInterval = 0.1f;

        private static Type colorControllerType;
        private static object colorControllerInstance;
        private static MethodInfo playerSetColorMethod;
        private static bool isInitialized = false;

        private static void Initialize()
        {
            if (isInitialized) return;

            colorControllerType = Type.GetType("PlayerAvatar, Assembly-CSharp");
            if (colorControllerType == null)
            {
                DLog.Log("colorControllerType (PlayerAvatar) não encontrado.");
                return;
            }
            DLog.Log("colorControllerType (PlayerAvatar) encontrado.");

            colorControllerInstance = null;

            if (PhotonNetwork.IsConnected)
            {
                var photonViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
                DLog.Log($"Encontrados {photonViews.Length} PhotonViews na cena.");
                foreach (var photonView in photonViews)
                {
                    if (photonView != null && photonView.IsMine)
                    {
                        var playerAvatar = photonView.gameObject.GetComponent(colorControllerType);
                        if (playerAvatar != null)
                        {
                            colorControllerInstance = playerAvatar;
                            DLog.Log($"PlayerAvatar local encontrado via PhotonView: {photonView.gameObject.name}, Owner: {photonView.Owner?.NickName}");
                            break;
                        }
                    }
                }
            }
            else
            {
                var playerAvatar = UnityEngine.Object.FindObjectOfType(colorControllerType);
                if (playerAvatar != null)
                {
                    colorControllerInstance = playerAvatar;
                    DLog.Log($"PlayerAvatar encontrado no singleplayer via FindObjectOfType: {(playerAvatar as MonoBehaviour).gameObject.name}");
                }
                else
                {
                    GameObject localPlayer = DebugCheats.GetLocalPlayer();
                    if (localPlayer != null)
                    {
                        var playerAvatarComponent = localPlayer.GetComponent(colorControllerType);
                        if (playerAvatarComponent != null)
                        {
                            colorControllerInstance = playerAvatarComponent;
                            DLog.Log($"PlayerAvatar encontrado no singleplayer via GetLocalPlayer: {localPlayer.name}");
                        }
                        else
                        {
                            DLog.Log("Componente PlayerAvatar não encontrado no objeto retornado por GetLocalPlayer.");
                        }
                    }
                    else
                    {
                        DLog.Log("Nenhum PlayerAvatar encontrado no singleplayer via GetLocalPlayer.");
                    }
                }
            }

            if (colorControllerInstance == null)
            {
                DLog.Log("Nenhum PlayerAvatar local encontrado para este cliente (multiplayer ou singleplayer).");
                return;
            }

            playerSetColorMethod = colorControllerType.GetMethod("PlayerAvatarSetColor", BindingFlags.Public | BindingFlags.Instance);
            if (playerSetColorMethod == null)
            {
                DLog.Log("Método PlayerAvatarSetColor não encontrado em PlayerAvatar.");
                return;
            }

            isInitialized = true;
            DLog.Log("playerColor inicializado com sucesso para o jogador local.");
        }

        public static void colorRandomizer()
        {
            Initialize();

            if (!isInitialized || colorControllerInstance == null || playerSetColorMethod == null)
            {
                DLog.Log("Randomizer ignorado: Falha na inicialização ou instância/método ausentes.");
                return;
            }

            if (isRandomizing && Time.time - lastColorChangeTime >= changeInterval)
            {
                var colorIndex = new System.Random().Next(0, 30);
                try
                {
                    playerSetColorMethod.Invoke(colorControllerInstance, new object[] { colorIndex });
                    lastColorChangeTime = Time.time;
                    DLog.Log($"Cor do jogador local alterada para índice: {colorIndex}");
                }
                catch (Exception e)
                {
                    DLog.Log($"Erro ao invocar PlayerAvatarSetColor: {e.Message}");
                }
            }
        }

        public static void Reset()
        {
            isInitialized = false;
            colorControllerType = null;
            colorControllerInstance = null;
            playerSetColorMethod = null;
            DLog.Log("playerColor reiniciado.");
        }
    }
}