using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    public enum StationType
    {
        Cocina,
        Parrilla,
        Horno,
        Freidora,
        Cafetera,
        MesaPreparacion,
        Refrigerador,
        MesaEntrega
    }

    [CreateAssetMenu(fileName = "NewStation", menuName = "VillaDelChef/Station")]
    public class StationSO : ScriptableObject
    {
        [Header("Station Details")]
        public string stationID;
        public string stationName;
        public StationType stationType;
        public Sprite icon;
        public int unlockLevel = 1;
        [TextArea(2, 4)]
        public string description;
    }
}
