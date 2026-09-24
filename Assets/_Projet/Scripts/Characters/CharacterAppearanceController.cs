using UnityEngine;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Characters
{
    /// <summary>
    /// Componente reutilizable para aplicar apariencia y animaciones según CharacterSO y CharacterOutfit (Fase 7).
    /// Compatible con el protagonista (Player) y los empleados/ayudantes (Workers).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterAppearanceController : MonoBehaviour
    {
        [Header("Current Configuration")]
        [SerializeField] private CharacterSO currentCharacter;
        [SerializeField] private CharacterOutfit currentOutfit = CharacterOutfit.ChefBlack;

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        public CharacterSO CurrentCharacter => currentCharacter;
        public CharacterOutfit CurrentOutfit => currentOutfit;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Aplica un personaje y vestuario específicos, actualizando preview estático y AnimatorController.
        /// </summary>
        public void ApplyCharacter(CharacterSO character, CharacterOutfit outfit)
        {
            if (character == null)
            {
                Debug.LogWarning("[CharacterAppearanceController] Intentando aplicar un CharacterSO nulo.");
                return;
            }

            currentCharacter = character;
            currentOutfit = outfit;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();

            // 1. Asignar Sprite de fallback / preview
            Sprite previewSprite = character.GetPreviewSprite(outfit);
            if (previewSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = previewSprite;
            }

            // 2. Asignar Animator Controller emparejado exactamente
            RuntimeAnimatorController runtimeController = character.GetAnimator(outfit);
            if (runtimeController != null)
            {
                if (animator == null) animator = GetComponent<Animator>() ?? gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = runtimeController;
                animator.enabled = true;
            }
            else if (animator != null)
            {
                animator.enabled = false;
            }
        }

        /// <summary>
        /// Cambia únicamente el vestuario del personaje actual (ej: Alternar ChefBlack <-> ChefWhite).
        /// </summary>
        public void SetOutfit(CharacterOutfit outfit)
        {
            if (currentCharacter != null)
            {
                ApplyCharacter(currentCharacter, outfit);
            }
            else
            {
                currentOutfit = outfit;
            }
        }
    }
}
