using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    /// <summary>
    /// Convención oficial de vestuarios de Villa del Chef:
    /// Normal = rnormal (Ropa casual de calle)
    /// ChefBlack = rnchef (Uniforme NEGRO de chef)
    /// ChefWhite = rbchef (Uniforme BLANCO de chef)
    /// </summary>
    public enum CharacterOutfit
    {
        Normal,
        ChefBlack,
        ChefWhite
    }

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "VillaDelChef/Character Definition")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Identificador único del personaje (ej: alex, conny, dafne, diego_serena)")]
        public string characterID;
        [Tooltip("Nombre legible para mostrar en pantalla y diálogos")]
        public string displayName;

        [Header("Selection Flags")]
        [Tooltip("Indica si el jugador puede elegirlo como protagonista de la partida")]
        public bool selectableAsPlayer = true;
        [Tooltip("Indica si puede ser contratado como ayudante / camarero en el restaurante")]
        public bool selectableAsHelper = true;
        [Tooltip("Indica si puede visitar el restaurante como cliente/comensal social")]
        public bool canAppearAsCustomer = true;

        [Header("Static Previews (Emparejados exactamente por Outfit)")]
        [Tooltip("Preview estático de cuerpo entero con ropa normal (<nombre>_rnormal.png)")]
        public Sprite normalPreview;
        [Tooltip("Preview estático de cuerpo entero con uniforme negro de chef (<nombre>_rnchef.png)")]
        public Sprite blackChefPreview;
        [Tooltip("Preview estático de cuerpo entero con uniforme blanco de chef (<nombre>_rbchef.png)")]
        public Sprite whiteChefPreview;
        [Tooltip("Retrato para diálogos y medallón (opcional)")]
        public Sprite portrait;

        [Header("Movement Animators (Emparejados exactamente por Outfit)")]
        [Tooltip("Animator Controller con ropa normal (movimientos_rnormal.png)")]
        public RuntimeAnimatorController normalAnimator;
        [Tooltip("Animator Controller con uniforme negro de chef (movimientos_rnchef.png)")]
        public RuntimeAnimatorController blackChefAnimator;
        [Tooltip("Animator Controller con uniforme blanco de chef (movimientos_rbchef.png)")]
        public RuntimeAnimatorController whiteChefAnimator;

        [Header("Lore & Description")]
        [TextArea(2, 4)]
        public string description;

        [Header("Audio (Opcional - Sonidos neutrales o clips específicos)")]
        public AudioClip selectionVoiceClip;
        public AudioClip celebrateVoiceClip;

        /// <summary>
        /// Retorna el sprite de preview correspondiente al vestuario solicitado.
        /// Si allowCrossOutfitFallback es false (obligatorio para Customers), nunca se mezclan prendas de chef con ropa normal.
        /// </summary>
        public Sprite GetPreviewSprite(CharacterOutfit outfit, bool allowCrossOutfitFallback = true)
        {
            switch (outfit)
            {
                case CharacterOutfit.ChefBlack:
                    if (blackChefPreview != null) return blackChefPreview;
                    return allowCrossOutfitFallback ? (whiteChefPreview != null ? whiteChefPreview : normalPreview) : null;
                case CharacterOutfit.ChefWhite:
                    if (whiteChefPreview != null) return whiteChefPreview;
                    return allowCrossOutfitFallback ? (blackChefPreview != null ? blackChefPreview : normalPreview) : null;
                case CharacterOutfit.Normal:
                default:
                    if (normalPreview != null) return normalPreview;
                    // REGLA CRÍTICA: Los clientes NUNCA usan ropa de chef. Si allowCrossOutfitFallback es false, retornar null.
                    return allowCrossOutfitFallback ? (blackChefPreview != null ? blackChefPreview : whiteChefPreview) : null;
            }
        }

        /// <summary>
        /// Retorna el Animator Controller correspondiente al vestuario solicitado.
        /// Si allowCrossOutfitFallback es false (obligatorio para Customers), nunca se asigna un animador de chef a comensales.
        /// </summary>
        public RuntimeAnimatorController GetAnimator(CharacterOutfit outfit, bool allowCrossOutfitFallback = true)
        {
            switch (outfit)
            {
                case CharacterOutfit.ChefBlack:
                    if (blackChefAnimator != null) return blackChefAnimator;
                    return allowCrossOutfitFallback ? (whiteChefAnimator != null ? whiteChefAnimator : normalAnimator) : null;
                case CharacterOutfit.ChefWhite:
                    if (whiteChefAnimator != null) return whiteChefAnimator;
                    return allowCrossOutfitFallback ? (blackChefAnimator != null ? blackChefAnimator : normalAnimator) : null;
                case CharacterOutfit.Normal:
                default:
                    if (normalAnimator != null) return normalAnimator;
                    // REGLA CRÍTICA: Los clientes NUNCA usan animaciones de chef. Si allowCrossOutfitFallback es false, retornar null.
                    return allowCrossOutfitFallback ? (blackChefAnimator != null ? blackChefAnimator : whiteChefAnimator) : null;
            }
        }
    }
}
