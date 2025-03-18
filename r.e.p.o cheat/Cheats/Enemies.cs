using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

namespace r.e.p.o_cheat
{
    public static class Enemies
    {
        private static Dictionary<Enemy, int> enemyMaxHealthCache = new Dictionary<Enemy, int>();
        public static List<Enemy> enemyList = new List<Enemy>();

        public static void KillSelectedEnemy(int selectedEnemyIndex, List<Enemy> enemyList, List<string> enemyNames)
        {
            if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
            {
                DLog.Log("Invalid enemy index!");
                return;
            }

            var selectedEnemy = enemyList[selectedEnemyIndex];
            if (selectedEnemy == null)
            {
                DLog.Log("Selected enemy is null!");
                return;
            }

            try
            {
                var healthField = selectedEnemy.GetType().GetField("Health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthField != null)
                {
                    var healthComponent = healthField.GetValue(selectedEnemy);
                    if (healthComponent != null)
                    {
                        var healthType = healthComponent.GetType();
                        var hurtMethod = healthType.GetMethod("Hurt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (hurtMethod != null)
                        {
                            hurtMethod.Invoke(healthComponent, new object[] { 9999, Vector3.zero });
                            DLog.Log($"Enemy {enemyNames[selectedEnemyIndex]} hurt with 9999 damage via Hurt");
                        }
                        else
                            DLog.Log("'Hurt' method not found in EnemyHealth");
                    }
                    else
                        DLog.Log("EnemyHealth component is null");
                }
                else
                    DLog.Log("'Health' field not found in Enemy");

                // Update enemy list after killing
                DebugCheats.UpdateEnemyList();
            }
            catch (Exception e)
            {
                DLog.Log($"Error killing enemy {enemyNames[selectedEnemyIndex]}: {e.Message}");
            }
        }

        public static void KillAllEnemies()
        {
            DLog.Log("Attempting to kill all enemies");

            // Get the list directly from DebugCheats since that's the source of truth
            var currentEnemyList = DebugCheats.enemyList;

            if (currentEnemyList == null || currentEnemyList.Count == 0)
            {
                DLog.Log("No enemies found to kill");
                return;
            }

            DLog.Log($"Found {currentEnemyList.Count} enemies to process");
            int killCount = 0;

            // Create a copy of the list to avoid modification issues during iteration
            List<Enemy> enemiesCopy = new List<Enemy>(currentEnemyList);

            foreach (var enemyInstance in enemiesCopy)
            {
                if (enemyInstance == null) continue;

                try
                {
                    var healthField = enemyInstance.GetType().GetField("Health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (healthField != null)
                    {
                        var healthComponent = healthField.GetValue(enemyInstance);
                        if (healthComponent != null)
                        {
                            var healthType = healthComponent.GetType();
                            var hurtMethod = healthType.GetMethod("Hurt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (hurtMethod != null)
                            {
                                hurtMethod.Invoke(healthComponent, new object[] { 9999, Vector3.zero });
                                killCount++;
                            }
                            else
                            {
                                DLog.Log($"'Hurt' method not found in EnemyHealth for enemy {enemyInstance.name}");
                            }
                        }
                        else
                        {
                            DLog.Log($"EnemyHealth component is null for enemy {enemyInstance.name}");
                        }
                    }
                    else
                    {
                        DLog.Log($"'Health' field not found in Enemy {enemyInstance.name}");
                    }
                }
                catch (Exception e)
                {
                    DLog.Log($"Error killing enemy {enemyInstance.name}: {e.Message}");
                }
            }

            DLog.Log($"Killed {killCount} out of {enemiesCopy.Count} enemies");

            // Update the enemy list after killing
            DebugCheats.UpdateEnemyList();
        }

        public static void TeleportEnemyToMe(int selectedEnemyIndex, List<Enemy> enemyList, List<string> enemyNames)
        {
            if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
            {
                DLog.Log($"Invalid enemy index! selectedEnemyIndex={selectedEnemyIndex}, enemyList.Count={enemyList.Count}");
                return;
            }

            var selectedEnemy = enemyList[selectedEnemyIndex];
            if (selectedEnemy == null)
            {
                DLog.Log("Selected enemy is null!");
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

                Vector3 forwardDirection = localPlayer.transform.forward;
                Vector3 targetPosition = localPlayer.transform.position + forwardDirection * 1f + Vector3.up * 1.5f;

                var photonView = selectedEnemy.GetComponent<PhotonView>();
                if (PhotonNetwork.IsConnected && photonView != null && !photonView.IsMine)
                {
                    photonView.RequestOwnership();
                    DLog.Log($"Requested ownership of enemy {enemyNames[selectedEnemyIndex]} to ensure local control.");
                }

                var navMeshAgentField = selectedEnemy.GetType().GetField("NavMeshAgent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object navMeshAgent = null;
                if (navMeshAgentField != null)
                {
                    navMeshAgent = navMeshAgentField.GetValue(selectedEnemy);
                    if (navMeshAgent != null)
                    {
                        var enabledProperty = navMeshAgent.GetType().GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
                        if (enabledProperty != null)
                        {
                            enabledProperty.SetValue(navMeshAgent, false);
                            DLog.Log($"NavMeshAgent of {enemyNames[selectedEnemyIndex]} disabled to prevent immediate movement.");
                        }
                    }
                }

                selectedEnemy.transform.position = targetPosition;
                DLog.Log($"Enemy {enemyNames[selectedEnemyIndex]} teleported locally to {targetPosition}");

                Vector3 currentPosition = selectedEnemy.transform.position;
                DLog.Log($"Current position of enemy after teleport: {currentPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var enemyType = selectedEnemy.GetType();
                    var teleportMethod = enemyType.GetMethod("EnemyTeleported", BindingFlags.Public | BindingFlags.Instance);
                    if (teleportMethod != null)
                    {
                        teleportMethod.Invoke(selectedEnemy, new object[] { targetPosition });
                        DLog.Log($"Enemy {enemyNames[selectedEnemyIndex]} teleported via EnemyTeleported for multiplayer synchronization.");
                    }
                    else
                    {
                        DLog.Log("'EnemyTeleported' method not found, synchronization may not occur.");
                    }
                }

                if (navMeshAgent != null)
                {
                    MonoBehaviour mb = selectedEnemy as MonoBehaviour;
                    if (mb != null)
                    {
                        mb.StartCoroutine(ReEnableNavMeshAgent(navMeshAgent, 2f));
                    }
                }

                // Get the GameObject directly from the Enemy (which is a MonoBehaviour)
                GameObject enemyGameObject = selectedEnemy is MonoBehaviour ? ((MonoBehaviour)selectedEnemy).gameObject : null;
                if (enemyGameObject != null && enemyGameObject.activeInHierarchy)
                {
                    enemyGameObject.SetActive(false);
                    enemyGameObject.SetActive(true);
                    DLog.Log($"Enemy {enemyNames[selectedEnemyIndex]} reactivated to force rendering.");
                }
                else
                {
                    DLog.Log($"Enemy GameObject {enemyNames[selectedEnemyIndex]} not found or not active for re-rendering.");
                }

                DLog.Log($"Teleport of {enemyNames[selectedEnemyIndex]} completed.");
            }
            catch (Exception e)
            {
                DLog.Log($"Error teleporting enemy {enemyNames[selectedEnemyIndex]}: {e.Message}");
            }
        }

        public static void TeleportEnemyToPlayer(int selectedEnemyIndex, List<Enemy> enemyList, List<string> enemyNames,
                                        int targetPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
            {
                DLog.Log($"Invalid enemy index! selectedEnemyIndex={selectedEnemyIndex}, enemyList.Count={enemyList.Count}");
                return;
            }

            var selectedEnemy = enemyList[selectedEnemyIndex];
            if (selectedEnemy == null)
            {
                DLog.Log("Selected enemy is null!");
                return;
            }

            if (targetPlayerIndex < 0 || targetPlayerIndex >= playerList.Count)
            {
                DLog.Log($"Invalid target player index! targetPlayerIndex={targetPlayerIndex}, playerList.Count={playerList.Count}");
                return;
            }

            var targetPlayer = playerList[targetPlayerIndex];
            if (targetPlayer == null)
            {
                DLog.Log("Target player is null!");
                return;
            }

            try
            {
                Vector3 targetPosition; // Get the target player's position

                if (targetPlayer is GameObject playerObj) // Handle different player types to get position
                {
                    targetPosition = playerObj.transform.position + Vector3.up * 1.5f;
                }
                else if (targetPlayer is MonoBehaviour playerMono)
                {
                    targetPosition = playerMono.transform.position + Vector3.up * 1.5f;
                }
                else
                {
                    var transformField = targetPlayer.GetType().GetField("transform", // Try to get transform via reflection for other player types
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    if (transformField != null)
                    {
                        Transform transform = transformField.GetValue(targetPlayer) as Transform;
                        if (transform != null)
                        {
                            targetPosition = transform.position + Vector3.up * 1.5f;
                        }
                        else
                        {
                            DLog.Log("Could not get transform from player!");
                            return;
                        }
                    }
                    else
                    {
                        DLog.Log("Could not find transform field on player!");
                        return;
                    }
                }

                var photonView = selectedEnemy.GetComponent<PhotonView>();
                if (PhotonNetwork.IsConnected && photonView != null && !photonView.IsMine)
                {
                    photonView.RequestOwnership();
                    DLog.Log($"Requested ownership of enemy {enemyNames[selectedEnemyIndex]} to ensure local control.");
                }

                var navMeshAgentField = selectedEnemy.GetType().GetField("NavMeshAgent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object navMeshAgent = null;
                if (navMeshAgentField != null)
                {
                    navMeshAgent = navMeshAgentField.GetValue(selectedEnemy);
                    if (navMeshAgent != null)
                    {
                        var enabledProperty = navMeshAgent.GetType().GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
                        if (enabledProperty != null)
                        {
                            enabledProperty.SetValue(navMeshAgent, false);
                            DLog.Log($"NavMeshAgent of {enemyNames[selectedEnemyIndex]} disabled to prevent immediate movement.");
                        }
                    }
                }

                selectedEnemy.transform.position = targetPosition;
                DLog.Log($"Enemy {enemyNames[selectedEnemyIndex]} teleported locally to {playerNames[targetPlayerIndex]} at {targetPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var enemyType = selectedEnemy.GetType();
                    var teleportMethod = enemyType.GetMethod("EnemyTeleported", BindingFlags.Public | BindingFlags.Instance);
                    if (teleportMethod != null)
                    {
                        teleportMethod.Invoke(selectedEnemy, new object[] { targetPosition });
                        DLog.Log($"Enemy {enemyNames[selectedEnemyIndex]} teleported via EnemyTeleported for multiplayer synchronization.");
                    }
                    else
                    {
                        DLog.Log("'EnemyTeleported' method not found, synchronization may not occur.");
                    }
                }

                if (navMeshAgent != null)
                {
                    MonoBehaviour mb = selectedEnemy as MonoBehaviour;
                    if (mb != null)
                    {
                        mb.StartCoroutine(ReEnableNavMeshAgent(navMeshAgent, 2f));
                    }
                }

                // Get the GameObject directly from the Enemy (which is a MonoBehaviour)
                GameObject enemyGameObject = selectedEnemy is MonoBehaviour ? ((MonoBehaviour)selectedEnemy).gameObject : null;
                if (enemyGameObject != null && enemyGameObject.activeInHierarchy)
                {
                    enemyGameObject.SetActive(false);
                    enemyGameObject.SetActive(true);
                    DLog.Log($"Enemy {enemyNames[selectedEnemyIndex]} reactivated to force rendering.");
                }
                else
                {
                    DLog.Log($"Enemy GameObject {enemyNames[selectedEnemyIndex]} not found or not active for re-rendering.");
                }

                DLog.Log($"Teleport of {enemyNames[selectedEnemyIndex]} to {playerNames[targetPlayerIndex]} completed.");
            }
            catch (Exception e)
            {
                DLog.Log($"Error teleporting enemy {enemyNames[selectedEnemyIndex]} to player {playerNames[targetPlayerIndex]}: {e.Message}");
            }
        }

        private static IEnumerator ReEnableNavMeshAgent(object navMeshAgent, float delay)
        {
            yield return new WaitForSeconds(delay);
            var enabledProperty = navMeshAgent.GetType().GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
            if (enabledProperty != null)
            {
                enabledProperty.SetValue(navMeshAgent, true);
                DLog.Log("NavMeshAgent reactivated after teleport.");
            }
        }
        
        public static int GetEnemyHealth(Enemy enemy)
        {
            try
            {
                var healthField = enemy.GetType().GetField("Health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthField == null) return -1;

                var healthComponent = healthField.GetValue(enemy);
                if (healthComponent == null) return -1;

                var healthValueField = healthComponent.GetType().GetField("healthCurrent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthValueField == null) return -1;

                return (int)healthValueField.GetValue(healthComponent);
            }
            catch (Exception e)
            {
                DLog.Log($"Error getting enemy health: {e.Message}");
                return -1;
            }
        }

        public static int GetEnemyMaxHealth(Enemy enemy)
        {
            if (enemyMaxHealthCache.TryGetValue(enemy, out int cachedMaxHealth))
            {
                return cachedMaxHealth;
            }

            int currentHealth = GetEnemyHealth(enemy);
            int maxHealth = currentHealth > 0 ? currentHealth : 100;
            
            enemyMaxHealthCache[enemy] = maxHealth;
            
            return maxHealth;
        }

        public static float GetEnemyHealthPercentage(Enemy enemy)
        {
            int health = GetEnemyHealth(enemy);
            int maxHealth = GetEnemyMaxHealth(enemy);
            
            if (health < 0 || maxHealth <= 0) return -1f;
            
            return (float)health / maxHealth;
        }
    }
}
