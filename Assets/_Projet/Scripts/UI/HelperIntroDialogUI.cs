using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Characters;
using VillaDelChef.Managers;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    /// <summary>
    /// Escenario 1.5 — Invitación del Primer Ayudante (Fase 7).
    /// Se dispara cuando el jugador sirve 3 clientes. Permite invitar a un amigo (excluyendo al protagonista)
    /// o posponerlo con [AHORA NO].
    /// </summary>
    public class HelperIntroDialogUI : MonoBehaviour
    {
        public static HelperIntroDialogUI Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private GameObject invitePanel;
        [SerializeField] private GameObject selectionPanel;

        [Header("Invite Panel Elements")]
        [SerializeField] private Text inviteTitleText;
        [SerializeField] private Text inviteBodyText;
        [SerializeField] private Button chooseHelperButton;
        [SerializeField] private Button notNowButton;

        [Header("Selection Panel Elements")]
        [SerializeField] private Transform characterGridContainer;
        [SerializeField] private GameObject characterCardPrefab;
        [SerializeField] private Button blackOutfitBtn;
        [SerializeField] private Button whiteOutfitBtn;
        [SerializeField] private Image previewImage;
        [SerializeField] private Text selectedFriendNameText;
        [SerializeField] private Button confirmHelperBtn;
        [SerializeField] private Button cancelSelectionBtn;

        private CharacterSO selectedHelperCharacter;
        private CharacterOutfit selectedHelperOutfit = CharacterOutfit.ChefWhite;
        private List<CharacterSO> availableFriends = new List<CharacterSO>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (modalRoot != null) modalRoot.SetActive(false);
        }

        private void Start()
        {
            if (chooseHelperButton != null) chooseHelperButton.onClick.AddListener(OnChooseHelperClicked);
            if (notNowButton != null) notNowButton.onClick.AddListener(OnNotNowClicked);
            if (blackOutfitBtn != null) blackOutfitBtn.onClick.AddListener(() => SetOutfit(CharacterOutfit.ChefBlack));
            if (whiteOutfitBtn != null) whiteOutfitBtn.onClick.AddListener(() => SetOutfit(CharacterOutfit.ChefWhite));
            if (confirmHelperBtn != null) confirmHelperBtn.onClick.AddListener(OnConfirmHelperClicked);
            if (cancelSelectionBtn != null) cancelSelectionBtn.onClick.AddListener(OnCancelSelectionClicked);
        }

        public static void ShowIfAvailable()
        {
            if (Instance != null)
            {
                Instance.OpenInviteModal();
            }
            else
            {
                // Fallback dinámico si no existe en la escena
                CreateFallbackModal();
            }
        }

        public void OpenInviteModal()
        {
            if (modalRoot != null) modalRoot.SetActive(true);
            if (invitePanel != null) invitePanel.SetActive(true);
            if (selectionPanel != null) selectionPanel.SetActive(false);

            if (inviteTitleText != null) inviteTitleText.text = "¡El restaurante se está llenando!";
            if (inviteBodyText != null)
            {
                inviteBodyText.text = "Has atendido a tus primeros clientes con éxito.\n¿Te gustaría invitar a un amigo para que te ayude en el servicio?";
            }
        }

        private void OnChooseHelperClicked()
        {
            if (invitePanel != null) invitePanel.SetActive(false);
            if (selectionPanel != null) selectionPanel.SetActive(true);

            LoadAvailableFriends();
        }

        private void OnNotNowClicked()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                SaveManager.Instance.SaveData.helperIntroTriggered = true;
                SaveManager.Instance.SaveData.helperSelectionSkipped = true;
                SaveManager.Instance.SaveGame();
            }

            if (modalRoot != null) modalRoot.SetActive(false);
            Debug.Log("[HelperIntroDialogUI] Selección de ayudante pospuesta por el jugador.");
        }

        private void LoadAvailableFriends()
        {
            availableFriends.Clear();
            selectedHelperCharacter = null;
            if (selectedFriendNameText != null) selectedFriendNameText.text = "Selecciona un amigo";
            if (previewImage != null) previewImage.sprite = null;
            if (confirmHelperBtn != null) confirmHelperBtn.interactable = false;

            var allCharacters = Resources.LoadAll<CharacterSO>("Characters");

            string playerID = SaveManager.Instance != null && SaveManager.Instance.SaveData != null
                ? SaveManager.Instance.SaveData.selectedPlayerCharacterID
                : "";

            foreach (var ch in allCharacters)
            {
                if (ch == null) continue;
                // REGLA CRÍTICA: Excluir al protagonista
                if (!string.IsNullOrEmpty(playerID) && ch.characterID == playerID) continue;
                if (!ch.selectableAsHelper) continue;

                availableFriends.Add(ch);
            }

            PopulateGrid();
        }

        private void PopulateGrid()
        {
            if (characterGridContainer == null) return;

            // Limpiar tarjetas previas
            foreach (Transform child in characterGridContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var ch in availableFriends)
            {
                bool isVisiting = CustomerManager.Instance != null && CustomerManager.Instance.IsFriendCurrentlyCustomer(ch.characterID);

                GameObject cardGO = null;
                if (characterCardPrefab != null)
                {
                    cardGO = Instantiate(characterCardPrefab, characterGridContainer);
                    var cardUI = cardGO.GetComponent<CharacterCardUI>() ?? cardGO.AddComponent<CharacterCardUI>();
                    bool bound = cardUI.Bind(ch, isVisiting, SelectCharacter);
                    if (!bound)
                    {
                        Debug.LogWarning($"[HelperIntroDialogUI] El prefab '{characterCardPrefab.name}' no tiene la estructura requerida. Creando tarjeta procedural de respaldo para '{ch.characterID}'.");
                        Destroy(cardGO);
                        cardGO = CharacterCardUI.CreateProceduralCard(characterGridContainer, ch, isVisiting, SelectCharacter);
                    }
                }
                else
                {
                    cardGO = CharacterCardUI.CreateProceduralCard(characterGridContainer, ch, isVisiting, SelectCharacter);
                }
            }
        }

        private void SelectCharacter(CharacterSO character)
        {
            selectedHelperCharacter = character;
            if (selectedFriendNameText != null && character != null)
            {
                selectedFriendNameText.text = character.displayName;
            }
            UpdatePreview();
        }

        private void SetOutfit(CharacterOutfit outfit)
        {
            selectedHelperOutfit = outfit;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (selectedHelperCharacter != null)
            {
                bool blackAvailable = selectedHelperCharacter.HasCompleteOutfit(CharacterOutfit.ChefBlack);
                bool whiteAvailable = selectedHelperCharacter.HasCompleteOutfit(CharacterOutfit.ChefWhite);

                if (blackOutfitBtn != null) blackOutfitBtn.interactable = blackAvailable;
                if (whiteOutfitBtn != null) whiteOutfitBtn.interactable = whiteAvailable;

                if (!selectedHelperCharacter.HasCompleteOutfit(selectedHelperOutfit))
                {
                    if (whiteAvailable) selectedHelperOutfit = CharacterOutfit.ChefWhite;
                    else if (blackAvailable) selectedHelperOutfit = CharacterOutfit.ChefBlack;
                }

                if (previewImage != null)
                {
                    previewImage.sprite = selectedHelperCharacter.GetPreviewSprite(selectedHelperOutfit, allowCrossOutfitFallback: false);
                }

                if (confirmHelperBtn != null)
                {
                    confirmHelperBtn.interactable = selectedHelperCharacter.HasCompleteOutfit(selectedHelperOutfit);
                }
            }
            else
            {
                if (previewImage != null) previewImage.sprite = null;
                if (blackOutfitBtn != null) blackOutfitBtn.interactable = false;
                if (whiteOutfitBtn != null) whiteOutfitBtn.interactable = false;
                if (confirmHelperBtn != null) confirmHelperBtn.interactable = false;
            }
        }

        private void OnConfirmHelperClicked()
        {
            if (selectedHelperCharacter == null) return;

            if (!selectedHelperCharacter.HasCompleteOutfit(selectedHelperOutfit))
            {
                Debug.LogError($"[HelperIntroDialogUI] El vestuario {selectedHelperOutfit} no está completo para {selectedHelperCharacter.displayName}.");
                return;
            }

            // REGLA CRÍTICA FASE 7.0.2: Proteger contra condición de carrera con comensal activo
            if (CustomerManager.Instance != null && CustomerManager.Instance.IsFriendCurrentlyCustomer(selectedHelperCharacter.characterID))
            {
                Debug.LogWarning($"[HelperIntroDialogUI] Este amigo ({selectedHelperCharacter.displayName}) está visitando el restaurante como comensal.");
                return;
            }

            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                var data = SaveManager.Instance.SaveData;
                data.selectedHelperCharacterID = selectedHelperCharacter.characterID;
                data.helperChefOutfit = selectedHelperOutfit;
                data.helperIntroTriggered = true;
                data.helperSelectionSkipped = false;
                SaveManager.Instance.SaveGame();
            }

            // Spawn or update worker in WorkerManager
            ApplyHelperToRestaurant(selectedHelperCharacter, selectedHelperOutfit);

            if (modalRoot != null) modalRoot.SetActive(false);
            Debug.Log($"[HelperIntroDialogUI] ¡Ayudante {selectedHelperCharacter.displayName} contratado con uniforme {selectedHelperOutfit}!");
        }

        private void OnCancelSelectionClicked()
        {
            if (selectionPanel != null) selectionPanel.SetActive(false);
            if (invitePanel != null) invitePanel.SetActive(true);
        }

        public static void ApplyHelperToRestaurant(CharacterSO helperCharacter, CharacterOutfit outfit)
        {
            if (helperCharacter == null) return;

            // REGLA CRÍTICA FASE 7.0.2: Validar outfit solicitado antes de aplicar
            if (!helperCharacter.HasCompleteOutfit(outfit))
            {
                Debug.LogError($"[HelperIntroDialogUI] No se puede aplicar el ayudante '{helperCharacter.displayName}' con vestuario {outfit} porque no está completo.");
                return;
            }

            if (WorkerManager.Instance != null)
            {
                // Buscar si ya hay un worker activo para asignarle apariencia
                if (WorkerManager.Instance.activeWorkers.Count > 0)
                {
                    var worker = WorkerManager.Instance.activeWorkers[0];
                    var app = worker.GetComponent<CharacterAppearanceController>();
                    if (app == null) app = worker.gameObject.AddComponent<CharacterAppearanceController>();
                    app.ApplyCharacter(helperCharacter, outfit);
                }
                else
                {
                    // Crear worker con WorkerSO fallback y aplicarle CharacterAppearanceController
                    var defaultWorkerSO = Resources.Load<WorkerSO>("Worker_Waiter");
                    if (defaultWorkerSO == null)
                    {
                        defaultWorkerSO = ScriptableObject.CreateInstance<WorkerSO>();
                        defaultWorkerSO.workerName = helperCharacter.displayName;
                    }
                    WorkerManager.Instance.SpawnWorker(defaultWorkerSO, WorkerManager.Instance.initialWorkerGrid);
                    if (WorkerManager.Instance.activeWorkers.Count > 0)
                    {
                        var worker = WorkerManager.Instance.activeWorkers[0];
                        var app = worker.GetComponent<CharacterAppearanceController>();
                        if (app == null) app = worker.gameObject.AddComponent<CharacterAppearanceController>();
                        app.ApplyCharacter(helperCharacter, outfit);
                    }
                }
            }
        }

        private static void CreateFallbackModal()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject root = new GameObject("HelperIntroModal_Fallback");
            root.transform.SetParent(canvas.transform, false);

            var dialog = root.AddComponent<HelperIntroDialogUI>();
            dialog.modalRoot = root;

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(580, 360);
            rt.anchoredPosition = Vector2.zero;

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            GameObject textGO = new GameObject("InviteText");
            textGO.transform.SetParent(root.transform, false);
            Text t = textGO.AddComponent<Text>();
            t.text = "<b>¡El restaurante se está llenando!</b>\n\nHas atendido a tus primeros comensales.\n¿Te gustaría invitar a un amigo para que te ayude?";
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 20;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.anchoredPosition = new Vector2(0f, 40f);
            trt.sizeDelta = new Vector2(520, 160);

            // Elegir Ayudante Button
            GameObject chooseGO = new GameObject("ChooseBtn");
            chooseGO.transform.SetParent(root.transform, false);
            Image cBg = chooseGO.AddComponent<Image>();
            cBg.color = new Color(0.2f, 0.65f, 0.35f);
            Button cBtn = chooseGO.AddComponent<Button>();
            RectTransform cRT = chooseGO.GetComponent<RectTransform>();
            cRT.anchoredPosition = new Vector2(120f, -100f);
            cRT.sizeDelta = new Vector2(200, 50);

            GameObject cTextGO = new GameObject("Label");
            cTextGO.transform.SetParent(chooseGO.transform, false);
            Text cTxt = cTextGO.AddComponent<Text>();
            cTxt.text = "ELEGIR AMIGO";
            cTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cTxt.fontSize = 18;
            cTxt.alignment = TextAnchor.MiddleCenter;
            cTxt.color = Color.white;
            RectTransform ctxtRT = cTextGO.GetComponent<RectTransform>();
            ctxtRT.sizeDelta = cRT.sizeDelta;

            // Ahora No Button
            GameObject skipGO = new GameObject("SkipBtn");
            skipGO.transform.SetParent(root.transform, false);
            Image sBg = skipGO.AddComponent<Image>();
            sBg.color = new Color(0.45f, 0.48f, 0.52f);
            Button sBtn = skipGO.AddComponent<Button>();
            RectTransform sRT = skipGO.GetComponent<RectTransform>();
            sRT.anchoredPosition = new Vector2(-120f, -100f);
            sRT.sizeDelta = new Vector2(180, 50);

            GameObject sTextGO = new GameObject("Label");
            sTextGO.transform.SetParent(skipGO.transform, false);
            Text sTxt = sTextGO.AddComponent<Text>();
            sTxt.text = "AHORA NO";
            sTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sTxt.fontSize = 18;
            sTxt.alignment = TextAnchor.MiddleCenter;
            sTxt.color = Color.white;
            RectTransform stxtRT = sTextGO.GetComponent<RectTransform>();
            stxtRT.sizeDelta = sRT.sizeDelta;

            cBtn.onClick.AddListener(() =>
            {
                Object.Destroy(root);
                if (StaffMenuUI.Instance != null)
                {
                    StaffMenuUI.Instance.OpenHelperSelection();
                }
                else
                {
                    StaffMenuUI.CreateFallbackModal();
                    if (StaffMenuUI.Instance != null)
                    {
                        StaffMenuUI.Instance.OpenHelperSelection();
                    }
                    else
                    {
                        Debug.LogError("[HelperIntroDialogUI] ERROR: No fue posible abrir el menú de selección de personal.");
                    }
                }
            });

            sBtn.onClick.AddListener(() =>
            {
                if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
                {
                    SaveManager.Instance.SaveData.helperIntroTriggered = true;
                    SaveManager.Instance.SaveData.helperSelectionSkipped = true;
                    SaveManager.Instance.SaveGame();
                }
                Object.Destroy(root);
            });
        }
    }
}
