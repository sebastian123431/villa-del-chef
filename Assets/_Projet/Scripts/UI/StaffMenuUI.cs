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
    /// Menú de Personal / Equipo (Fase 7.0.1 — Secciones 15, 16, 56, 57, 74).
    /// Permite al jugador ver el ayudante actual, alternar su uniforme (ChefBlack / ChefWhite),
    /// cambiar de ayudante (relevo con reincorporación dinámica del anterior al pool de clientes),
    /// o retirarlo del restaurante.
    /// </summary>
    public class StaffMenuUI : MonoBehaviour
    {
        public static StaffMenuUI Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private GameObject overviewPanel;
        [SerializeField] private GameObject selectionPanel;

        [Header("Overview Elements")]
        [SerializeField] private Image currentHelperPreview;
        [SerializeField] private Text currentHelperNameText;
        [SerializeField] private Text currentHelperStatusText;
        [SerializeField] private Button changeOutfitBlackBtn;
        [SerializeField] private Button changeOutfitWhiteBtn;
        [SerializeField] private Button changeHelperBtn;
        [SerializeField] private Button dismissHelperBtn;
        [SerializeField] private Button closeBtn;

        [Header("Selection Elements")]
        [SerializeField] private Transform friendsGridContainer;
        [SerializeField] private Image selectionPreviewImage;
        [SerializeField] private Text selectionNameText;
        [SerializeField] private Button selectOutfitBlackBtn;
        [SerializeField] private Button selectOutfitWhiteBtn;
        [SerializeField] private Button confirmSelectionBtn;
        [SerializeField] private Button cancelSelectionBtn;

        private CharacterSO pendingSelectedFriend;
        private CharacterOutfit pendingSelectedOutfit = CharacterOutfit.ChefWhite;
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

            if (menuRoot != null) menuRoot.SetActive(false);
        }

        private void Start()
        {
            HookButtons();
        }

        private void HookButtons()
        {
            if (closeBtn != null) closeBtn.onClick.AddListener(Close);
            if (changeHelperBtn != null) changeHelperBtn.onClick.AddListener(OpenSelectionPanel);
            if (dismissHelperBtn != null) dismissHelperBtn.onClick.AddListener(OnDismissHelperClicked);
            if (changeOutfitBlackBtn != null) changeOutfitBlackBtn.onClick.AddListener(() => SetCurrentHelperOutfit(CharacterOutfit.ChefBlack));
            if (changeOutfitWhiteBtn != null) changeOutfitWhiteBtn.onClick.AddListener(() => SetCurrentHelperOutfit(CharacterOutfit.ChefWhite));

            if (selectOutfitBlackBtn != null) selectOutfitBlackBtn.onClick.AddListener(() => SetPendingOutfit(CharacterOutfit.ChefBlack));
            if (selectOutfitWhiteBtn != null) selectOutfitWhiteBtn.onClick.AddListener(() => SetPendingOutfit(CharacterOutfit.ChefWhite));
            if (confirmSelectionBtn != null) confirmSelectionBtn.onClick.AddListener(OnConfirmSelectionClicked);
            if (cancelSelectionBtn != null) cancelSelectionBtn.onClick.AddListener(OpenOverviewPanel);
        }

        public static void CreateFallbackModal()
        {
            if (Instance == null)
            {
                Canvas canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas == null) return;
                GameObject go = new GameObject("StaffMenu_Auto");
                go.transform.SetParent(canvas.transform, false);
                Instance = go.AddComponent<StaffMenuUI>();
            }
            Instance.Open();
        }

        public void OpenHelperSelection()
        {
            Open();
            OpenSelectionPanel();
        }

        public void Toggle()
        {
            if (menuRoot != null && menuRoot.activeSelf)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (menuRoot == null)
            {
                CreateFallbackMenuUI();
                return;
            }

            menuRoot.SetActive(true);
            OpenOverviewPanel();
        }

        public void Close()
        {
            if (menuRoot != null) menuRoot.SetActive(false);
        }

        private void OpenOverviewPanel()
        {
            if (overviewPanel != null) overviewPanel.SetActive(true);
            if (selectionPanel != null) selectionPanel.SetActive(false);

            RefreshOverviewDisplay();
        }

        private void RefreshOverviewDisplay()
        {
            string helperID = SaveManager.Instance?.SaveData?.selectedHelperCharacterID ?? "";
            CharacterOutfit outfit = SaveManager.Instance?.SaveData?.helperChefOutfit ?? CharacterOutfit.ChefWhite;

            CharacterSO helperSO = !string.IsNullOrEmpty(helperID) ? Resources.Load<CharacterSO>($"Characters/{helperID}") : null;

            if (helperSO != null)
            {
                if (currentHelperNameText != null) currentHelperNameText.text = helperSO.displayName;
                if (currentHelperStatusText != null) currentHelperStatusText.text = "<b>Rol:</b> Ayudante de Cocina Activo";
                if (currentHelperPreview != null) currentHelperPreview.sprite = helperSO.GetPreviewSprite(outfit, allowCrossOutfitFallback: true);

                if (changeOutfitBlackBtn != null) changeOutfitBlackBtn.gameObject.SetActive(true);
                if (changeOutfitWhiteBtn != null) changeOutfitWhiteBtn.gameObject.SetActive(true);
                if (dismissHelperBtn != null) dismissHelperBtn.gameObject.SetActive(true);
                if (changeHelperBtn != null)
                {
                    Text btnTxt = changeHelperBtn.GetComponentInChildren<Text>();
                    if (btnTxt != null) btnTxt.text = "CAMBIAR AYUDANTE";
                }
            }
            else
            {
                if (currentHelperNameText != null) currentHelperNameText.text = "Sin Ayudante Asignado";
                if (currentHelperStatusText != null) currentHelperStatusText.text = "Contrata a uno de tus amigos para que te asista en el servicio del restaurante.";
                if (currentHelperPreview != null) currentHelperPreview.sprite = null;

                if (changeOutfitBlackBtn != null) changeOutfitBlackBtn.gameObject.SetActive(false);
                if (changeOutfitWhiteBtn != null) changeOutfitWhiteBtn.gameObject.SetActive(false);
                if (dismissHelperBtn != null) dismissHelperBtn.gameObject.SetActive(false);
                if (changeHelperBtn != null)
                {
                    Text btnTxt = changeHelperBtn.GetComponentInChildren<Text>();
                    if (btnTxt != null) btnTxt.text = "CONTRATAR AYUDANTE";
                }
            }
        }

        private void SetCurrentHelperOutfit(CharacterOutfit outfit)
        {
            string helperID = SaveManager.Instance?.SaveData?.selectedHelperCharacterID ?? "";
            if (string.IsNullOrEmpty(helperID)) return;

            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                SaveManager.Instance.SaveData.helperChefOutfit = outfit;
                SaveManager.Instance.SaveGame();
            }

            CharacterSO helperSO = Resources.Load<CharacterSO>($"Characters/{helperID}");
            if (helperSO != null)
            {
                HelperIntroDialogUI.ApplyHelperToRestaurant(helperSO, outfit);
            }

            RefreshOverviewDisplay();
        }

        private void OnDismissHelperClicked()
        {
            string oldHelperID = SaveManager.Instance?.SaveData?.selectedHelperCharacterID ?? "";
            if (string.IsNullOrEmpty(oldHelperID)) return;

            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                SaveManager.Instance.SaveData.selectedHelperCharacterID = "";
                SaveManager.Instance.SaveGame();
            }

            // Desactivar apariencia del worker en runtime
            if (WorkerManager.Instance != null && WorkerManager.Instance.activeWorkers.Count > 0)
            {
                var worker = WorkerManager.Instance.activeWorkers[0];
                var app = worker.GetComponent<CharacterAppearanceController>();
                if (app != null) app.ResetAppearance();
            }

            Debug.Log($"[StaffMenuUI] Ayudante '{oldHelperID}' retirado. Reingresa de inmediato al pool de comensales elegibles.");
            RefreshOverviewDisplay();
        }

        private void OpenSelectionPanel()
        {
            if (overviewPanel != null) overviewPanel.SetActive(false);
            if (selectionPanel != null) selectionPanel.SetActive(true);

            LoadEligibleFriends();
        }

        private void LoadEligibleFriends()
        {
            availableFriends.Clear();
            var allCharacters = Resources.LoadAll<CharacterSO>("Characters");

            string playerID = SaveManager.Instance?.SaveData?.selectedPlayerCharacterID ?? "";
            string currentHelperID = SaveManager.Instance?.SaveData?.selectedHelperCharacterID ?? "";

            foreach (var ch in allCharacters)
            {
                if (ch == null) continue;
                // Excluir al protagonista (Sección 10)
                if (!string.IsNullOrEmpty(playerID) && ch.characterID.Equals(playerID, System.StringComparison.OrdinalIgnoreCase)) continue;
                // Excluir al ayudante que ya está contratado
                if (!string.IsNullOrEmpty(currentHelperID) && ch.characterID.Equals(currentHelperID, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (!ch.selectableAsHelper) continue;

                availableFriends.Add(ch);
            }

            pendingSelectedFriend = availableFriends.Count > 0 ? availableFriends[0] : null;
            pendingSelectedOutfit = CharacterOutfit.ChefWhite;

            PopulateFriendsGrid();
            UpdateSelectionDisplay();
        }

        private void PopulateFriendsGrid()
        {
            if (friendsGridContainer == null) return;

            // Limpiar hijos previos
            for (int i = friendsGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(friendsGridContainer.GetChild(i).gameObject);
            }

            foreach (var friend in availableFriends)
            {
                bool isCurrentlyCustomer = CustomerManager.Instance != null && CustomerManager.Instance.IsFriendCurrentlyCustomer(friend.characterID);

                GameObject cardGO = new GameObject($"Card_{friend.characterID}");
                cardGO.transform.SetParent(friendsGridContainer, false);

                RectTransform rt = cardGO.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 120);

                Image bg = cardGO.AddComponent<Image>();
                bg.color = isCurrentlyCustomer ? new Color(0.25f, 0.25f, 0.25f, 0.7f) : new Color(0.18f, 0.22f, 0.30f, 0.95f);

                Button btn = cardGO.AddComponent<Button>();
                btn.interactable = !isCurrentlyCustomer;

                // Preview normal del Friend
                GameObject imgGO = new GameObject("Icon");
                imgGO.transform.SetParent(cardGO.transform, false);
                Image img = imgGO.AddComponent<Image>();
                img.sprite = friend.GetPreviewSprite(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                img.preserveAspect = true;
                RectTransform imgRT = imgGO.GetComponent<RectTransform>();
                imgRT.anchoredPosition = new Vector2(0f, 15f);
                imgRT.sizeDelta = new Vector2(64, 64);

                // Nombre
                GameObject nameGO = new GameObject("Name");
                nameGO.transform.SetParent(cardGO.transform, false);
                Text nameTxt = nameGO.AddComponent<Text>();
                nameTxt.text = isCurrentlyCustomer ? $"{friend.displayName}\n(En restaurante)" : friend.displayName;
                nameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameTxt.fontSize = 11;
                nameTxt.alignment = TextAnchor.MiddleCenter;
                nameTxt.color = isCurrentlyCustomer ? Color.gray : Color.white;
                RectTransform nameRT = nameGO.GetComponent<RectTransform>();
                nameRT.anchoredPosition = new Vector2(0f, -38f);
                nameRT.sizeDelta = new Vector2(95, 30);

                var captured = friend;
                btn.onClick.AddListener(() =>
                {
                    pendingSelectedFriend = captured;
                    UpdateSelectionDisplay();
                });
            }
        }

        private void SetPendingOutfit(CharacterOutfit outfit)
        {
            pendingSelectedOutfit = outfit;
            UpdateSelectionDisplay();
        }

        private void UpdateSelectionDisplay()
        {
            if (pendingSelectedFriend != null)
            {
                if (selectionNameText != null) selectionNameText.text = pendingSelectedFriend.displayName;
                if (selectionPreviewImage != null)
                {
                    selectionPreviewImage.sprite = pendingSelectedFriend.GetPreviewSprite(pendingSelectedOutfit, allowCrossOutfitFallback: true);
                }
                if (confirmSelectionBtn != null) confirmSelectionBtn.interactable = true;
            }
            else
            {
                if (selectionNameText != null) selectionNameText.text = "Selecciona un amigo";
                if (selectionPreviewImage != null) selectionPreviewImage.sprite = null;
                if (confirmSelectionBtn != null) confirmSelectionBtn.interactable = false;
            }
        }

        private void OnConfirmSelectionClicked()
        {
            if (pendingSelectedFriend == null) return;

            string oldHelperID = SaveManager.Instance?.SaveData?.selectedHelperCharacterID ?? "";

            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                var data = SaveManager.Instance.SaveData;
                data.selectedHelperCharacterID = pendingSelectedFriend.characterID;
                data.helperChefOutfit = pendingSelectedOutfit;
                data.helperIntroTriggered = true;
                data.helperSelectionSkipped = false;
                SaveManager.Instance.SaveGame();
            }

            // Aplicar nuevo helper a la escena de restaurante
            HelperIntroDialogUI.ApplyHelperToRestaurant(pendingSelectedFriend, pendingSelectedOutfit);

            Debug.Log($"[StaffMenuUI] ¡Relevo completado! Antiguo ayudante '{oldHelperID}' reincorporado a comensales. Nuevo ayudante: '{pendingSelectedFriend.displayName}' con uniforme {pendingSelectedOutfit}.");

            OpenOverviewPanel();
        }

        private void CreateFallbackMenuUI()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject root = new GameObject("StaffMenu_Fallback");
            root.transform.SetParent(canvas.transform, false);
            menuRoot = root;

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(620, 420);
            rt.anchoredPosition = Vector2.zero;

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.12f, 0.16f, 0.98f);

            // Overview Panel
            overviewPanel = new GameObject("OverviewPanel");
            overviewPanel.transform.SetParent(root.transform, false);
            RectTransform ovRT = overviewPanel.AddComponent<RectTransform>();
            ovRT.sizeDelta = rt.sizeDelta;

            // Title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(overviewPanel.transform, false);
            Text titleTxt = titleGO.AddComponent<Text>();
            titleTxt.text = "<b>PERSONAL Y EQUIPO DE COCINA</b>";
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 20;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1f, 0.85f, 0.3f);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0f, 170f);
            titleRT.sizeDelta = new Vector2(500, 40);

            // Preview
            GameObject prevGO = new GameObject("Preview");
            prevGO.transform.SetParent(overviewPanel.transform, false);
            currentHelperPreview = prevGO.AddComponent<Image>();
            currentHelperPreview.preserveAspect = true;
            RectTransform prevRT = prevGO.GetComponent<RectTransform>();
            prevRT.anchoredPosition = new Vector2(-150f, 20f);
            prevRT.sizeDelta = new Vector2(160, 200);

            // Name
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(overviewPanel.transform, false);
            currentHelperNameText = nameGO.AddComponent<Text>();
            currentHelperNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            currentHelperNameText.fontSize = 22;
            currentHelperNameText.alignment = TextAnchor.MiddleLeft;
            currentHelperNameText.color = Color.white;
            RectTransform nRT = nameGO.GetComponent<RectTransform>();
            nRT.anchoredPosition = new Vector2(80f, 60f);
            nRT.sizeDelta = new Vector2(280, 40);

            // Status
            GameObject statGO = new GameObject("Status");
            statGO.transform.SetParent(overviewPanel.transform, false);
            currentHelperStatusText = statGO.AddComponent<Text>();
            currentHelperStatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            currentHelperStatusText.fontSize = 15;
            currentHelperStatusText.alignment = TextAnchor.MiddleLeft;
            currentHelperStatusText.color = new Color(0.8f, 0.8f, 0.8f);
            RectTransform stRT = statGO.GetComponent<RectTransform>();
            stRT.anchoredPosition = new Vector2(80f, 10f);
            stRT.sizeDelta = new Vector2(280, 50);

            // Change Outfit Buttons
            changeOutfitBlackBtn = CreateButton(overviewPanel.transform, new Vector2(0f, -50f), new Vector2(130, 40), "Uniforme Negro", new Color(0.2f, 0.2f, 0.2f));
            changeOutfitWhiteBtn = CreateButton(overviewPanel.transform, new Vector2(140f, -50f), new Vector2(130, 40), "Uniforme Blanco", new Color(0.85f, 0.85f, 0.85f));
            Text wTxt = changeOutfitWhiteBtn.GetComponentInChildren<Text>();
            if (wTxt != null) wTxt.color = Color.black;

            // Change Helper Btn
            changeHelperBtn = CreateButton(overviewPanel.transform, new Vector2(-70f, -120f), new Vector2(180, 45), "CAMBIAR AYUDANTE", new Color(0.2f, 0.65f, 0.35f));

            // Dismiss Helper Btn
            dismissHelperBtn = CreateButton(overviewPanel.transform, new Vector2(120f, -120f), new Vector2(160, 45), "RETIRAR", new Color(0.75f, 0.25f, 0.25f));

            // Close Btn
            closeBtn = CreateButton(overviewPanel.transform, new Vector2(260f, 175f), new Vector2(40, 40), "X", new Color(0.4f, 0.4f, 0.4f));

            // Selection Panel
            selectionPanel = new GameObject("SelectionPanel");
            selectionPanel.transform.SetParent(root.transform, false);
            RectTransform selRT = selectionPanel.AddComponent<RectTransform>();
            selRT.sizeDelta = rt.sizeDelta;

            GameObject selTitle = new GameObject("SelTitle");
            selTitle.transform.SetParent(selectionPanel.transform, false);
            Text sTitleTxt = selTitle.AddComponent<Text>();
            sTitleTxt.text = "<b>SELECCIONA UN AMIGO DE LA VILLA</b>";
            sTitleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sTitleTxt.fontSize = 18;
            sTitleTxt.alignment = TextAnchor.MiddleCenter;
            sTitleTxt.color = Color.white;
            RectTransform sTitleRT = selTitle.GetComponent<RectTransform>();
            sTitleRT.anchoredPosition = new Vector2(0f, 170f);
            sTitleRT.sizeDelta = new Vector2(450, 35);

            // Container Grid
            GameObject gridGO = new GameObject("GridContainer");
            gridGO.transform.SetParent(selectionPanel.transform, false);
            friendsGridContainer = gridGO.transform;
            RectTransform gridRT = gridGO.AddComponent<RectTransform>();
            gridRT.anchoredPosition = new Vector2(-90f, 10f);
            gridRT.sizeDelta = new Vector2(360, 240);
            GridLayoutGroup glg = gridGO.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(100, 110);
            glg.spacing = new Vector2(10, 10);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            // Selection Preview & Details
            GameObject sPrevGO = new GameObject("SelPreview");
            sPrevGO.transform.SetParent(selectionPanel.transform, false);
            selectionPreviewImage = sPrevGO.AddComponent<Image>();
            selectionPreviewImage.preserveAspect = true;
            RectTransform sPrevRT = sPrevGO.GetComponent<RectTransform>();
            sPrevRT.anchoredPosition = new Vector2(180f, 50f);
            sPrevRT.sizeDelta = new Vector2(120, 140);

            GameObject sNameGO = new GameObject("SelName");
            sNameGO.transform.SetParent(selectionPanel.transform, false);
            selectionNameText = sNameGO.AddComponent<Text>();
            selectionNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            selectionNameText.fontSize = 16;
            selectionNameText.alignment = TextAnchor.MiddleCenter;
            selectionNameText.color = Color.white;
            RectTransform snRT = sNameGO.GetComponent<RectTransform>();
            snRT.anchoredPosition = new Vector2(180f, -30f);
            snRT.sizeDelta = new Vector2(160, 30);

            selectOutfitBlackBtn = CreateButton(selectionPanel.transform, new Vector2(120f, -80f), new Vector2(90, 35), "Negro", new Color(0.2f, 0.2f, 0.2f));
            selectOutfitWhiteBtn = CreateButton(selectionPanel.transform, new Vector2(220f, -80f), new Vector2(90, 35), "Blanco", new Color(0.85f, 0.85f, 0.85f));
            Text swTxt = selectOutfitWhiteBtn.GetComponentInChildren<Text>();
            if (swTxt != null) swTxt.color = Color.black;

            confirmSelectionBtn = CreateButton(selectionPanel.transform, new Vector2(170f, -140f), new Vector2(160, 45), "CONFIRMAR", new Color(0.2f, 0.65f, 0.35f));
            cancelSelectionBtn = CreateButton(selectionPanel.transform, new Vector2(-150f, -140f), new Vector2(140, 45), "VOLVER", new Color(0.5f, 0.5f, 0.5f));

            HookButtons();
            OpenOverviewPanel();
        }

        private Button CreateButton(Transform parent, Vector2 pos, Vector2 size, string text, Color color)
        {
            GameObject btnGO = new GameObject(text + "Btn");
            btnGO.transform.SetParent(parent, false);
            Image img = btnGO.AddComponent<Image>();
            img.color = color;
            Button btn = btnGO.AddComponent<Button>();
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            GameObject tGO = new GameObject("Label");
            tGO.transform.SetParent(btnGO.transform, false);
            Text t = tGO.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 14;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            RectTransform trt = tGO.GetComponent<RectTransform>();
            trt.sizeDelta = size;

            return btn;
        }
    }
}
