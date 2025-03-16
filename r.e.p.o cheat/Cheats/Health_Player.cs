using System;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

namespace r.e.p.o_cheat
{
    static class Health_Player
    {
        static public object playerHealthInstance;
        static public object playerMaxHealthInstance;

        public static void HealPlayer(object targetPlayer, int healAmount, string playerName)
        {
            if (targetPlayer == null)
            {
                DLog.Log("Jogador alvo é nulo!");
                return;
            }

            try
            {
                DLog.Log($"Tentando curar: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = targetPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField não encontrado!"); return; }
                var photonView = photonViewField.GetValue(targetPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView não é válido!"); return; }

                var playerHealthField = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("Campo 'playerHealth' não encontrado!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(targetPlayer);
                if (playerHealthInstance == null) { DLog.Log("Instância de playerHealth é nula!"); return; }

                var healthType = playerHealthInstance.GetType();
                var healMethod = healthType.GetMethod("Heal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healMethod != null)
                {
                    healMethod.Invoke(playerHealthInstance, new object[] { healAmount, true });
                    DLog.Log($"Método 'Heal' chamado localmente com {healAmount} HP.");
                }
                else
                {
                    DLog.Log("Método 'Heal' não encontrado!");
                }

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var currentHealthField = healthType.GetField("currentHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    int currentHealth = currentHealthField != null ? (int)currentHealthField.GetValue(playerHealthInstance) : 0;
                    int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
                    DLog.Log(maxHealthField != null ? $"maxHealth encontrado: {maxHealth}" : "Campo 'maxHealth' não encontrado, usando valor padrão: 100");

                    photonView.RPC("UpdateHealthRPC", RpcTarget.AllBuffered, new object[] {maxHealth, maxHealth, true });
                    DLog.Log($"RPC 'UpdateHealthRPC' enviado para todos com saúde={currentHealth + maxHealth}, maxHealth={maxHealth}, effect=true.");

                    try
                    {
                        photonView.RPC("HealRPC", RpcTarget.AllBuffered, new object[] { healAmount, true });
                        DLog.Log($"RPC 'HealRPC' enviado com {healAmount} HP.");
                    }
                    catch
                    {
                        DLog.Log("RPC 'HealRPC' não registrado, confiando no UpdateHealthRPC.");
                    }
                }
                else
                {
                    DLog.Log("Não conectado ao Photon, cura apenas local.");
                }
                DLog.Log($"Tentativa de curar concluída.");
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao tentar curar: {e.Message}");
            }
        }

        public static void DamagePlayer(object targetPlayer, int damageAmount, string playerName)
        {
            if (targetPlayer == null)
            {
                DLog.Log("Jogador alvo é nulo!");
                return;
            }

            try
            {
                DLog.Log($"Tentando causar dano: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = targetPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField não encontrado!"); return; }
                var photonView = photonViewField.GetValue(targetPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView não é válido!"); return; }

                var playerHealthField = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("Campo 'playerHealth' não encontrado!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(targetPlayer);
                if (playerHealthInstance == null) { DLog.Log("Instância de playerHealth é nula!"); return; }

                var healthType = playerHealthInstance.GetType();
                var hurtMethod = healthType.GetMethod("Hurt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (hurtMethod != null)
                {
                    hurtMethod.Invoke(playerHealthInstance, new object[] { damageAmount, true, -1 });
                    DLog.Log($"Método 'Hurt' chamado localmente com {damageAmount} de dano.");
                }
                else
                {
                    DLog.Log("Método 'Hurt' não encontrado!");
                }

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
                    DLog.Log(maxHealthField != null ? $"maxHealth encontrado: {maxHealth}" : "Campo 'maxHealth' não encontrado, usando valor padrão: 100");

                    photonView.RPC("HurtOtherRPC", RpcTarget.AllBuffered, new object[] { damageAmount, Vector3.zero, false, -1 });
                    DLog.Log($"RPC 'HurtOtherRPC' enviado com {damageAmount} de dano.");

                    try
                    {
                        photonView.RPC("HurtRPC", RpcTarget.AllBuffered, new object[] { damageAmount, true, -1 });
                        DLog.Log($"RPC 'HurtRPC' enviado com {damageAmount} de dano.");
                    }
                    catch
                    {
                        DLog.Log("RPC 'HurtRPC' não registrado, confiando no HurtOtherRPC.");
                    }
                }
                else
                {
                    DLog.Log("Não conectado ao Photon, dano apenas local.");
                }
                DLog.Log($"Tentativa de causar dano concluída.");
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao tentar causar dano: {e.Message}");
            }
        }

        public static void MaxHealth()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                DLog.Log("PlayerController encontrado.");
                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    var playerAvatarScriptField = playerControllerInstance.GetType().GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                        var playerHealthField = playerAvatarScriptInstance.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.Instance);
                        if (playerHealthField != null)
                        {
                            var playerHealthInstance = playerHealthField.GetValue(playerAvatarScriptInstance);
                            var damageMethod = playerHealthInstance.GetType().GetMethod("UpdateHealthRPC");
                            if (damageMethod != null)
                            {
                                if (Hax2.infiniteHealthActive)
                                {
                                    damageMethod.Invoke(playerHealthInstance, new object[] { 999999, 100, true });
                                }
                                else if (!Hax2.infiniteHealthActive)
                                {
                                    damageMethod.Invoke(playerHealthInstance, new object[] { 100, 100, true });
                                }
                                DLog.Log("Vida máxima ajustada para 999999.");
                            }
                            else
                            {
                                DLog.Log("Método 'UpdateHealthRPC' não encontrado em playerHealth.");
                            }
                        }
                        else
                        {
                            DLog.Log("Campo 'playerHealth' não encontrado em playerAvatarScript.");
                        }
                    }
                    else
                    {
                        DLog.Log("Campo 'playerAvatarScript' não encontrado em PlayerController.");
                    }
                }
                else
                {
                    DLog.Log("playerControllerInstance não encontrado.");
                }
            }
            else
            {
                DLog.Log("Tipo PlayerController não encontrado.");
            }
        }
    }
}