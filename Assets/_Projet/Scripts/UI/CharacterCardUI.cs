using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    /// <summary>
    /// Componente UI reutilizable para tarjetas de selección de personajes (Friends)
    /// utilizado en HelperIntroDialogUI, StaffMenuUI y pantallas de elenco social (Fase 7.0.2 — Secciones 17–21).
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

        /// <summary>
        /// Asigna referencias internas si la tarjeta fue instanciada dinámicamente sin prefab.
        /// </summary>
        public void SetReferences(Image preview, Text name, Text status, Button button)
        {
            previewImage = preview;
            nameText = name;
            statusText = status;
            selectButton = button;
        }

        /// <summary>
        /// Enlaza los datos del personaje a los elementos visuales de la tarjeta.
        /// </summary>
        public void Bind(
            CharacterSO character,
            bool isUnavailable,
            System.Action<CharacterSO> onSelected
        )
        {
            if (character == null) return;

            if (previewImage != null)
            {
                // REGLA CRÍTICA: Los comensales/amigos siempre se muestran en preview con ropa normal sin fallback a chef
                previewImage.sprite = character.GetPreviewSprite(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                previewImage.preserveAspect = true;
                previewImage.color = isUnavailable ? new Color(0.6f, 0.6f, 0.6f, 0.7f) : Color.white;
            }

            if (nameText != null)
            {
                nameText.text = character.displayName;
                nameText.color = isUnavailable ? Color.gray : Color.white;
            }

            if (statusText != null)
            {
                statusText.text = isUnavailable ? "En restaurante" : "";
                statusText.gameObject.SetActive(isUnavailable);
            }

            if (selectButton != null)
            {
                selectButton.interactable = !isUnavailable;
                selectButton.onClick.RemoveAllListeners();
                if (!isUnavailable && onSelected != null)
                {
                    selectButton.onClick.AddListener(() => onSelected(character));
                }
            }
        }
    }
}
