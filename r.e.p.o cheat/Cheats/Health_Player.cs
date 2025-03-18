﻿using System;
using System.Reflection;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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
                DLog.Log("Target player is null!");
                return;
            }

            try
            {
                DLog.Log($"Attempting to heal: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = targetPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField not found!"); return; }
                var photonView = photonViewField.GetValue(targetPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView is not valid!"); return; }

                var playerHealthField = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("'playerHealth' field not found!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(targetPlayer);
                if (playerHealthInstance == null) { DLog.Log("playerHealth instance is null!"); return; }

                var healthType = playerHealthInstance.GetType();
                var healMethod = healthType.GetMethod("Heal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healMethod != null)
                {
                    healMethod.Invoke(playerHealthInstance, new object[] { healAmount, true });
                    DLog.Log($"'Heal' method called locally with {healAmount} HP.");
                }
                else
                {
                    DLog.Log("'Heal' method not found!");
                }

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var currentHealthField = healthType.GetField("currentHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    int currentHealth = currentHealthField != null ? (int)currentHealthField.GetValue(playerHealthInstance) : 0;
                    int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
                    DLog.Log(maxHealthField != null ? $"maxHealth found: {maxHealth}" : "'maxHealth' field not found, using default value: 100");

                    photonView.RPC("UpdateHealthRPC", RpcTarget.AllBuffered, new object[] {maxHealth, maxHealth, true });
                    DLog.Log($"RPC 'UpdateHealthRPC' sent to all with health={currentHealth + maxHealth}, maxHealth={maxHealth}, effect=true.");

                    try
                    {
                        photonView.RPC("HealRPC", RpcTarget.AllBuffered, new object[] { healAmount, true });
                        DLog.Log($"RPC 'HealRPC' sent with {healAmount} HP.");
                    }
                    catch
                    {
                        DLog.Log("RPC 'HealRPC' not registered, relying on UpdateHealthRPC.");
                    }
                }
                else
                {
                    DLog.Log("Not connected to Photon, local healing only.");
                }
                DLog.Log($"Healing attempt completed.");
            }
            catch (Exception e)
            {
                DLog.Log($"Error trying to heal: {e.Message}");
            }
        }

        public static void DamagePlayer(object targetPlayer, int damageAmount, string playerName)
        {
            if (targetPlayer == null)
            {
                DLog.Log("Target player is null!");
                return;
            }

            try
            {
                DLog.Log($"Attempting to damage: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = targetPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField not found!"); return; }
                var photonView = photonViewField.GetValue(targetPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView is not valid!"); return; }

                var playerHealthField = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("'playerHealth' field not found!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(targetPlayer);
                if (playerHealthInstance == null) { DLog.Log("playerHealth instance is null!"); return; }

                var healthType = playerHealthInstance.GetType();
                var hurtMethod = healthType.GetMethod("Hurt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (hurtMethod != null)
                {
                    hurtMethod.Invoke(playerHealthInstance, new object[] { damageAmount, true, -1 });
                    DLog.Log($"'Hurt' method called locally with {damageAmount} damage.");
                }
                else
                {
                    DLog.Log("'Hurt' method not found!");
                }

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
                    DLog.Log(maxHealthField != null ? $"maxHealth found: {maxHealth}" : "'maxHealth' field not found, using default value: 100");

                    photonView.RPC("HurtOtherRPC", RpcTarget.AllBuffered, new object[] { damageAmount, Vector3.zero, false, -1 });
                    DLog.Log($"RPC 'HurtOtherRPC' sent with {damageAmount} damage.");

                    try
                    {
                        photonView.RPC("HurtRPC", RpcTarget.AllBuffered, new object[] { damageAmount, true, -1 });
                        DLog.Log($"RPC 'HurtRPC' sent with {damageAmount} damage.");
                    }
                    catch
                    {
                        DLog.Log("RPC 'HurtRPC' not registered, relying on HurtOtherRPC.");
                    }
                }
                else
                {
                    DLog.Log("Not connected to Photon, local damage only.");
                }
                DLog.Log($"Damage attempt completed.");
            }
            catch (Exception e)
            {
                DLog.Log($"Error trying to damage: {e.Message}");
            }
        }

        public static void ReviveSelectedPlayer(object selectedPlayer, string playerName)
        {
            if (selectedPlayer == null)
            {
                DLog.Log("Selected player is null!");
                return;
            }

            try
            {
                var playerDeathHeadField = selectedPlayer.GetType().GetField("playerDeathHead", BindingFlags.Public | BindingFlags.Instance);
                if (playerDeathHeadField != null)
                {
                    var playerDeathHeadInstance = playerDeathHeadField.GetValue(selectedPlayer);
                    if (playerDeathHeadInstance != null)
                    {
                        // Retrieve and modify 'inExtractionPoint' to allow revival
                        var inExtractionPointField = playerDeathHeadInstance.GetType().GetField("inExtractionPoint", BindingFlags.NonPublic | BindingFlags.Instance);
                        var reviveMethod = playerDeathHeadInstance.GetType().GetMethod("Revive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (inExtractionPointField != null)
                        {
                            inExtractionPointField.SetValue(playerDeathHeadInstance, true);
                            DLog.Log("'inExtractionPoint' field set to true.");
                            DLog.Log("'inExtractionPoint' field set to true.");
                        }
                        if (reviveMethod != null)
                        {
                            reviveMethod.Invoke(playerDeathHeadInstance, null);
                            DLog.Log("'Revive' method successfully called for: " + playerName);
                        }
                        else DLog.Log("'Revive' method not found!");
                    }
                    else DLog.Log("'playerDeathHead' instance not found.");
                }
                else DLog.Log("'playerDeathHead' field not found.");

                var playerHealthField = selectedPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField != null)
                {
                    var playerHealthInstance = playerHealthField.GetValue(selectedPlayer);
                    if (playerHealthInstance != null)
                    {
                        var healthType = playerHealthInstance.GetType();
                        var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var healthField = healthType.GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
                        DLog.Log($"Max health retrieved: {maxHealth}");
                        if (healthField != null)
                        {
                            healthField.SetValue(playerHealthInstance, maxHealth);
                            DLog.Log($"Health set directly to {maxHealth} via 'health' field.");
                        }
                        else
                        {
                            DLog.Log("'health' field not found, attempting HealPlayer as fallback.");
                            Health_Player.HealPlayer(selectedPlayer, maxHealth, playerName);
                        }

                        int currentHealth = healthField != null ? (int)healthField.GetValue(playerHealthInstance) : -1;
                        DLog.Log($"Current health after revive: {currentHealth}");
                    }
                    else DLog.Log("PlayerHealth instance is null, health restoration failed.");
                }
                else DLog.Log("'playerHealth' field not found, healing not performed.");
            }
            catch (Exception e)
            {
                DLog.Log($"Error reviving and healing {playerName}: {e.Message}");
            }
        }

        public static void KillSelectedPlayer(object selectedPlayer, string playerName)
        {
            if (selectedPlayer == null) 
            { 
                DLog.Log("Selected player is null!"); 
                return; 
            }
            
            try
            {
                DLog.Log($"Attempting to kill: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField not found!"); return; }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView is not valid!"); return; }
                var playerHealthField = selectedPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("'playerHealth' field not found!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(selectedPlayer);
                if (playerHealthInstance == null) { DLog.Log("playerHealth instance is null!"); return; }
                var healthType = playerHealthInstance.GetType();
                var deathMethod = healthType.GetMethod("Death", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (deathMethod == null) { DLog.Log("'Death' method not found!"); return; }
                deathMethod.Invoke(playerHealthInstance, null);
                DLog.Log($"'Death' method called locally for {playerName}.");

                var playerAvatarField = healthType.GetField("playerAvatar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerAvatarField != null)
                {
                    var playerAvatarInstance = playerAvatarField.GetValue(playerHealthInstance);
                    if (playerAvatarInstance != null)
                    {
                        var playerAvatarType = playerAvatarInstance.GetType();
                        var playerDeathMethod = playerAvatarType.GetMethod("PlayerDeath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (playerDeathMethod != null) { playerDeathMethod.Invoke(playerAvatarInstance, new object[] { -1 }); DLog.Log($"'PlayerDeath' method called locally for {playerName}."); }
                        else DLog.Log("'PlayerDeath' method not found in PlayerAvatar!");
                    }
                    else DLog.Log("PlayerAvatar instance is null!");
                }
                else DLog.Log("'playerAvatar' field not found in PlayerHealth!");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
                    DLog.Log(maxHealthField != null ? $"maxHealth found: {maxHealth}" : "'maxHealth' field not found, using default value: 100");
                    photonView.RPC("UpdateHealthRPC", RpcTarget.AllBuffered, new object[] { 0, maxHealth, true });
                    DLog.Log($"RPC 'UpdateHealthRPC' sent to all with health=0, maxHealth={maxHealth}, effect=true.");
                    try { photonView.RPC("PlayerDeathRPC", RpcTarget.AllBuffered, new object[] { -1 }); DLog.Log("Trying RPC 'PlayerDeathRPC' to force death..."); }
                    catch { DLog.Log("RPC 'PlayerDeathRPC' not registered, trying alternative..."); }
                    photonView.RPC("HurtOtherRPC", RpcTarget.AllBuffered, new object[] { 9999, Vector3.zero, false, -1 });
                    DLog.Log("RPC 'HurtOtherRPC' sent with 9999 damage to ensure death.");
                }
                else DLog.Log("Not connected to Photon, death is only local.");
                DLog.Log($"Attempt to kill {playerName} completed.");
            }
            catch (Exception e) { DLog.Log($"Error trying to kill {playerName}: {e.Message}"); }
        }
    }
}