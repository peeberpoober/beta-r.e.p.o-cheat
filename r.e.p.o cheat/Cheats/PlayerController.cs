using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

namespace r.e.p.o_cheat
{
    class PlayerController
    {
        public static object playerSpeedInstance;
        public static object reviveInstance;
        public static object enemyDirectorInstance;
        public static object playerControllerInstance;
        public static Type playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");

        private static float desiredDelayMultiplier = 1f;
        private static float desiredRateMultiplier = 1f;

        private static void InitializePlayerController()
        {
            if (playerControllerType == null)
            {
                Hax2.Log1("PlayerController type not found.");
                return;
            }

            playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
            if (playerControllerInstance == null)
            {
                Hax2.Log1("PlayerController instance not found in current scene.");
            }
            else
            {
                Hax2.Log1("PlayerController instance updated successfully.");
            }
        }

        public static void GodMode()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                Hax2.Log1("PlayerController found.");

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

                            var godModeField = playerHealthInstance.GetType().GetField("godMode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (godModeField != null)
                            {
                                bool currentGodMode = (bool)godModeField.GetValue(playerHealthInstance);

                                bool newGodModeState = !currentGodMode;
                                godModeField.SetValue(playerHealthInstance, newGodModeState);

                                Hax2.godModeActive = !newGodModeState;

                                Hax2.Log1("God Mode " + (newGodModeState ? "enabled" : "disabled"));
                            }
                            else
                            {
                                Hax2.Log1("godMode field not found in playerHealth.");
                            }
                        }
                        else
                        {
                            Hax2.Log1("playerHealth field not found in playerAvatarScript.");
                        }
                    }
                    else
                    {
                        Hax2.Log1("playerAvatarScript field not found in PlayerController.");
                    }
                }
                else
                {
                    Hax2.Log1("playerControllerInstance not found.");
                }
            }
            else
            {
                Hax2.Log1("PlayerController type not found.");
            }
        }

        public static void RemoveSpeed(float sliderValue)
        {
            var playerInSpeedType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerInSpeedType != null)
            {
                Hax2.Log1("playerInSpeedType n é null");
                playerSpeedInstance = GameHelper.FindObjectOfType(playerInSpeedType);
                if (playerSpeedInstance != null)
                {
                    Hax2.Log1("playerSpeedInstance n é null");
                }
                else
                {
                    Hax2.Log1("playerSpeedInstance null");
                }
            }
            else
            {
                Hax2.Log1("playerInSpeedType null");
            }
            if (playerSpeedInstance != null)
            {
                Hax2.Log1("playerSpeedInstance n é null");

                var playerControllerType = playerSpeedInstance.GetType();

                var moveSpeedField1 = playerControllerType.GetField("MoveSpeed", BindingFlags.Public | BindingFlags.Instance);

                if (moveSpeedField1 != null)
                {
                    moveSpeedField1.SetValue(playerSpeedInstance, sliderValue);
                    Hax2.Log1("MoveSpeed value set to " + sliderValue);
                }
                else
                {
                    Hax2.Log1("MoveSpeed field not found in PlayerController.");
                }
            }
        }

        public static void MaxStamina()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                Hax2.Log1("PlayerController found.");

                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    var energyCurrentField = playerControllerInstance.GetType().GetField("EnergyCurrent", BindingFlags.Public | BindingFlags.Instance);
                    if (energyCurrentField != null)
                    {
                        if (Hax2.stamineState)
                        {
                            energyCurrentField.SetValue(playerControllerInstance, 999999);
                        }
                        else if (!Hax2.stamineState)
                        {
                            energyCurrentField.SetValue(playerControllerInstance, 40);
                        }

                        Hax2.Log1("EnergyCurrent set to " + (Hax2.stamineState ? 999999 : 40));
                    }
                    else
                    {
                        Hax2.Log1("EnergyCurrent field not found in playerAvatarScript.");
                    }
                }
                else
                {
                    Hax2.Log1("playerControllerInstance not found.");
                }
            }
            else
            {
                Hax2.Log1("PlayerController type not found.");
            }
        }

        public static void DecreaseStaminaRechargeDelay(float delayMultiplier, float rateMultiplier = 1f)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            desiredDelayMultiplier = delayMultiplier;
            desiredRateMultiplier = rateMultiplier;

            Hax2.Log1("Attempting to decrease stamina recharge delay.");

            var sprintRechargeTimeField = playerControllerType.GetField("sprintRechargeTime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sprintRechargeTimeField != null)
            {
                float defaultRechargeTime = 1f;
                float newRechargeTime = defaultRechargeTime * delayMultiplier;
                sprintRechargeTimeField.SetValue(playerControllerInstance, newRechargeTime);
                Hax2.Log1($"sprintRechargeTime set to {newRechargeTime} (multiplier: {delayMultiplier})");
            }
            else
            {
                Hax2.Log1("sprintRechargeTime field not found in PlayerController.");
            }

            var sprintRechargeAmountField = playerControllerType.GetField("sprintRechargeAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sprintRechargeAmountField != null)
            {
                float defaultRechargeAmount = 2f;
                float newRechargeAmount = defaultRechargeAmount * rateMultiplier;
                sprintRechargeAmountField.SetValue(playerControllerInstance, newRechargeAmount);
                Hax2.Log1($"sprintRechargeAmount set to {newRechargeAmount} (multiplier: {rateMultiplier})");
            }
            else
            {
                Hax2.Log1("sprintRechargeAmount field not found in PlayerController.");
            }
        }

        public static void ReapplyStaminaSettings()
        {
            InitializePlayerController();
            if (playerControllerInstance != null)
            {
                DecreaseStaminaRechargeDelay(desiredDelayMultiplier, desiredRateMultiplier);
                Hax2.Log1("Reapplied stamina settings after scene change.");
            }
        }

        public static void SetFlashlightIntensity(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
            if (playerAvatarScriptField != null)
            {
                var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                if (playerAvatarScriptInstance != null)
                {
                    var flashlightControllerField = playerAvatarScriptInstance.GetType().GetField("flashlightController", BindingFlags.Public | BindingFlags.Instance);
                    if (flashlightControllerField != null)
                    {
                        var flashlightControllerInstance = flashlightControllerField.GetValue(playerAvatarScriptInstance);
                        if (flashlightControllerInstance != null)
                        {
                            var baseIntensityField = flashlightControllerInstance.GetType().GetField("baseIntensity", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (baseIntensityField != null)
                            {
                                baseIntensityField.SetValue(flashlightControllerInstance, value);
                                Hax2.Log1($"Flashlight BaseIntensity set to {value}");
                            }
                        }
                    }
                }
            }
        }

        public static void SetCrouchDelay(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var crouchTimeMinField = playerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"CrouchTimeMin set to {value}");
            }
            else
            {
                Hax2.Log1("CrouchTimeMin field not found in PlayerController.");
            }
        }
        public static void SetCrouchSpeed(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var crouchTimeMinField = playerControllerType.GetField("CrouchSpeed", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"CrouchSpeed set to {value}");
            }
            else
            {
                Hax2.Log1("CrouchSpeed field not found in PlayerController.");
            }
        }

        public static void SetJumpForce(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var jumpForceField = playerControllerType.GetField("JumpForce", BindingFlags.Public | BindingFlags.Instance);
            if (jumpForceField != null)
            {
                jumpForceField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"JumpForce set to {value}");
            }
            else
            {
                Hax2.Log1("JumpForce field not found in PlayerController.");
            }
        }

        public static void SetExtraJumps(int value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var jumpExtraField = playerControllerType.GetField("JumpExtra", BindingFlags.NonPublic | BindingFlags.Instance);
            if (jumpExtraField != null)
            {
                jumpExtraField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"JumpExtra set to {value}");
            }
            else
            {
                Hax2.Log1("JumpExtra field not found in PlayerController.");
            }
        }

        public static void SetCustomGravity(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var customGravityField = playerControllerType.GetField("CustomGravity", BindingFlags.Public | BindingFlags.Instance);
            if (customGravityField != null)
            {
                customGravityField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"CustomGravity set to {value}");
            }
            else
            {
                Hax2.Log1("CustomGravity field not found in PlayerController.");
            }
        }

        public static void SetCrawlDelay(float crawlDelay)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            // Aplicar localmente
            var crouchTimeMinField = playerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, crawlDelay);
                Hax2.Log1($"CrouchTimeMin set locally to {crawlDelay}");
            }
            else
            {
                Hax2.Log1("CrouchTimeMin field not found in PlayerController.");
                return;
            }

            if (PhotonNetwork.IsConnected)
            {
                var photonViewField = playerControllerType.GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                PhotonView photonView = photonViewField != null ? (PhotonView)photonViewField.GetValue(playerControllerInstance) : null;

                if (photonView == null)
                {
                    var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                        photonView = playerAvatarScriptInstance?.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(playerAvatarScriptInstance) as PhotonView;
                    }
                }

                if (photonView != null)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("SetCrawlDelayRPC", RpcTarget.AllBuffered, crawlDelay);
                        Hax2.Log1($"Master Client set crawl delay to {crawlDelay} and synced via RPC.");
                    }
                    else
                    {
                        photonView.RPC("SetCrawlDelayRPC", RpcTarget.MasterClient, crawlDelay);
                        Hax2.Log1($"Requested Master Client to set crawl delay to {crawlDelay} via RPC.");
                    }
                }
                else
                {
                    Hax2.Log1("PhotonView not found for crawl delay synchronization.");
                }
            }
            else
            {
                Hax2.Log1("Not connected to Photon, crawl delay applied only locally.");
            }
        }

        public static void SetGrabRange(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScript = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerControllerInstance);
            if (playerAvatarScript == null) return;

            var physGrabber = playerAvatarScript.GetType().GetField("physGrabber", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerAvatarScript);
            if (physGrabber == null) return;

            var grabRangeField = physGrabber.GetType().GetField("grabRange", BindingFlags.Public | BindingFlags.Instance);
            if (grabRangeField != null)
            {
                grabRangeField.SetValue(physGrabber, value);
                Hax2.Log1($"GrabRange set to {value}");
            }
            else
            {
                Hax2.Log1("GrabRange field not found in PhysGrabber.");
            }
        }

        public static void SetThrowStrength(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScript = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerControllerInstance);
            if (playerAvatarScript == null) return;

            var physGrabber = playerAvatarScript.GetType().GetField("physGrabber", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerAvatarScript);
            if (physGrabber == null) return;

            var throwStrengthField = physGrabber.GetType().GetField("throwStrength", BindingFlags.Public | BindingFlags.Instance);
            if (throwStrengthField != null)
            {
                throwStrengthField.SetValue(physGrabber, value);
                Hax2.Log1($"ThrowStrength set to {value}");
            }
            else
            {
                Hax2.Log1("ThrowStrength field not found in PhysGrabber.");
            }
        }

        public static void SetSlideDecay(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var slideDecayField = playerControllerType.GetField("SlideDecay", BindingFlags.Public | BindingFlags.Instance);
            if (slideDecayField != null)
            {
                slideDecayField.SetValue(playerControllerInstance, value);
                Hax2.Log1($"SlideDecay set to {value}");
            }
            else
            {
                Hax2.Log1("SlideDecay field not found in PlayerController.");
            }
        }
    }
}