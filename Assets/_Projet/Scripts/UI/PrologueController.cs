using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    /// <summary>
    /// Controlador del Prólogo y Creación de Identidad del Jugador (Fase 7).
    /// Flujo:
    /// 1. Narrador: "¿Cómo te llamas?" (Validación nombre 1-20 chars).
    /// 2. Selector de Protagonista con preview casual (rnormal).
    /// 3. Confirmación obligatoria de personaje permanente.
    /// 4. Elección de Uniforme de Cocina: CHEF NEGRO vs CHEF BLANCO.
    /// 5. Guardado e ingreso al restaurante (02_Restaurant).
    /// </summary>
    public class PrologueController : MonoBehaviour
    {
        public static PrologueController Instance { get; private set; }

        [Header("Screens / Steps")]
        public GameObject nameInputPanel;
        public GameObject characterSelectPanel;
        public GameObject confirmCharacterPanel;
        public GameObject outfitSelectPanel;
        public GameObject welcomeStoryPanel;

        [Header("1. Name Input")]
        public InputField nameInputField;
        public Button submitNameBtn;
        public Text nameErrorText;

        [Header("2. Character Selection (rnormal)")]
        public Image characterPreviewImage;
        public Text characterNameText;
        public Text characterLoreText;
        public Button prevCharacterBtn;
        public Button nextCharacterBtn;
        public Button selectCharacterBtn;

        [Header("3. Confirmation Modal")]
        public Text confirmPromptText;
        public Button confirmCharacterBtn;
        public Button backToSelectionBtn;

        [Header("4. Outfit Selection")]
        public Image blackUniformPreviewImage;
        public Image whiteUniformPreviewImage;
        public Button chooseBlackOutfitBtn;
        public Button chooseWhiteOutfitBtn;

        [Header("5. Welcome Story Outro")]
        public Text welcomeStoryText;
        public Button enterRestaurantBtn;

        [Header("Navigation")]
        public string restaurantSceneName = "02_Restaurant";

        private List<CharacterSO> availableCharacters = new List<CharacterSO>();
        private int currentCharacterIndex = 0;
        private string validatedPlayerName = "Chef";
        private CharacterSO selectedCharacter;
        private CharacterOutfit selectedOutfit = CharacterOutfit.ChefBlack;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            LoadAvailableCharacters();
            HookUIEvents();
            ShowStep(1);
        }

        private void LoadAvailableCharacters()
        {
            availableCharacters.Clear();
            var loaded = Resources.LoadAll<CharacterSO>("Characters");
            foreach (var ch in loaded)
            {
                if (ch != null && ch.selectableAsPlayer)
                {
                    availableCharacters.Add(ch);
                }
            }

            // Fallback si no hay ScriptableObjects aún creados en tiempo de ejecución
            if (availableCharacters.Count == 0)
            {
                Debug.LogWarning("[PrologueController] No se encontraron CharacterSO en Resources/Characters. Se creará fallback temporal.");
                var fallback = ScriptableObject.CreateInstance<CharacterSO>();
                fallback.characterID = "alex";
                fallback.displayName = "Alex";
                availableCharacters.Add(fallback);
            }
        }

        private void HookUIEvents()
        {
            if (submitNameBtn != null) submitNameBtn.onClick.AddListener(OnSubmitName);
            if (prevCharacterBtn != null) prevCharacterBtn.onClick.AddListener(OnPrevCharacter);
            if (nextCharacterBtn != null) nextCharacterBtn.onClick.AddListener(OnNextCharacter);
            if (selectCharacterBtn != null) selectCharacterBtn.onClick.AddListener(OnSelectCharacterClicked);
            if (confirmCharacterBtn != null) confirmCharacterBtn.onClick.AddListener(OnConfirmCharacter);
            if (backToSelectionBtn != null) backToSelectionBtn.onClick.AddListener(OnBackToCharacterSelection);
            if (chooseBlackOutfitBtn != null) chooseBlackOutfitBtn.onClick.AddListener(() => OnOutfitChosen(CharacterOutfit.ChefBlack));
            if (chooseWhiteOutfitBtn != null) chooseWhiteOutfitBtn.onClick.AddListener(() => OnOutfitChosen(CharacterOutfit.ChefWhite));
            if (enterRestaurantBtn != null) enterRestaurantBtn.onClick.AddListener(OnEnterRestaurant);
        }

        public void ShowStep(int step)
        {
            if (nameInputPanel != null) nameInputPanel.SetActive(step == 1);
            if (characterSelectPanel != null) characterSelectPanel.SetActive(step == 2);
            if (confirmCharacterPanel != null) confirmCharacterPanel.SetActive(step == 3);
            if (outfitSelectPanel != null) outfitSelectPanel.SetActive(step == 4);
            if (welcomeStoryPanel != null) welcomeStoryPanel.SetActive(step == 5);

            if (step == 2)
            {
                UpdateCharacterDisplay();
            }
            else if (step == 4)
            {
                UpdateOutfitDisplay();
            }
        }

        // ==========================================
        // PASO 1: NOMBRE
        // ==========================================
        private void OnSubmitName()
        {
            string entered = nameInputField != null ? nameInputField.text.Trim() : "";
            if (string.IsNullOrEmpty(entered))
            {
                if (nameErrorText != null) nameErrorText.text = "Por favor ingresa un nombre para tu Chef.";
                return;
            }

            if (entered.Length > 20)
            {
                entered = entered.Substring(0, 20);
            }

            validatedPlayerName = entered;
            ShowStep(2);
        }

        // ==========================================
        // PASO 2: PERSONAJE (Preview rnormal)
        // ==========================================
        private void OnPrevCharacter()
        {
            if (availableCharacters.Count == 0) return;
            currentCharacterIndex = (currentCharacterIndex - 1 + availableCharacters.Count) % availableCharacters.Count;
            UpdateCharacterDisplay();
        }

        private void OnNextCharacter()
        {
            if (availableCharacters.Count == 0) return;
            currentCharacterIndex = (currentCharacterIndex + 1) % availableCharacters.Count;
            UpdateCharacterDisplay();
        }

        private void UpdateCharacterDisplay()
        {
            if (availableCharacters.Count == 0) return;
            var ch = availableCharacters[currentCharacterIndex];
            if (characterNameText != null) characterNameText.text = ch.displayName;
            if (characterLoreText != null) characterLoreText.text = string.IsNullOrEmpty(ch.description) ? "Un entusiasta cocinero listo para hacer historia." : ch.description;

            // Regla oficial: En el selector se muestra la ropa normal (rnormal)
            if (characterPreviewImage != null)
            {
                characterPreviewImage.sprite = ch.GetPreviewSprite(CharacterOutfit.Normal);
            }
        }

        private void OnSelectCharacterClicked()
        {
            if (availableCharacters.Count == 0) return;
            selectedCharacter = availableCharacters[currentCharacterIndex];

            if (confirmPromptText != null)
            {
                confirmPromptText.text = $"¿Elegir a <b>{selectedCharacter.displayName}</b>?\n\nEste será el protagonista de tu historia.\nNo podrás cambiarlo durante esta partida.";
            }

            ShowStep(3);
        }

        // ==========================================
        // PASO 3: CONFIRMACIÓN PERMANENTE
        // ==========================================
        private void OnConfirmCharacter()
        {
            ShowStep(4);
        }

        private void OnBackToCharacterSelection()
        {
            ShowStep(2);
        }

        // ==========================================
        // PASO 4: ELECCIÓN DE UNIFORME CHEF
        // ==========================================
        private void UpdateOutfitDisplay()
        {
            if (selectedCharacter == null) return;

            // Mostrar el sprite de uniforme negro (rnchef)
            if (blackUniformPreviewImage != null)
            {
                blackUniformPreviewImage.sprite = selectedCharacter.GetPreviewSprite(CharacterOutfit.ChefBlack);
            }

            // Mostrar el sprite de uniforme blanco (rbchef)
            if (whiteUniformPreviewImage != null)
            {
                whiteUniformPreviewImage.sprite = selectedCharacter.GetPreviewSprite(CharacterOutfit.ChefWhite);
            }
        }

        private void OnOutfitChosen(CharacterOutfit outfit)
        {
            selectedOutfit = outfit;

            if (welcomeStoryText != null)
            {
                string outfitDesc = (outfit == CharacterOutfit.ChefBlack) ? "elegante uniforme negro" : "inmaculado uniforme blanco";
                welcomeStoryText.text = $"¡Bienvenido a Villa del Chef, <b>{validatedPlayerName}</b>!\n\nCon tu {outfitDesc}, las puertas del restaurante están listas para abrirse al pueblo.\n\nPrepara tus fogones y comencemos.";
            }

            ShowStep(5);
        }

        // ==========================================
        // PASO 5: INGRESO AL JUEGO Y GUARDADO
        // ==========================================
        private void OnEnterRestaurant()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                var data = SaveManager.Instance.SaveData;
                data.playerName = validatedPlayerName;
                data.selectedPlayerCharacterID = selectedCharacter != null ? selectedCharacter.characterID : "alex";
                data.selectedChefOutfit = selectedOutfit;
                data.playerCharacterLocked = true;
                data.prologueCompleted = true;
                data.prologueStep = 5;
                data.restaurantOpen = true; // Abre con la gran inauguración
                SaveManager.Instance.SaveGame();
            }

            SceneManager.LoadScene(restaurantSceneName);
        }
    }
}
