using UnityEngine;
using VillaDelChef.Building;

namespace VillaDelChef.Restaurant
{
    public class Chair : GridObject
    {
        [Header("Chair State")]
        public bool isOccupied = false;
        public Table attachedTable;
        public Transform sitPosition;

        public Vector3 GetSitPosition()
        {
            return sitPosition != null ? sitPosition.position : transform.position;
        }

        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
        }
    }
}
