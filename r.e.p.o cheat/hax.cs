using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace r.e.p.o_cheat
{
    public static class UIHelper
    {
        private static Dictionary<Color, Texture2D> solidTextures = new Dictionary<Color, Texture2D>();
        private static float x, y, width, height, margin, controlHeight, controlDist, nextControlY;
        private static int columns = 1;
        private static int currentColumn = 0;
        private static int currentRow = 0;

        private static GUIStyle sliderStyle;
        private static GUIStyle thumbStyle;

        public static bool ButtonBool(string text, bool value, float? customX = null, float? customY = null)
        {
            Rect rect = NextControlRect(customX, customY);
            string displayText = $"{text} {(value ? "✔" : " ")}";
            GUIStyle style = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, normal = { textColor = value ? Color.green : Color.red } };
            return GUI.Button(rect, displayText, style) ? !value : value;
        }

        public static bool Checkbox(string text, bool value, float? customX = null, float? customY = null)
        {
            return GUI.Toggle(NextControlRect(customX, customY), value, text);
        }

        public static void Begin(string text, float _x, float _y, float _width, float _height, float InstructionHeight, float _controlHeight, float _controlDist)
        {
            x = _x; y = _y; width = _width; height = _height; margin = InstructionHeight; controlHeight = _controlHeight; controlDist = _controlDist;
            nextControlY = y + margin + 60;
            GUI.Box(new Rect(x, y, width, height), text);
            ResetGrid();
        }

        private static Rect NextControlRect(float? customX = null, float? customY = null)
        {
            float controlX = customX ?? (x + margin + currentColumn * ((width - (columns + 1) * margin) / columns));
            float controlY = customY ?? nextControlY;
            float controlWidth = customX == null ? ((width - (columns + 1) * margin) / columns) : width - 2 * margin;

            Rect rect = new Rect(controlX, controlY, controlWidth, controlHeight);

            if (customX == null && customY == null)
            {
                currentColumn++;
                if (currentColumn >= columns)
                {
                    currentColumn = 0;
                    currentRow++;
                    nextControlY += controlHeight + controlDist;
                }
            }

            return rect;
        }

        public static bool Button(string text, float? customX = null, float? customY = null)
        {
            return GUI.Button(NextControlRect(customX, customY), text);
        }

        public static bool Button(string text, float customX, float customY, float width, float height)
        {
            Rect rect = new Rect(customX, customY, width, height);
            return GUI.Button(rect, text);
        }

        public static void InitSliderStyles()
        {
            // Estilo personalizado para o slider
            if (sliderStyle == null)
            {
                sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
                {
                    normal = { background = MakeSolidBackground(new Color(0.7f, 0.7f, 0.7f), 1f) },
                    hover = { background = MakeSolidBackground(new Color(0.8f, 0.8f, 0.8f), 1f) },
                    active = { background = MakeSolidBackground(new Color(0.9f, 0.9f, 0.9f), 1f) }
                };
            }
            if (thumbStyle == null)
            {
                thumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
                {
                    normal = { background = MakeSolidBackground(Color.white, 1f) },
                    hover = { background = MakeSolidBackground(new Color(0.9f, 0.9f, 0.9f), 1f) },
                    active = { background = MakeSolidBackground(Color.green, 1f) }
                };
            }
        }

        public static string MakeEnable(string text, bool state) => $"{text}{(state ? "ON" : "OFF")}";
        public static void Label(string text, float? customX = null, float? customY = null) => GUI.Label(NextControlRect(customX, customY), text);
        public static float Slider(float val, float min, float max, float? customX = null, float? customY = null)
        {
            Rect rect = NextControlRect(customX, customY);

            return Mathf.Round(GUI.HorizontalSlider(rect, val, min, max, sliderStyle, thumbStyle));
        }
        private static Texture2D MakeSolidBackground(Color color, float alpha)
        {
            Color key = new Color(color.r, color.g, color.b, alpha);

            if (!solidTextures.ContainsKey(key))
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture.SetPixel(0, 0, key);
                texture.Apply();
                solidTextures[key] = texture;
            }
            return solidTextures[key];
        }

        public static void ResetGrid() { currentColumn = 0; currentRow = 0; nextControlY = y + margin + 60; }
    }

    public class Hax2 : MonoBehaviour
    {
        private float nextUpdateTime = 0f;
        private const float updateInterval = 10f;

        private int selectedPlayerIndex = 0;
        private List<string> playerNames = new List<string>();
        private List<object> playerList = new List<object>();
        private int selectedEnemyIndex = 0;
        private List<string> enemyNames = new List<string>();
        private List<Enemy> enemyList = new List<Enemy>();
        private float oldSliderValue = 0.5f;
        private float oldSliderValueStrength = 0.5f;
        private float sliderValue = 0.5f;
        public static float sliderValueStrength = 0.5f;
        public static float offsetESp = 0.5f;
        private bool showMenu = true;
        public static bool godModeActive = false;
        public static bool infiniteHealthActive = false;
        public static bool stamineState = false;
        private Vector2 playerScrollPosition = Vector2.zero;
        private Vector2 enemyScrollPosition = Vector2.zero;

        public static string[] levelsToSearchItems = { "Level - Manor", "Level - Wizard", "Level - Arctic" };

        private GUIStyle menuStyle;
        private bool initialized = false;
        private static Dictionary<Color, Texture2D> solidTextures = new Dictionary<Color, Texture2D>();

        private enum MenuCategory { Player, ESP, Combat, Misc, Enemies, Items, Hotkeys }
        private MenuCategory currentCategory = MenuCategory.Player;

        public static float staminaRechargeDelay = 1f;
        public static float staminaRechargeRate = 1f;
        public static float oldStaminaRechargeDelay = 1f;
        public static float oldStaminaRechargeRate = 1f;

        public static float jumpForce = 1f;
        public static float customGravity = 1f;
        public static int extraJumps = 1;
        public static float flashlightIntensity = 1f;
        public static float crouchDelay = 1f;
        public static float crouchSpeed = 1f;
        public static float grabRange = 1f;
        public static float throwStrength = 1f;
        public static float slideDecay = 1f;

        public static float OldflashlightIntensity = 1f;
        public static float OldcrouchDelay = 1f;
        public static float OldjumpForce = 1f;
        public static float OldcustomGravity = 1f;
        public static float OldextraJumps = 1f;
        public static float OldcrouchSpeed = 1f;
        public static float OldgrabRange = 1f;
        public static float OldthrowStrength = 1f;
        public static float OldslideDecay = 1f;

        private List<ItemTeleport.GameItem> itemList = new List<ItemTeleport.GameItem>();
        private int selectedItemIndex = 0;
        private Vector2 itemScrollPosition = Vector2.zero;
        private float lastItemListUpdateTime = 0f;
        private const float itemListUpdateInterval = 2f;
        private bool isDragging = false;
        private Vector2 dragOffset;
        private float menuX = 50f;
        private float menuY = 50f;
        private const float titleBarHeight = 30f;

        private Dictionary<KeyCode, Action> hotkeyBindings = new Dictionary<KeyCode, Action>();
        private List<HotkeyAction> availableActions = new List<HotkeyAction>();
        private int selectedHotkeySlot = 0;
        private bool configuringHotkey = false;
        private bool configuringSystemKey = false;
        private bool waitingForAnyKey = false;
        private bool showingActionSelector = false;
        private int systemKeyConfigIndex = -1; // 0=menu, 1=reload, 2=unload
        private KeyCode[] defaultHotkeys = new KeyCode[12];
        private Vector2 actionSelectorScroll = Vector2.zero;
        private Vector2 hotkeyScrollPosition = Vector2.zero;
        private int currentHotkeySlot = -1;
        private string keyAssignmentError = "";
        private float errorMessageTime = 0f;
        private const float ERROR_MESSAGE_DURATION = 3f; // how long to show duplicate hotkey error message
        private KeyCode currentHotkeyKey = KeyCode.None;
        private KeyCode menuToggleKey = KeyCode.Delete;
        private KeyCode reloadKey = KeyCode.F5;
        private KeyCode unloadKey = KeyCode.F10;

        private float actionSelectorX = 300f;
        private float actionSelectorY = 200f;
        private bool isDraggingActionSelector = false;
        private Vector2 dragOffsetActionSelector;
        private GUIStyle overlayDimStyle;
        private GUIStyle actionSelectorBoxStyle;
        private static bool cursorStateInitialized = false;


        public void Start()
        {
            UpdateCursorState();
            InitializeHotkeyActions();
            LoadHotkeySettings();

            DebugCheats.texture2 = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            DebugCheats.texture2.SetPixels(new[] { Color.red, Color.red, Color.red, Color.red });
            DebugCheats.texture2.Apply();

            var playerHealthType = Type.GetType("PlayerHealth, Assembly-CSharp");
            if (playerHealthType != null)
            {
                DLog.Log("playerHealthType não é null");
                Health_Player.playerHealthInstance = FindObjectOfType(playerHealthType);
                DLog.Log(Health_Player.playerHealthInstance != null ? "playerHealthInstance não é null" : "playerHealthInstance null");
            }
            else DLog.Log("playerHealthType null");

            var playerMaxHealth = Type.GetType("ItemUpgradePlayerHealth, Assembly-CSharp");
            if (playerMaxHealth != null)
            {
                Health_Player.playerMaxHealthInstance = FindObjectOfType(playerMaxHealth);
                DLog.Log("playerMaxHealth não é null");
            }
            else DLog.Log("playerMaxHealth null");
        }

        public void Update()
        {
            Strength.UpdateStrength();

            if (RunManager.instance.levelCurrent != null && levelsToSearchItems.Contains(RunManager.instance.levelCurrent.name))
            {
                // Limit update frequency to prevent lag
                if (Time.time >= nextUpdateTime)
                {
                    DebugCheats.UpdateEnemyList();
                    nextUpdateTime = Time.time + updateInterval;
                }

                // Reduce item list updates from every frame to every 5 seconds
                if (Time.time - lastItemListUpdateTime > 5f)
                {
                    UpdateItemList();
                    itemList = ItemTeleport.GetItemList();
                    lastItemListUpdateTime = Time.time;
                }
            }

            if (oldSliderValue != sliderValue)
            {
                PlayerController.RemoveSpeed(sliderValue);
                oldSliderValue = sliderValue;
            }

            if (oldSliderValueStrength != sliderValueStrength)
            {
                Strength.MaxStrength();
                oldSliderValueStrength = sliderValueStrength;
            }

            if (playerColor.isRandomizing)
            {
                playerColor.colorRandomizer();
            }

            // Prevent excessive logging by adding a cooldown
            if (Time.time - lastItemListUpdateTime > 10f)
            {
                DLog.Log($"Item list contains {itemList.Count} items.");
                lastItemListUpdateTime = Time.time;
            }

            if (Input.GetKeyDown(menuToggleKey))
            {
                showMenu = !showMenu;
                DLog.Log("MENU " + showMenu);
                if (!showMenu) TryUnlockCamera();
                UpdateCursorState();
            }

            if (Input.GetKeyDown(reloadKey)) Start();

            if (Input.GetKeyDown(unloadKey))
            {
                showMenu = false;
                TryUnlockCamera();
                UpdateCursorState();
                Loader.UnloadCheat();
            }


            if (configuringHotkey)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key) && key != KeyCode.Escape)
                    {
                        if (key == menuToggleKey || key == reloadKey || key == unloadKey)
                        {
                            keyAssignmentError = $"Cannot assign {key} as hotkey - it's already used as a system key!";
                            errorMessageTime = Time.time;
                            DLog.Log(keyAssignmentError);
                            configuringHotkey = false;
                        }
                        else if (hotkeyBindings.ContainsKey(key))
                        {
                            keyAssignmentError = $"Cannot assign {key} as hotkey - it's already used for another action!";
                            errorMessageTime = Time.time;
                            DLog.Log(keyAssignmentError);
                            configuringHotkey = false;
                        }
                        else
                        {
                            KeyCode oldKey = defaultHotkeys[selectedHotkeySlot];
                            if (oldKey != KeyCode.None && hotkeyBindings.ContainsKey(oldKey))
                            {
                                Action oldAction = hotkeyBindings[oldKey];
                                hotkeyBindings.Remove(oldKey);

                                hotkeyBindings[key] = oldAction;
                            }
                            else
                            {
                                hotkeyBindings[key] = null;
                            }

                            defaultHotkeys[selectedHotkeySlot] = key;

                            DLog.Log($"Hotkey set to: {key}");
                            configuringHotkey = false;
                        }
                        break;
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        configuringHotkey = false;
                        DLog.Log("Hotkey configuration canceled");
                        break;
                    }
                }
            }
            if (configuringSystemKey)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key) && key != KeyCode.Escape)
                    {
                        bool isUsed = false;
                        string conflictType = "";
                        waitingForAnyKey = false;

                        if (key == menuToggleKey && systemKeyConfigIndex != 0)
                        {
                            isUsed = true;
                            conflictType = "Menu Toggle";
                        }
                        else if (key == reloadKey && systemKeyConfigIndex != 1)
                        {
                            isUsed = true;
                            conflictType = "Reload";
                        }
                        else if (key == unloadKey && systemKeyConfigIndex != 2)
                        {
                            isUsed = true;
                            conflictType = "Unload";
                        }
                        else if (hotkeyBindings.ContainsKey(key))
                        {
                            isUsed = true;
                            conflictType = "action hotkey";
                        }

                        if (isUsed)
                        {
                            keyAssignmentError = $"Cannot assign {key} - already used as {conflictType}!";
                            errorMessageTime = Time.time;
                            DLog.Log(keyAssignmentError);
                            configuringSystemKey = false;
                        }
                        else
                        {
                            switch (systemKeyConfigIndex)
                            {
                                case 0: menuToggleKey = key; break;
                                case 1: reloadKey = key; break;
                                case 2: unloadKey = key; break;
                            }
                            DLog.Log($"{GetSystemKeyName(systemKeyConfigIndex)} key set to: {key}");
                            configuringSystemKey = false;
                            SaveHotkeySettings();
                        }
                        break;
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        configuringSystemKey = false;
                        DLog.Log($"{GetSystemKeyName(systemKeyConfigIndex)} key configuration canceled");
                        break;
                    }
                }
            }
            else
            {
                foreach (var kvp in hotkeyBindings)
                {
                    if (Input.GetKeyDown(kvp.Key) && kvp.Value != null)
                    {
                        kvp.Value.Invoke();
                        break;
                    }
                }
            }

            if (showMenu) TryLockCamera();

            if (NoclipController.noclipActive)
            {
                NoclipController.UpdateMovement();
            }
        }

        private void TryLockCamera()
        {
            if (InputManager.instance != null)
            {
                Type type = typeof(InputManager);
                FieldInfo field = type.GetField("disableAimingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    float currentValue = (float)field.GetValue(InputManager.instance);
                    if (currentValue < 2f || currentValue > 10f)
                    {
                        float clampedValue = Mathf.Clamp(currentValue, 2f, 10f);
                        field.SetValue(InputManager.instance, clampedValue);
                    }
                }
                else DLog.LogError("Failed to find field disableAimingTimer.");
            }
            else DLog.LogWarning("InputManager.instance not found!");
        }

        private void TryUnlockCamera()
        {
            if (InputManager.instance != null)
            {
                Type type = typeof(InputManager);
                FieldInfo field = type.GetField("disableAimingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    float currentValue = (float)field.GetValue(InputManager.instance);
                    field.SetValue(InputManager.instance, 0f);
                    DLog.Log("disableAimingTimer reset to 0 (menu closed).");
                }
                else DLog.LogError("Failed to find field disableAimingTimer.");
            }
            else DLog.LogWarning("InputManager.instance not found!");
        }

        private void UpdateCursorState()
        {
            Cursor.visible = showMenu;
            Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void UpdateItemList()
        {
            DebugCheats.valuableObjects.Clear();

            var valuableArray = UnityEngine.Object.FindObjectsOfType(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableArray != null)
            {
                DebugCheats.valuableObjects.AddRange(valuableArray);
            }

            var playerDeathHeadArray = UnityEngine.Object.FindObjectsOfType(Type.GetType("PlayerDeathHead, Assembly-CSharp"));
            if (playerDeathHeadArray != null)
            {
                DebugCheats.valuableObjects.AddRange(playerDeathHeadArray);
            }

            itemList = ItemTeleport.GetItemList();
            DLog.Log($"Lista de itens atualizada: {itemList.Count} itens encontrados (incluindo ValuableObject e PlayerDeathHead).");
        }

        private void UpdateEnemyList()
        {
            enemyNames.Clear();
            enemyList.Clear();

            DebugCheats.UpdateEnemyList();
            enemyList = DebugCheats.enemyList;

            foreach (var enemy in enemyList)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    string enemyName = "Enemy";
                    var enemyParent = enemy.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                    if (enemyParent != null)
                    {
                        var nameField = enemyParent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                        enemyName = nameField?.GetValue(enemyParent) as string ?? "Enemy";
                    }

                    int health = GetEnemyHealth(enemy);
                    string healthText = health >= 0 ? $"HP: {health}" : "HP: Unknown";
                    enemyNames.Add($"{enemyName} [{healthText}]");
                }
            }

            if (enemyNames.Count == 0) enemyNames.Add("No enemies found");
        }

        private void TeleportEnemyToMe()
        {
            if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
            {
                DLog.Log($"Índice de inimigo inválido! selectedEnemyIndex={selectedEnemyIndex}, enemyList.Count={enemyList.Count}");
                return;
            }

            var selectedEnemy = enemyList[selectedEnemyIndex];
            if (selectedEnemy == null)
            {
                DLog.Log("Inimigo selecionado é nulo!");
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

                Vector3 forwardDirection = localPlayer.transform.forward;
                Vector3 targetPosition = localPlayer.transform.position + forwardDirection * 1f + Vector3.up * 1.5f;

                var photonView = selectedEnemy.GetComponent<PhotonView>();
                if (PhotonNetwork.IsConnected && photonView != null && !photonView.IsMine)
                {
                    photonView.RequestOwnership();
                    DLog.Log($"Solicitada posse do inimigo {enemyNames[selectedEnemyIndex]} para garantir controle local.");
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
                            DLog.Log($"NavMeshAgent de {enemyNames[selectedEnemyIndex]} desativado para evitar movimento imediato.");
                        }
                    }
                }

                selectedEnemy.transform.position = targetPosition;
                DLog.Log($"Inimigo {enemyNames[selectedEnemyIndex]} teleportado localmente para {targetPosition}");

                Vector3 currentPosition = selectedEnemy.transform.position;
                DLog.Log($"Posição atual do inimigo após teleporte: {currentPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var enemyType = selectedEnemy.GetType();
                    var teleportMethod = enemyType.GetMethod("EnemyTeleported", BindingFlags.Public | BindingFlags.Instance);
                    if (teleportMethod != null)
                    {
                        teleportMethod.Invoke(selectedEnemy, new object[] { targetPosition });
                        DLog.Log($"Inimigo {enemyNames[selectedEnemyIndex]} teleportado via EnemyTeleported para sincronização multiplayer.");
                    }
                    else
                    {
                        DLog.Log("Método 'EnemyTeleported' não encontrado, sincronização pode não ocorrer.");
                    }
                }

                if (navMeshAgent != null) StartCoroutine(ReEnableNavMeshAgent(navMeshAgent, 2f));

                var enemyGameObject = selectedEnemy.GetComponent<GameObject>();
                if (enemyGameObject == null) enemyGameObject = ((MonoBehaviour)selectedEnemy).gameObject;
                if (enemyGameObject != null)
                {
                    enemyGameObject.SetActive(false);
                    enemyGameObject.SetActive(true);
                    DLog.Log($"Inimigo {enemyNames[selectedEnemyIndex]} reativado para forçar renderização.");
                }
                else
                {
                    DLog.Log($"GameObject do inimigo {enemyNames[selectedEnemyIndex]} não encontrado para re-renderização.");
                }

                UpdateEnemyList();
                DLog.Log($"Teleporte de {enemyNames[selectedEnemyIndex]} concluído.");
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao teleportar inimigo {enemyNames[selectedEnemyIndex]}: {e.Message}");
            }
        }

        private System.Collections.IEnumerator ReEnableNavMeshAgent(object navMeshAgent, float delay)
        {
            yield return new WaitForSeconds(delay);
            var enabledProperty = navMeshAgent.GetType().GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
            if (enabledProperty != null)
            {
                enabledProperty.SetValue(navMeshAgent, true);
                DLog.Log("NavMeshAgent reativado após teleporte.");
            }
        }

        private void KillSelectedEnemy()
        {
            if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
            {
                DLog.Log("Índice de inimigo inválido!");
                return;
            }

            var selectedEnemy = enemyList[selectedEnemyIndex];
            if (selectedEnemy == null)
            {
                DLog.Log("Inimigo selecionado é nulo!");
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
                            DLog.Log($"Inimigo {enemyNames[selectedEnemyIndex]} ferido com 9999 de dano via Hurt");
                        }
                        else
                            DLog.Log("Método 'Hurt' não encontrado em EnemyHealth");
                    }
                    else
                        DLog.Log("Componente EnemyHealth é nulo");
                }
                else
                    DLog.Log("Campo 'Health' não encontrado em Enemy");

                UpdateEnemyList();
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao matar inimigo {enemyNames[selectedEnemyIndex]}: {e.Message}");
            }
        }

        private void ActionSelectorWindow(int windowID)
        {
            if (GUI.Button(new Rect(370, 5, 20, 20), "X"))
            {
                showingActionSelector = false;
            }

            GUI.DragWindow(new Rect(0, 0, 400, 30));

            Rect scrollViewRect = new Rect(10, 35, 380, 355);
            Rect contentRect = new Rect(0, 0, 360, availableActions.Count * 35);
            actionSelectorScroll = GUI.BeginScrollView(scrollViewRect, actionSelectorScroll, contentRect);

            for (int i = 0; i < availableActions.Count; i++)
            {
                Rect actionRect = new Rect(0, i * 35, 340, 30);
                if (GUI.Button(actionRect, availableActions[i].Name))
                {
                    hotkeyBindings[currentHotkeyKey] = availableActions[i].Action;
                    DLog.Log("assigned " + availableActions[i].Name + " to " + currentHotkeyKey);
                    showingActionSelector = false;
                    SaveHotkeySettings();
                }

                if (actionRect.Contains(Event.current.mousePosition))
                {
                    Rect tooltipRect = new Rect(Event.current.mousePosition.x + 15, Event.current.mousePosition.y, 200, 30);
                    GUI.Label(tooltipRect, availableActions[i].Description);
                }
            }

            GUI.EndScrollView();
        }

        private void SaveHotkeySettings()
        {
            PlayerPrefs.SetInt("MenuToggleKey", (int)menuToggleKey);
            PlayerPrefs.SetInt("ReloadKey", (int)reloadKey);
            PlayerPrefs.SetInt("UnloadKey", (int)unloadKey);

            for (int i = 0; i < defaultHotkeys.Length; i++)
            {
                PlayerPrefs.SetInt($"HotkeyKey_{i}", (int)defaultHotkeys[i]);

                int actionIndex = -1;
                if (defaultHotkeys[i] != KeyCode.None && hotkeyBindings.ContainsKey(defaultHotkeys[i]))
                {
                    var action = hotkeyBindings[defaultHotkeys[i]];
                    if (action != null)
                    {
                        for (int j = 0; j < availableActions.Count; j++)
                        {
                            if (availableActions[j].Action == action)
                            {
                                actionIndex = j;
                                break;
                            }
                        }
                    }
                }
                PlayerPrefs.SetInt($"HotkeyAction_{i}", actionIndex);
            }

            PlayerPrefs.Save();
            DLog.Log("Hotkey settings saved");
        }

        private void LoadHotkeySettings()
        {
            menuToggleKey = (KeyCode)PlayerPrefs.GetInt("MenuToggleKey", (int)KeyCode.Delete);
            reloadKey = (KeyCode)PlayerPrefs.GetInt("ReloadKey", (int)KeyCode.F5);
            unloadKey = (KeyCode)PlayerPrefs.GetInt("UnloadKey", (int)KeyCode.F10);

            hotkeyBindings.Clear();

            for (int i = 0; i < defaultHotkeys.Length; i++)
            {
                defaultHotkeys[i] = (KeyCode)PlayerPrefs.GetInt($"HotkeyKey_{i}", (int)KeyCode.None);
                int actionIndex = PlayerPrefs.GetInt($"HotkeyAction_{i}", -1);

                if (defaultHotkeys[i] != KeyCode.None && actionIndex >= 0 && actionIndex < availableActions.Count)
                {
                    hotkeyBindings[defaultHotkeys[i]] = availableActions[actionIndex].Action;
                }
            }

            DLog.Log("Hotkey settings loaded");
        }

        private class HotkeyAction
        {
            public string Name { get; set; }
            public Action Action { get; set; }
            public string Description { get; set; }
            public HotkeyAction(string name, Action action, string description = "")
            {
                Name = name;
                Action = action;
                Description = description;
            }
        }

        private void StartConfigureSystemKey(int index)
        {
            configuringSystemKey = true;
            systemKeyConfigIndex = index;
            waitingForAnyKey = true;
            DLog.Log($"Press any key to set {GetSystemKeyName(index)}...");
        }

        private string GetSystemKeyName(int index)
        {
            switch (index)
            {
                case 0: return "Menu Toggle";
                case 1: return "Reload";
                case 2: return "Unload";
                default: return "Unknown";
            }
        }

        private void ShowActionSelector(int slotIndex, KeyCode key)
        {
            showingActionSelector = true;
            currentHotkeySlot = slotIndex;
            currentHotkeyKey = key;
        }

        private void InitializeHotkeyActions()
        {
            availableActions.Add(new HotkeyAction("God Mode", () =>
            {
                bool newGodModeState = !godModeActive;
                PlayerController.GodMode();
                godModeActive = newGodModeState;
                DLog.Log("god mode toggled: " + godModeActive);
            }, "toggles god mode on/off"));

            availableActions.Add(new HotkeyAction("Noclip Toggle", () =>
            {
                bool newNoclipState = !NoclipController.noclipActive;
                NoclipController.ToggleNoclip();
                NoclipController.noclipActive = newNoclipState;
                DLog.Log("Noclip toggled: " + NoclipController.noclipActive);
            }, "Toggles noclip on/off"));

            availableActions.Add(new HotkeyAction("Infinite Health", () =>
            {
                bool newHealState = !infiniteHealthActive;
                infiniteHealthActive = newHealState;
                Health_Player.MaxHealth();
                DLog.Log("infinite health toggled: " + infiniteHealthActive);
            }, "toggles infinite health on/off"));

            availableActions.Add(new HotkeyAction("Infinite Stamina", () =>
            {
                bool newStaminaState = !stamineState;
                stamineState = newStaminaState;
                PlayerController.MaxStamina();
                DLog.Log("infinite stamina toggled: " + stamineState);
            }, "toggles infinite stamina on/off"));

            availableActions.Add(new HotkeyAction("RGB Player", () =>
            {
                playerColor.isRandomizing = !playerColor.isRandomizing;
                DLog.Log("rgb player toggled: " + playerColor.isRandomizing);
            }, "toggles rgb player effect"));

            availableActions.Add(new HotkeyAction("Spawn Money", () =>
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer != null)
                {
                    Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                    transform.position = targetPosition;
                    ItemSpawner.SpawnItem(targetPosition);
                    DLog.Log("money spawned.");
                }
                else
                {
                    DLog.Log("local player not found!");
                }
            }, "spawns money at your position"));

            availableActions.Add(new HotkeyAction("Kill All Enemies", () =>
            {
                DebugCheats.KillAllEnemies();
                DLog.Log("all enemies killed.");
            }, "kills all enemies on the map"));

            availableActions.Add(new HotkeyAction("Enemy ESP Toggle", () =>
            {
                DebugCheats.drawEspBool = !DebugCheats.drawEspBool;
                DLog.Log("enemy esp toggled: " + DebugCheats.drawEspBool);
            }, "toggles enemy esp on/off"));

            availableActions.Add(new HotkeyAction("Item ESP Toggle", () =>
            {
                DebugCheats.drawItemEspBool = !DebugCheats.drawItemEspBool;
                DLog.Log("item esp toggled: " + DebugCheats.drawItemEspBool);
            }, "toggles item esp on/off"));

            availableActions.Add(new HotkeyAction("Player ESP Toggle", () =>
            {
                DebugCheats.drawPlayerEspBool = !DebugCheats.drawPlayerEspBool;
                DLog.Log("player esp toggled: " + DebugCheats.drawPlayerEspBool);
            }, "toggles player esp on/off"));

            availableActions.Add(new HotkeyAction("Heal Self", () =>
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer != null)
                {
                    Health_Player.HealPlayer(localPlayer, 100, "Self");
                    DLog.Log("healed self by 100 hp.");
                }
                else
                {
                    DLog.Log("local player not found!");
                }
            }, "heals yourself by 100 hp"));

            availableActions.Add(new HotkeyAction("Max Speed", () =>
            {
                sliderValue = 30f;
                PlayerController.RemoveSpeed(sliderValue);
                DLog.Log("speed set to maximum (30)");
            }, "sets speed to maximum value"));

            availableActions.Add(new HotkeyAction("Normal Speed", () =>
            {
                sliderValue = 5f;
                PlayerController.RemoveSpeed(sliderValue);
                DLog.Log("speed set to normal (5)");
            }, "sets speed to normal value"));

            for (int i = 0; i < defaultHotkeys.Length; i++)
            {
                defaultHotkeys[i] = KeyCode.None;
            }
        }

        private int GetEnemyHealth(Enemy enemy)
        {
            try
            {
                var healthField = enemy.GetType().GetField("Health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthField == null) return -1;

                var healthComponent = healthField.GetValue(enemy);
                if (healthComponent == null) return -1;

                var healthValueField = healthComponent.GetType().GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthValueField == null) return -1;

                return (int)healthValueField.GetValue(healthComponent);
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao obter vida do inimigo: {e.Message}");
                return -1;
            }
        }

        private void UpdatePlayerList()
        {
            var fakePlayers = playerNames.Where(name => name.Contains("FakePlayer")).ToList();
            var fakePlayerCount = fakePlayers.Count;

            playerNames.Clear();
            playerList.Clear();

            var players = SemiFunc.PlayerGetList();
            foreach (var player in players)
            {
                playerList.Add(player);
                string baseName = SemiFunc.PlayerGetName(player) ?? "Unknown Player";
                bool isAlive = IsPlayerAlive(player, baseName);
                string statusText = isAlive ? "<color=green>[LIVE]</color> " : "<color=red>[DEAD]</color> ";
                playerNames.Add(statusText + baseName);
            }

            for (int i = 0; i < fakePlayerCount; i++)
            {
                playerNames.Add(fakePlayers[i]);
                playerList.Add(null);
            }

            if (playerNames.Count == 0) playerNames.Add("No player Found");
        }

        private void AddFakePlayer()
        {
            int fakePlayerId = playerNames.Count(name => name.Contains("FakePlayer")) + 1;
            string fakeName = $"<color=green>[LIVE]</color> FakePlayer{fakePlayerId}";
            playerNames.Add(fakeName);
            playerList.Add(null);
            DLog.Log($"Added fake player: {fakeName}");
        }

        private bool IsPlayerAlive(object player, string playerName)
        {
            try
            {
                var playerHealthField = player.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) return true;

                var playerHealthInstance = playerHealthField.GetValue(player);
                if (playerHealthInstance == null) return true;

                var healthField = playerHealthInstance.GetType().GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthField == null) return true;

                int health = (int)healthField.GetValue(playerHealthInstance);
                return health > 0;
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao verificar vida de {playerName}: {e.Message}");
                return true;
            }
        }

        private void ReviveSelectedPlayer()
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                DLog.Log("Índice de jogador inválido!");
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
                            DLog.Log("Campo 'inExtractionPoint' definido como true.");
                            DLog.Log("'inExtractionPoint' field set to true.");
                        }
                        if (reviveMethod != null)
                        {
                            reviveMethod.Invoke(playerDeathHeadInstance, null);
                            DLog.Log("'Revive' method successfully called for: " + playerNames[selectedPlayerIndex]);
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
                            Health_Player.HealPlayer(selectedPlayer, maxHealth, playerNames[selectedPlayerIndex]);
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
                DLog.Log($"Error reviving and healing {playerNames[selectedPlayerIndex]}: {e.Message}");
            }
        }

        private void KillSelectedPlayer()
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count) { DLog.Log("Índice de jogador inválido!"); return; }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null) { DLog.Log("Jogador selecionado é nulo!"); return; }
            try
            {
                DLog.Log($"Tentando matar: {playerNames[selectedPlayerIndex]} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField não encontrado!"); return; }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView não é válido!"); return; }
                var playerHealthField = selectedPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("Campo 'playerHealth' não encontrado!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(selectedPlayer);
                if (playerHealthInstance == null) { DLog.Log("Instância de playerHealth é nula!"); return; }
                var healthType = playerHealthInstance.GetType();
                var deathMethod = healthType.GetMethod("Death", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (deathMethod == null) { DLog.Log("Método 'Death' não encontrado!"); return; }
                deathMethod.Invoke(playerHealthInstance, null);
                DLog.Log($"Método 'Death' chamado localmente para {playerNames[selectedPlayerIndex]}.");

                var playerAvatarField = healthType.GetField("playerAvatar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerAvatarField != null)
                {
                    var playerAvatarInstance = playerAvatarField.GetValue(playerHealthInstance);
                    if (playerAvatarInstance != null)
                    {
                        var playerAvatarType = playerAvatarInstance.GetType();
                        var playerDeathMethod = playerAvatarType.GetMethod("PlayerDeath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (playerDeathMethod != null) { playerDeathMethod.Invoke(playerAvatarInstance, new object[] { -1 }); DLog.Log($"Método 'PlayerDeath' chamado localmente para {playerNames[selectedPlayerIndex]}."); }
                        else DLog.Log("Método 'PlayerDeath' não encontrado em PlayerAvatar!");
                    }
                    else DLog.Log("Instância de PlayerAvatar é nula!");
                }
                else DLog.Log("Campo 'playerAvatar' não encontrado em PlayerHealth!");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
                    DLog.Log(maxHealthField != null ? $"maxHealth encontrado: {maxHealth}" : "Campo 'maxHealth' não encontrado, usando valor padrão: 100");
                    photonView.RPC("UpdateHealthRPC", RpcTarget.AllBuffered, new object[] { 0, maxHealth, true });
                    DLog.Log($"RPC 'UpdateHealthRPC' enviado para todos com saúde=0, maxHealth={maxHealth}, effect=true.");
                    try { photonView.RPC("PlayerDeathRPC", RpcTarget.AllBuffered, new object[] { -1 }); DLog.Log("Tentando RPC 'PlayerDeathRPC' para forçar morte..."); }
                    catch { DLog.Log("RPC 'PlayerDeathRPC' não registrado, tentando alternativa..."); }
                    photonView.RPC("HurtOtherRPC", RpcTarget.AllBuffered, new object[] { 9999, Vector3.zero, false, -1 });
                    DLog.Log("RPC 'HurtOtherRPC' enviado com 9999 de dano para garantir morte.");
                }
                else DLog.Log("Não conectado ao Photon, morte apenas local.");
                DLog.Log($"Tentativa de matar {playerNames[selectedPlayerIndex]} concluída.");
            }
            catch (Exception e) { DLog.Log($"Erro ao tentar matar {playerNames[selectedPlayerIndex]}: {e.Message}"); }
        }

        private void SendSelectedPlayerToVoid()
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
                DLog.Log($"Tentando enviar {playerNames[selectedPlayerIndex]} para o void | MasterClient: {PhotonNetwork.IsMasterClient}");

                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    DLog.Log("PhotonViewField não encontrado!");
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
                    DLog.Log("Transform é nulo!");
                    return;
                }

                Vector3 voidPosition = new Vector3(0, -10, 0);
                transform.position = voidPosition;
                DLog.Log($"Jogador {playerNames[selectedPlayerIndex]} enviado localmente para o void: {voidPosition}");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { voidPosition, transform.rotation });
                    DLog.Log($"RPC 'SpawnRPC' enviado para todos com posição: {voidPosition}");
                }
                else
                {
                    DLog.Log("Não conectado ao Photon, teleporte apenas local.");
                }
            }
            catch (Exception e)
            {
                DLog.Log($"Erro ao enviar {playerNames[selectedPlayerIndex]} para o void: {e.Message}");
            }
        }

        private void InitializeGUIStyles()
        {
            menuStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeSolidBackground(new Color(0.21f, 0.21f, 0.21f), 0.7f) },
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 10, 10),
                border = new RectOffset(5, 5, 5, 5)
            };

            overlayDimStyle = new GUIStyle();
            overlayDimStyle.normal.background = MakeSolidBackground(new Color(0f, 0f, 0f, 0.5f), 0.5f);

            actionSelectorBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeSolidBackground(new Color(0.25f, 0.25f, 0.25f), 0.95f) },
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 10, 10)
            };

            UIHelper.InitSliderStyles();
        }

        public void OnGUI()
        {
            if (!initialized)
            {
                InitializeGUIStyles();
                initialized = true;
            }

            UIHelper.InitSliderStyles();

            if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool || DebugCheats.draw3DPlayerEspBool || DebugCheats.draw3DItemEspBool || DebugCheats.drawChamsBool) DebugCheats.DrawESP();

            GUI.Label(new Rect(10, 10, 200, 30), $"D.A.R.K CHEAT | {menuToggleKey} - MENU");
            GUI.Label(new Rect(230, 10, 200, 30), "MADE BY Github/D4rkks");

            // handle modal first
            if (showingActionSelector)
            {
                Rect fullOverlay = new Rect(0, 0, Screen.width, Screen.height);
                GUI.Box(fullOverlay, "", overlayDimStyle);

                Rect modalRect = new Rect(actionSelectorX, actionSelectorY, 400, 400);

                // trying to only block events that are outside the modal window
                if (Event.current.type == EventType.MouseDown ||
                    Event.current.type == EventType.MouseUp ||
                    Event.current.type == EventType.MouseDrag)
                {
                    if (!modalRect.Contains(Event.current.mousePosition))
                    {
                        // blocking outer event
                        Event.current.Use();
                    }


                    modalRect = GUI.Window(12345, modalRect, ActionSelectorWindow, "", actionSelectorBoxStyle);
                    actionSelectorX = modalRect.x;
                    actionSelectorY = modalRect.y;
                }

                GUI.depth = 0;

                if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                {
                    return;
                }
            }

            if (showMenu)
            {
                GUIStyle overlayStyle = new GUIStyle();
                overlayStyle.normal.background = MakeSolidBackground(Color.clear, 0f);
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, overlayStyle);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                Rect menuRect = new Rect(menuX, menuY, 600, 730);
                Rect titleRect = new Rect(menuX, menuY, 600, titleBarHeight);

                GUI.Box(menuRect, "", menuStyle);
                UIHelper.Begin("D.A.R.K. Menu 1.1.2.2", menuX, menuY, 600, 800, 30, 30, 10);

                if (Event.current.type == EventType.MouseDown && titleRect.Contains(Event.current.mousePosition))
                {
                    isDragging = true;
                    dragOffset = Event.current.mousePosition - new Vector2(menuX, menuY);
                }
                if (Event.current.type == EventType.MouseUp) isDragging = false;
                if (isDragging && Event.current.type == EventType.MouseDrag)
                {
                    Vector2 newPosition = Event.current.mousePosition - dragOffset;
                    menuX = Mathf.Clamp(newPosition.x, 0, Screen.width - 600);
                    menuY = Mathf.Clamp(newPosition.y, 0, Screen.height - 730);
                }

                float tabWidth = 75f;
                float tabHeight = 40f;
                float spacing = 5f;
                float totalWidth = 7 * tabWidth + 6 * spacing;
                float startX = menuX + (600 - totalWidth) / 2f;

                GUIStyle tabStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white, background = MakeSolidBackground(Color.gray, 1f) },
                    hover = { textColor = Color.yellow, background = MakeSolidBackground(new Color(0.2f, 0.2f, 0.2f), 1f) },
                    active = { textColor = Color.green, background = MakeSolidBackground(Color.black, 1f) }
                };
                GUIStyle selectedTabStyle = new GUIStyle(tabStyle)
                {
                    normal = { textColor = Color.white, background = MakeSolidBackground(new Color(0.35f, 0.35f, 0.35f), 1f) }
                };

                if (GUI.Button(new Rect(startX, menuY + 30, tabWidth, tabHeight), "Player", currentCategory == MenuCategory.Player ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Player;
                if (GUI.Button(new Rect(startX + (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "ESP", currentCategory == MenuCategory.ESP ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.ESP;
                if (GUI.Button(new Rect(startX + 2 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Combat", currentCategory == MenuCategory.Combat ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Combat;
                if (GUI.Button(new Rect(startX + 3 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Misc", currentCategory == MenuCategory.Misc ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Misc;
                if (GUI.Button(new Rect(startX + 4 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Enemies", currentCategory == MenuCategory.Enemies ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Enemies;
                if (GUI.Button(new Rect(startX + 5 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Items", currentCategory == MenuCategory.Items ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Items;
                if (GUI.Button(new Rect(startX + 6 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Hotkeys", currentCategory == MenuCategory.Hotkeys ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Hotkeys;

                GUIStyle instructionStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = Color.white } };
                GUI.Label(new Rect(menuX + 10, menuY + 75, 580, 20), "Press F5 to reload! Press DEL to close! Press F10 to unload!", instructionStyle);

                switch (currentCategory)
                {
                    case MenuCategory.Player:
                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", menuX + 30, menuY + 95);
                        playerScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 150), playerScrollPosition, new Rect(0, 0, 520, playerNames.Count * 35), false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            if (i == selectedPlayerIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), playerNames[i])) selectedPlayerIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();

                        if (UIHelper.Button("Heal Player", menuX + 30, menuY + 275))
                        {
                            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                            {
                                Health_Player.HealPlayer(playerList[selectedPlayerIndex], 50, playerNames[selectedPlayerIndex]);
                                DLog.Log($"Player {playerNames[selectedPlayerIndex]} healed.");
                            }
                            else
                            {
                                DLog.Log("Nenhum jogador válido selecionado para curar!");
                            }
                        }
                        if (UIHelper.Button("Damage Player", menuX + 30, menuY + 315))
                        {
                            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                            {
                                Health_Player.DamagePlayer(playerList[selectedPlayerIndex], 1, playerNames[selectedPlayerIndex]);
                                DLog.Log($"Player {playerNames[selectedPlayerIndex]} damaged.");
                            }
                            else
                            {
                                DLog.Log("Nenhum jogador válido selecionado para causar dano!");
                            }
                        }
                        bool newHealState = UIHelper.ButtonBool("Toggle Infinite Health", infiniteHealthActive, menuX + 30, menuY + 355);
                        if (newHealState != infiniteHealthActive) { infiniteHealthActive = newHealState; Health_Player.MaxHealth(); }
                        bool newStaminaState = UIHelper.ButtonBool("Toggle Infinite Stamina", stamineState, menuX + 30, menuY + 395);
                        if (newStaminaState != stamineState) { stamineState = newStaminaState; PlayerController.MaxStamina(); DLog.Log("God mode toggled: " + stamineState); }
                        bool newGodModeState = UIHelper.ButtonBool("Toggle God Mode", godModeActive, menuX + 30, menuY + 435);
                        if (newGodModeState != godModeActive) { PlayerController.GodMode(); godModeActive = newGodModeState; DLog.Log("God mode toggled: " + godModeActive); }

                        bool newNoclipActive = UIHelper.ButtonBool("Toggle Noclip", NoclipController.noclipActive, menuX + 30, menuY + 475);
                        if (newNoclipActive != NoclipController.noclipActive) { NoclipController.ToggleNoclip(); }

                        UIHelper.Label("Speed Value " + sliderValue, menuX + 30, menuY + 515);

                        oldSliderValue = sliderValue;
                        sliderValue = UIHelper.Slider(sliderValue, 1f, 30f, menuX + 30, menuY + 535);

                        UIHelper.Label("Strength Value: " + sliderValueStrength, menuX + 30, menuY + 555);
                        oldSliderValueStrength = sliderValueStrength;
                        sliderValueStrength = UIHelper.Slider(sliderValueStrength, 1f, 100f, menuX + 30, menuY + 575);

                        UIHelper.Label("Stamina Recharge Delay: " + Hax2.staminaRechargeDelay, menuX + 30, menuY + 605);
                        Hax2.staminaRechargeDelay = UIHelper.Slider(Hax2.staminaRechargeDelay, 0f, 10f, menuX + 30, menuY + 626);

                        UIHelper.Label("Stamina Recharge Rate: " + Hax2.staminaRechargeRate, menuX + 30, menuY + 645);
                        Hax2.staminaRechargeRate = UIHelper.Slider(Hax2.staminaRechargeRate, 1f, 20f, menuX + 30, menuY + 665);

                        if (Hax2.staminaRechargeDelay != oldStaminaRechargeDelay || Hax2.staminaRechargeRate != oldStaminaRechargeRate)
                        {
                            PlayerController.DecreaseStaminaRechargeDelay(Hax2.staminaRechargeDelay, Hax2.staminaRechargeRate);
                            DLog.Log($"Stamina recharge updated: Delay={Hax2.staminaRechargeDelay}x, Rate={Hax2.staminaRechargeRate}x");
                            oldStaminaRechargeDelay = Hax2.staminaRechargeDelay;
                            oldStaminaRechargeRate = Hax2.staminaRechargeRate;
                        }
                        break;

                    case MenuCategory.ESP:
                        DebugCheats.drawEspBool = UIHelper.Checkbox("Enemy ESP", DebugCheats.drawEspBool, menuX + 30, menuY + 105);
                        if (DebugCheats.drawEspBool)
                        {
                            DebugCheats.showEnemyNames = UIHelper.Checkbox("Show Enemy Names", DebugCheats.showEnemyNames, menuX + 50, menuY + 125);
                            DebugCheats.showEnemyDistance = UIHelper.Checkbox("Show Enemy Distance", DebugCheats.showEnemyDistance, menuX + 50, menuY + 155);
                        }

                        DebugCheats.drawChamsBool = UIHelper.Checkbox("Enemy Chams", DebugCheats.drawChamsBool, menuX + 30, menuY + 175);

                        DebugCheats.drawItemEspBool = UIHelper.Checkbox("Item ESP", DebugCheats.drawItemEspBool, menuX + 30, menuY + 205);
                        if (DebugCheats.drawItemEspBool)
                        {
                            DebugCheats.showItemNames = UIHelper.Checkbox("Show Item Names", DebugCheats.showItemNames, menuX + 50, menuY + 235);
                            DebugCheats.showItemDistance = UIHelper.Checkbox("Show Item Distance", DebugCheats.showItemDistance, menuX + 50, menuY + 265);
                            DebugCheats.showItemValue = UIHelper.Checkbox("Show Item Value", DebugCheats.showItemValue, menuX + 50, menuY + 295);
                            DebugCheats.draw3DItemEspBool = UIHelper.Checkbox("3D Item ESP", DebugCheats.draw3DItemEspBool, menuX + 50, menuY + 325);
                            DebugCheats.showPlayerDeathHeads = UIHelper.Checkbox("Show Dead Player Heads", DebugCheats.showPlayerDeathHeads, menuX + 50, menuY + 355);
                        }

                        DebugCheats.drawExtractionPointEspBool = UIHelper.Checkbox("Extraction ESP", DebugCheats.drawExtractionPointEspBool, menuX + 30, menuY + 385);
                        if (DebugCheats.drawExtractionPointEspBool)
                        {
                            DebugCheats.showExtractionNames = UIHelper.Checkbox("Show Extraction Names", DebugCheats.showExtractionNames, menuX + 50, menuY + 420);
                            DebugCheats.showExtractionDistance = UIHelper.Checkbox("Show Extraction Distance", DebugCheats.showExtractionDistance, menuX + 50, menuY + 450);
                        }

                        DebugCheats.drawPlayerEspBool = UIHelper.Checkbox("2D Player ESP", DebugCheats.drawPlayerEspBool, menuX + 30, menuY + 480);
                        DebugCheats.draw3DPlayerEspBool = UIHelper.Checkbox("3D Player ESP", DebugCheats.draw3DPlayerEspBool, menuX + 30, menuY + 510);
                        if (DebugCheats.drawPlayerEspBool || DebugCheats.draw3DPlayerEspBool)
                        {
                            DebugCheats.showPlayerNames = UIHelper.Checkbox("Show Player Names", DebugCheats.showPlayerNames, menuX + 50, menuY + 540);
                            DebugCheats.showPlayerDistance = UIHelper.Checkbox("Show Player Distance", DebugCheats.showPlayerDistance, menuX + 50, menuY + 570);
                            DebugCheats.showPlayerHP = UIHelper.Checkbox("Show Player HP", DebugCheats.showPlayerHP, menuX + 50, menuY + 600);
                        }
                        break;

                    case MenuCategory.Combat:
                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", menuX + 30, menuY + 95);
                        playerScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 200), playerScrollPosition, new Rect(0, 0, 520, playerNames.Count * 35), false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            if (i == selectedPlayerIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), playerNames[i])) selectedPlayerIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();

                        if (UIHelper.Button("Revive", menuX + 30, menuY + 330)) { ReviveSelectedPlayer(); DLog.Log("Player revived: " + playerNames[selectedPlayerIndex]); }
                        if (UIHelper.Button("Kill Selected Player", menuX + 30, menuY + 370)) { KillSelectedPlayer(); DLog.Log("Tentativa de matar o jogador selecionado realizada."); }
                        if (UIHelper.Button("Send Player To Void", menuX + 30, menuY + 410)) SendSelectedPlayerToVoid();
                        if (UIHelper.Button("Teleport Player To Me", menuX + 30, menuY + 450)) { Teleport.TeleportPlayerToMe(selectedPlayerIndex, playerList, playerNames); DLog.Log($"Teleportado {playerNames[selectedPlayerIndex]} até você."); }
                        if (UIHelper.Button("Teleport Me To Player", menuX + 30, menuY + 490)) { Teleport.TeleportMeToPlayer(selectedPlayerIndex, playerList, playerNames); DLog.Log($"Teleportado você até {playerNames[selectedPlayerIndex]}."); }
                        break;

                    case MenuCategory.Misc:
                        if (UIHelper.Button("Spawn Money", menuX + 30, menuY + 105))
                        {
                            DLog.Log("Botão 'Spawn Money' clicado!");
                            GameObject localPlayer = DebugCheats.GetLocalPlayer();
                            if (localPlayer == null)
                            {
                                DLog.Log("Jogador local não encontrado!");
                                return;
                            }
                            Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                            transform.position = targetPosition;
                            ItemSpawner.SpawnItem(targetPosition);
                            DLog.Log("Money spawned.");
                        }
                        bool newPlayerColorState = UIHelper.ButtonBool("RGB Player", playerColor.isRandomizing, menuX + 30, menuY + 145);
                        if (newPlayerColorState != playerColor.isRandomizing)
                        {
                            playerColor.isRandomizing = newPlayerColorState;
                            DLog.Log("Randomize toggled: " + playerColor.isRandomizing);
                        }

                        UIHelper.Label("Flashlight Intensity: " + Hax2.flashlightIntensity, menuX + 30, menuY + 185);
                        Hax2.flashlightIntensity = UIHelper.Slider(Hax2.flashlightIntensity, 1f, 100f, menuX + 30, menuY + 205);

                        UIHelper.Label("Crouch Delay: " + Hax2.crouchDelay, menuX + 30, menuY + 225);
                        Hax2.crouchDelay = UIHelper.Slider(Hax2.crouchDelay, 0f, 5f, menuX + 30, menuY + 245);

                        UIHelper.Label("Set Crouch Speed: " + Hax2.crouchSpeed, menuX + 30, menuY + 265);
                        Hax2.crouchSpeed = UIHelper.Slider(Hax2.crouchSpeed, 1f, 50f, menuX + 30, menuY + 285);

                        UIHelper.Label("Set Jump Force: " + Hax2.jumpForce, menuX + 30, menuY + 305);
                        Hax2.jumpForce = UIHelper.Slider(Hax2.jumpForce, 1f, 50f, menuX + 30, menuY + 326);

                        UIHelper.Label("Set Extra Jumps: " + Hax2.extraJumps, menuX + 30, menuY + 345);
                        Hax2.extraJumps = (int)UIHelper.Slider(Hax2.extraJumps, 1f, 100f, menuX + 30, menuY + 365);

                        UIHelper.Label("Set Custom Gravity: " + Hax2.customGravity, menuX + 30, menuY + 385);
                        Hax2.customGravity = UIHelper.Slider(Hax2.customGravity, -10f, 50f, menuX + 30, menuY + 405);

                        UIHelper.Label("Set Grab Range: " + Hax2.grabRange, menuX + 30, menuY + 425);
                        Hax2.grabRange = UIHelper.Slider(Hax2.grabRange, 0f, 50f, menuX + 30, menuY + 445);

                        UIHelper.Label("Set Throw Strength: " + Hax2.throwStrength, menuX + 30, menuY + 465);
                        Hax2.throwStrength = UIHelper.Slider(Hax2.throwStrength, 0f, 50f, menuX + 30, menuY + 485);

                        UIHelper.Label("Set Slide Decay: " + Hax2.slideDecay, menuX + 30, menuY + 505);
                        Hax2.slideDecay = UIHelper.Slider(Hax2.slideDecay, -10f, 50f, menuX + 30, menuY + 525);


                        if (Hax2.flashlightIntensity != OldflashlightIntensity)
                        {
                            PlayerController.SetFlashlightIntensity(Hax2.flashlightIntensity);
                            OldflashlightIntensity = Hax2.flashlightIntensity;
                        }
                        if (Hax2.crouchDelay != OldcrouchDelay)
                        {
                            PlayerController.SetCrouchDelay(Hax2.crouchDelay);
                            OldcrouchDelay = Hax2.crouchDelay;
                        }
                        if (Hax2.jumpForce != Hax2.OldjumpForce)
                        {
                            PlayerController.SetJumpForce(Hax2.jumpForce);
                            OldjumpForce = Hax2.jumpForce;
                        }
                        if (Hax2.customGravity != Hax2.OldcustomGravity)
                        {
                            PlayerController.SetCustomGravity(Hax2.customGravity);
                            OldcustomGravity = Hax2.customGravity;
                        }
                        if (Hax2.extraJumps != Hax2.OldextraJumps)
                        {
                            PlayerController.SetExtraJumps(Hax2.extraJumps);
                            OldextraJumps = Hax2.extraJumps;
                        }
                        if (Hax2.crouchSpeed != Hax2.OldcrouchSpeed)
                        {
                            PlayerController.SetCrouchSpeed(Hax2.crouchSpeed);
                            OldcrouchSpeed = Hax2.crouchSpeed;
                        }
                        if (Hax2.grabRange != Hax2.OldgrabRange)
                        {
                            PlayerController.SetGrabRange(Hax2.grabRange);
                            OldgrabRange = Hax2.grabRange;
                        }
                        if (Hax2.throwStrength != Hax2.OldthrowStrength)
                        {
                            PlayerController.SetThrowStrength(Hax2.throwStrength);
                            OldthrowStrength = Hax2.throwStrength;
                        }
                        if (Hax2.slideDecay != Hax2.OldslideDecay)
                        {
                            PlayerController.SetSlideDecay(Hax2.slideDecay);
                            OldslideDecay = Hax2.slideDecay;
                        }

                        bool newNoFogState = UIHelper.ButtonBool("Toggle No Fog", MiscFeatures.NoFogEnabled, menuX + 30, menuY + 565);
                        if (newNoFogState != MiscFeatures.NoFogEnabled)
                        {
                            MiscFeatures.ToggleNoFog(newNoFogState);
                        }
                        break;

                    case MenuCategory.Enemies:
                        UpdateEnemyList();
                        UIHelper.Label("Select an enemy:", menuX + 30, menuY + 95);

                        enemyScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 200), enemyScrollPosition, new Rect(0, 0, 520, enemyNames.Count * 35), false, true);
                        for (int i = 0; i < enemyNames.Count; i++)
                        {
                            if (i == selectedEnemyIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), enemyNames[i])) selectedEnemyIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();

                        if (UIHelper.Button("Kill Selected Enemy", menuX + 30, menuY + 330))
                        {
                            KillSelectedEnemy();
                            DLog.Log($"Tentativa de matar o inimigo selecionado realizada: {enemyNames[selectedEnemyIndex]}");
                        }
                        if (UIHelper.Button("Kill All Enemies", menuX + 30, menuY + 370))
                        {
                            DebugCheats.KillAllEnemies();
                            DLog.Log("Tentativa de matar todos os inimigos realizada.");
                        }
                        if (UIHelper.Button("Teleport Enemy to Me", menuX + 30, menuY + 410))
                        {
                            TeleportEnemyToMe();
                            DLog.Log($"Tentativa de teleportar {enemyNames[selectedEnemyIndex]} até você realizada.");
                        }
                        break;

                    case MenuCategory.Items:
                        UIHelper.Label("Select an item:", menuX + 30, menuY + 95);

                        itemScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 200), itemScrollPosition, new Rect(0, 0, 520, itemList.Count * 35), false, true);
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            if (i == selectedItemIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), $"{itemList[i].Name} [Value: {itemList[i].Value}$]"))
                            {
                                selectedItemIndex = i;
                            }
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();

                        if (UIHelper.Button("Teleport Item to Me", menuX + 30, menuY + 330))
                        {
                            if (selectedItemIndex >= 0 && selectedItemIndex < itemList.Count)
                            {
                                ItemTeleport.TeleportItemToMe(itemList[selectedItemIndex]);
                                DLog.Log($"Teleported item: {itemList[selectedItemIndex].Name}");
                            }
                            else
                            {
                                DLog.Log("Nenhum item válido selecionado para teleporte!");
                            }
                        }
                        if (UIHelper.Button("Teleport All Items to Me", menuX + 30, menuY + 370))
                        {
                            ItemTeleport.TeleportAllItemsToMe();
                            DLog.Log("Teleporting all items initiated.");
                        }
                        if (UIHelper.Button("Change Item Value to 10K", menuX + 30, menuY + 410))
                        {
                            if (selectedItemIndex >= 0 && selectedItemIndex < itemList.Count)
                            {
                                ItemTeleport.SetItemValue(itemList[selectedItemIndex], 10000);
                                DLog.Log($"Updated value: {itemList[selectedItemIndex].Value}");
                            }
                            else
                            {
                                DLog.Log("Nenhum item válido selecionado para alterar valor!");
                            }
                        }
                        break;

                    case MenuCategory.Hotkeys:
                        Rect viewRect = new Rect(menuX + 20, menuY + 95, 560, 620);
                        Rect contentRect = new Rect(0, 0, 540, 1200);

                        if (!string.IsNullOrEmpty(keyAssignmentError) && Time.time - errorMessageTime < ERROR_MESSAGE_DURATION)
                        {
                            GUIStyle errorStyle = new GUIStyle(GUI.skin.label)
                            {
                                fontSize = 14,
                                fontStyle = FontStyle.Bold,
                                normal = { textColor = Color.red },
                                alignment = TextAnchor.MiddleCenter
                            };

                            GUI.Label(new Rect(menuX + 20, menuY + 95, 560, 25), keyAssignmentError, errorStyle);

                            viewRect.y += 30;
                            viewRect.height -= 30;
                        }

                        hotkeyScrollPosition = GUI.BeginScrollView(viewRect, hotkeyScrollPosition, contentRect);

                        float yPos = 10;

                        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 16,
                            fontStyle = FontStyle.Bold,
                            normal = { textColor = Color.white }
                        };

                        GUI.Label(new Rect(50, yPos, 540, 25), "Hotkey Configuration", headerStyle);
                        yPos += 30;

                        GUI.Label(new Rect(50, yPos, 540, 20), "How to set up a hotkey:", instructionStyle);
                        yPos += 20;
                        GUI.Label(new Rect(70, yPos, 540, 20), "1. Click on a key field → press desired key", instructionStyle);
                        yPos += 20;
                        GUI.Label(new Rect(70, yPos, 540, 20), "2. Click on action field → select function", instructionStyle);
                        yPos += 25;

                        GUIStyle warningStyle = new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 12,
                            normal = { textColor = Color.yellow }
                        };
                        GUI.Label(new Rect(50, yPos, 540, 20), "Warning: Ensure each key is only assigned to one action", warningStyle);
                        yPos += 30;

                        GUI.Label(new Rect(10, yPos, 540, 25), "System Keys", headerStyle);
                        yPos += 30;

                        string menuToggleKeyText = (configuringSystemKey && systemKeyConfigIndex == 0 && waitingForAnyKey)
                            ? "Press any key..." : menuToggleKey.ToString();
                        GUI.Label(new Rect(10, yPos, 150, 30), "Menu Toggle:");
                        if (GUI.Button(new Rect(170, yPos, 290, 30), menuToggleKeyText))
                        {
                            StartConfigureSystemKey(0);
                        }
                        yPos += 40;

                        string reloadKeyText = (configuringSystemKey && systemKeyConfigIndex == 1 && waitingForAnyKey)
                            ? "Press any key..." : reloadKey.ToString();
                        GUI.Label(new Rect(10, yPos, 150, 30), "Reload:");
                        if (GUI.Button(new Rect(170, yPos, 290, 30), reloadKeyText))
                        {
                            StartConfigureSystemKey(1);
                        }
                        yPos += 40;

                        string unloadKeyText = (configuringSystemKey && systemKeyConfigIndex == 2 && waitingForAnyKey)
                            ? "Press any key..." : unloadKey.ToString();
                        GUI.Label(new Rect(10, yPos, 150, 30), "Unload:");
                        if (GUI.Button(new Rect(170, yPos, 290, 30), unloadKeyText))
                        {
                            StartConfigureSystemKey(2);
                        }
                        yPos += 50;

                        GUI.Label(new Rect(10, yPos, 540, 25), "Action Hotkeys", headerStyle);
                        yPos += 30;

                        for (int i = 0; i < 12; i++)
                        {
                            KeyCode currentKey = defaultHotkeys[i];
                            string keyText = (currentKey == KeyCode.None) ? "Not Set" : currentKey.ToString();
                            string actionName = "Not assigned";

                            if (currentKey != KeyCode.None && hotkeyBindings.ContainsKey(currentKey))
                            {
                                var action = hotkeyBindings[currentKey];
                                if (action != null)
                                {
                                    for (int j = 0; j < availableActions.Count; j++)
                                    {
                                        if (availableActions[j].Action == action)
                                        {
                                            actionName = availableActions[j].Name;
                                            break;
                                        }
                                    }
                                }
                            }

                            Rect slotRect = new Rect(10, yPos, 150, 30);
                            bool isSelected = selectedHotkeySlot == i && configuringHotkey;

                            if (GUI.Button(slotRect, isSelected ? "Press any key..." : keyText))
                            {
                                selectedHotkeySlot = i;
                                configuringHotkey = true;
                                DLog.Log("configuring hotkey for slot " + (i + 1));
                                SaveHotkeySettings();
                            }

                            Rect actionRect = new Rect(170, yPos, 290, 30);
                            if (GUI.Button(actionRect, actionName))
                            {
                                selectedHotkeySlot = i;
                                if (currentKey != KeyCode.None)
                                {
                                    ShowActionSelector(i, currentKey);
                                }
                                else
                                {
                                    DLog.Log("Please assign a key to this slot first");
                                }
                            }

                            Rect clearRect = new Rect(470, yPos, 60, 30);
                            if (GUI.Button(clearRect, "Clear") && currentKey != KeyCode.None)
                            {
                                if (hotkeyBindings.ContainsKey(currentKey))
                                {
                                    hotkeyBindings.Remove(currentKey);
                                }
                                defaultHotkeys[i] = KeyCode.None;
                                DLog.Log("cleared hotkey binding for slot " + (i + 1));
                                SaveHotkeySettings();
                            }

                            yPos += 40;
                        }

                        GUI.EndScrollView();
                        break;
                }
            }

            if (showingActionSelector)
            {
                // draw full-screen overlay and consume mouse events so main GUI is blocked
                Rect fullOverlay = new Rect(0, 0, Screen.width, Screen.height);
                GUI.Box(fullOverlay, "", overlayDimStyle);
                if (Event.current.type == EventType.MouseDown ||
                    Event.current.type == EventType.MouseUp ||
                    Event.current.type == EventType.MouseDrag)
                {
                    Event.current.Use(); // consume events
                }

                // draw modal window using GUI.Window for natural dragging
                Rect modalRect = new Rect(actionSelectorX, actionSelectorY, 400, 400);
                modalRect = GUI.Window(12345, modalRect, ActionSelectorWindow, "", actionSelectorBoxStyle);
                actionSelectorX = modalRect.x;
                actionSelectorY = modalRect.y;
            }
        }

        private static Texture2D MakeSolidBackground(Color color, float alpha)//fix
        {
            Color key = new Color(color.r, color.g, color.b, alpha);
            if (!solidTextures.ContainsKey(key))
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, key);
                texture.Apply();
                solidTextures[color] = texture;
            }
            return solidTextures[color];
        }

    }
}
