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
                Hax2.Log1("colorControllerType (PlayerAvatar) não encontrado.");
                return;
            }
            Hax2.Log1("colorControllerType (PlayerAvatar) encontrado.");

            colorControllerInstance = null;

            if (PhotonNetwork.IsConnected)
            {
                var photonViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
                Hax2.Log1($"Encontrados {photonViews.Length} PhotonViews na cena.");
                foreach (var photonView in photonViews)
                {
                    if (photonView != null && photonView.IsMine)
                    {
                        var playerAvatar = photonView.gameObject.GetComponent(colorControllerType);
                        if (playerAvatar != null)
                        {
                            colorControllerInstance = playerAvatar;
                            Hax2.Log1($"PlayerAvatar local encontrado via PhotonView: {photonView.gameObject.name}, Owner: {photonView.Owner?.NickName}");
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
                    Hax2.Log1($"PlayerAvatar encontrado no singleplayer via FindObjectOfType: {(playerAvatar as MonoBehaviour).gameObject.name}");
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
                            Hax2.Log1($"PlayerAvatar encontrado no singleplayer via GetLocalPlayer: {localPlayer.name}");
                        }
                        else
                        {
                            Hax2.Log1("Componente PlayerAvatar não encontrado no objeto retornado por GetLocalPlayer.");
                        }
                    }
                    else
                    {
                        Hax2.Log1("Nenhum PlayerAvatar encontrado no singleplayer via GetLocalPlayer.");
                    }
                }
            }

            if (colorControllerInstance == null)
            {
                Hax2.Log1("Nenhum PlayerAvatar local encontrado para este cliente (multiplayer ou singleplayer).");
                return;
            }

            playerSetColorMethod = colorControllerType.GetMethod("PlayerAvatarSetColor", BindingFlags.Public | BindingFlags.Instance);
            if (playerSetColorMethod == null)
            {
                Hax2.Log1("Método PlayerAvatarSetColor não encontrado em PlayerAvatar.");
                return;
            }

            isInitialized = true;
            Hax2.Log1("playerColor inicializado com sucesso para o jogador local.");
        }

        public static void colorRandomizer()
        {
            Initialize();

            if (!isInitialized || colorControllerInstance == null || playerSetColorMethod == null)
            {
                Hax2.Log1("Randomizer ignorado: Falha na inicialização ou instância/método ausentes.");
                return;
            }

            if (isRandomizing && Time.time - lastColorChangeTime >= changeInterval)
            {
                var colorIndex = new System.Random().Next(0, 30);
                try
                {
                    playerSetColorMethod.Invoke(colorControllerInstance, new object[] { colorIndex });
                    lastColorChangeTime = Time.time;
                    Hax2.Log1($"Cor do jogador local alterada para índice: {colorIndex}");
                }
                catch (Exception e)
                {
                    Hax2.Log1($"Erro ao invocar PlayerAvatarSetColor: {e.Message}");
                }
            }
        }

        public static void Reset()
        {
            isInitialized = false;
            colorControllerType = null;
            colorControllerInstance = null;
            playerSetColorMethod = null;
            Hax2.Log1("playerColor reiniciado.");
        }
    }
}