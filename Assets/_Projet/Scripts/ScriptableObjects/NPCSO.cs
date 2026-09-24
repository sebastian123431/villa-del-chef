using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewNPC", menuName = "VillaDelChef/NPC")]
    public class NPCSO : ScriptableObject
    {
        [Header("Identity")]
        public string npcID;
        public string npcName;
        public string roleTitle;

        [Header("Character Reference (Friends Art)")]
        public CharacterSO characterReference;

        [Header("Art")]
        public Sprite portrait;
        public Sprite worldSprite;

        [Header("Animation (Optional)")]
        public RuntimeAnimatorController animatorController;

        [Header("Dialogue")]
        [TextArea(2, 4)]
        public string greetingDialogue = "¡Hola! Bienvenido a mi puesto en Villa del Chef.";

        public Sprite GetPortrait()
        {
            if (characterReference != null && characterReference.normalPreview != null)
            {
                return characterReference.normalPreview;
            }
            return portrait != null ? portrait : worldSprite;
        }

        public Sprite GetWorldSprite()
        {
            if (characterReference != null && characterReference.normalPreview != null)
            {
                return characterReference.normalPreview;
            }
            return worldSprite != null ? worldSprite : portrait;
        }

        public RuntimeAnimatorController GetAnimator()
        {
            if (characterReference != null && characterReference.normalAnimator != null)
            {
                return characterReference.normalAnimator;
            }
            return animatorController;
        }

        [Header("Associated Vendor Catalog")]
        public VendorSO vendorData;

        [Header("Progression")]
        public int unlockLevel = 1;
    }
}
