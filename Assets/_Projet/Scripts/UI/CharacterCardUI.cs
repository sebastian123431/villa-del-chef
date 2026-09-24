using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    /// <summary>
    /// Componente UI reutilizable para tarjetas de selección de personajes (Friends)
    /// utilizado en HelperIntroDialogUI, StaffMenuUI y pantallas de elenco social (Fase 7.0.2 / 7.0.3 — Secciones 14–23).
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        [SerializeField] private Image previewImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button selectButton;

        public Image PreviewImage => previewImage;
        public Text NameText => nameText;
        public Text StatusText => statusText;
        public Button SelectButton => selectButton;

        public bool HasValidReferences => previewImage != null && nameText != null && statusText != null && selectButton != null;
        public bool IsConfigured { get; private set; }

        /// <summary>
        /// Asigna referencias internas manualmente si la tarjeta fue instanciada dinámicamente sin prefab.
        /// </summary>
        public void SetReferences(Image preview, Text name, Text status, Button button)
        {
            previewImage = preview;
            nameText = name;
            statusText = status;
            selectButton = button;
        }

        /// <summary>
        /// Intenta auto-vincular referencias faltantes por convención de nombres controlada:
        /// - Hijo 'Icon': Image del preview
        /// - Hijo 'Name': Text del nombre
        /// - Hijo 'Status': Text del estado
        /// - Root o hijo 'Button': Componente Button
        /// </summary>
        public bool TryAutoBindReferences()
        {
            if (previewImage == null)
            {
                var iconChild = transform.Find("Icon");
                if (iconChild != null) previewImage = iconChild.GetComponent<Image>();
            }

            if (nameText == null)
            {
                var nameChild = transform.Find("Name");
                if (nameChild != null) nameText = nameChild.GetComponent<Text>();
            }

            if (statusText == null)
            {
                var statusChild = transform.Find("Status");
                if (statusChild != null) statusText = statusChild.GetComponent<Text>();
            }

            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
                if (selectButton == null)
                {
                    var btnChild = transform.Find("Button");
                    if (btnChild != null) selectButton = btnChild.GetComponent<Button>();
                }
            }

            return HasValidReferences;
        }

        /// <summary>
        /// Enlaza los datos del personaje a los elementos visuales de la tarjeta.
        /// Retorna true si el binding fue exitoso, false si las referencias están incompletas o corruptas.
        /// </summary>
        public bool Bind(
            CharacterSO character,
            bool isUnavailable,
            System.Action<CharacterSO> onSelected
        )
        {
            if (character == null)
            {
                IsConfigured = false;
                return false;
            }

            if (!HasValidReferences)
            {
                TryAutoBindReferences();
            }

            if (!HasValidReferences)
            {
                Debug.LogError($"[CharacterCardUI] Tarjeta '{gameObject.name}' mal configurada: faltan referencias requeridas (Preview={previewImage != null}, Name={nameText != null}, Status={statusText != null}, Button={selectButton != null}).");
                IsConfigured = false;
                return false;
            }

            // REGLA CRÍTICA: Los comensales/amigos siempre se muestran en preview con ropa normal sin fallback a chef
            previewImage.sprite = character.GetPreviewSprite(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
            previewImage.preserveAspect = true;
            previewImage.color = isUnavailable ? new Color(0.6f, 0.6f, 0.6f, 0.7f) : Color.white;

            nameText.text = character.displayName;
            nameText.color = isUnavailable ? Color.gray : Color.white;

            statusText.text = isUnavailable ? "En restaurante" : "";
            statusText.gameObject.SetActive(isUnavailable);

            selectButton.interactable = !isUnavailable;
            selectButton.onClick.RemoveAllListeners();
            if (!isUnavailable && onSelected != null)
            {
                selectButton.onClick.AddListener(() => onSelected(character));
            }

            IsConfigured = true;
            return true;
        }

        /// <summary>
        /// Genera una tarjeta procedural completa con jerarquía y layout correctos.
        /// Usado como fallback si no existe prefab o si el prefab asignado está corrupto.
        /// </summary>
        public static GameObject CreateProceduralCard(
            Transform parent,
            CharacterSO character,
            bool isUnavailable,
            System.Action<CharacterSO> onSelected
        )
        {
            GameObject cardGO = new GameObject($"Card_{character.characterID}");
            cardGO.transform.SetParent(parent, false);

            Image cardBg = cardGO.AddComponent<Image>();
            cardBg.color = isUnavailable ? new Color(0.25f, 0.25f, 0.25f, 0.7f) : new Color(0.18f, 0.22f, 0.28f);

            var cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(100f, 130f);

            Button btn = cardGO.AddComponent<Button>();

            // Imagen preview
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(cardGO.transform, false);
            Image icon = iconGO.AddComponent<Image>();
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.6f);
            iconRT.anchorMax = new Vector2(0.5f, 0.6f);
            iconRT.sizeDelta = new Vector2(64f, 64f);

            // Nombre
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(cardGO.transform, false);
            Text nameTxt = nameGO.AddComponent<Text>();
            nameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameTxt.fontSize = 11;
            nameTxt.alignment = TextAnchor.MiddleCenter;
            var nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0.15f);
            nameRT.anchorMax = new Vector2(1f, 0.4f);
            nameRT.sizeDelta = Vector2.zero;

            // Estado
            GameObject statusGO = new GameObject("Status");
            statusGO.transform.SetParent(cardGO.transform, false);
            Text statusTxt = statusGO.AddComponent<Text>();
            statusTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusTxt.fontSize = 9;
            statusTxt.alignment = TextAnchor.MiddleCenter;
            statusTxt.color = new Color(0.9f, 0.7f, 0.2f);
            var statusRT = statusGO.GetComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0f, 0f);
            statusRT.anchorMax = new Vector2(1f, 0.15f);
            statusRT.sizeDelta = Vector2.zero;

            var cardUI = cardGO.AddComponent<CharacterCardUI>();
            cardUI.SetReferences(icon, nameTxt, statusTxt, btn);
            cardUI.Bind(character, isUnavailable, onSelected);

            return cardGO;
        }
    }
}
