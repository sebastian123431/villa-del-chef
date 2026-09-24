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
        /// Realiza fallback seguro si alguna variante de chef no está disponible.
        /// </summary>
        public Sprite GetPreviewSprite(CharacterOutfit outfit)
        {
            switch (outfit)
            {
                case CharacterOutfit.ChefBlack:
                    return blackChefPreview != null ? blackChefPreview : (whiteChefPreview != null ? whiteChefPreview : normalPreview);
                case CharacterOutfit.ChefWhite:
                    return whiteChefPreview != null ? whiteChefPreview : (blackChefPreview != null ? blackChefPreview : normalPreview);
                case CharacterOutfit.Normal:
                default:
                    return normalPreview != null ? normalPreview : (blackChefPreview != null ? blackChefPreview : whiteChefPreview);
            }
        }

        /// <summary>
        /// Retorna el Animator Controller correspondiente al vestuario solicitado.
        /// Realiza fallback seguro al uniforme disponible o a ropa normal.
        /// </summary>
        public RuntimeAnimatorController GetAnimator(CharacterOutfit outfit)
        {
            switch (outfit)
            {
                case CharacterOutfit.ChefBlack:
                    return blackChefAnimator != null ? blackChefAnimator : (whiteChefAnimator != null ? whiteChefAnimator : normalAnimator);
                case CharacterOutfit.ChefWhite:
                    return whiteChefAnimator != null ? whiteChefAnimator : (blackChefAnimator != null ? blackChefAnimator : normalAnimator);
                case CharacterOutfit.Normal:
                default:
                    return normalAnimator != null ? normalAnimator : (blackChefAnimator != null ? blackChefAnimator : whiteChefAnimator);
            }
        }
    }
}
