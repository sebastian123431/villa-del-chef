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

        [Header("Art")]
        public Sprite portrait;
        public Sprite worldSprite;

        [Header("Dialogue")]
        [TextArea(2, 4)]
        public string greetingDialogue = "¡Hola! Bienvenido a mi puesto en Villa del Chef.";

        [Header("Associated Vendor Catalog")]
        public VendorSO vendorData;

        [Header("Progression")]
        public int unlockLevel = 1;
    }
}
