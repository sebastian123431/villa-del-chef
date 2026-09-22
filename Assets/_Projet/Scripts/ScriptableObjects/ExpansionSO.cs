using UnityEngine;
using VillaDelChef.Building;

namespace VillaDelChef
{
    [CreateAssetMenu(fileName = "NewExpansion", menuName = "Villa del Chef/Expansion", order = 60)]
    public class ExpansionSO : ScriptableObject
    {
        [Header("Identity")]
        public string expansionID;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Zone & Grid Dimensions")]
        public ZoneType targetZone = ZoneType.Terrace;
        public RectInt gridBounds;

        [Header("Requirements")]
        public int requiredRestaurantLevel = 1;
        public int costGold = 250;

        [Header("Rewards")]
        public int rewardXP = 50;

        [Header("World Sign Placement")]
        public Vector2 worldSignPosition;
    }
}
