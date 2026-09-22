using UnityEngine;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Building
{
    public class GridObject : MonoBehaviour
    {
        [Header("Data Reference")]
        public FurnitureSO furnitureData;

        [Header("Placement State")]
        public Vector2Int gridPosition;
        public int rotationDegrees = 0; // 0, 90, 180, 270

        [Header("Settings")]
        public bool blocksWalkability = true;

        public int CurrentSizeX => (rotationDegrees % 180 == 0) ? (furnitureData != null ? furnitureData.sizeX : 1) : (furnitureData != null ? furnitureData.sizeY : 1);
        public int CurrentSizeY => (rotationDegrees % 180 == 0) ? (furnitureData != null ? furnitureData.sizeY : 1) : (furnitureData != null ? furnitureData.sizeX : 1);

        public virtual void Setup(FurnitureSO data, Vector2Int pos, int rotation = 0)
        {
            this.furnitureData = data;
            this.gridPosition = pos;
            this.rotationDegrees = rotation;

            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            UpdateWorldPosition();
            RegisterWithGrid();
        }

        public void UpdateWorldPosition()
        {
            if (GridManager.Instance != null)
            {
                transform.position = GridManager.Instance.GridToWorld(gridPosition, CurrentSizeX, CurrentSizeY);
            }
        }

        public void RegisterWithGrid()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.SetOccupancy(gridPosition.x, gridPosition.y, CurrentSizeX, CurrentSizeY, this, blocksWalkability);
            }
        }

        public void UnregisterFromGrid()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.SetOccupancy(gridPosition.x, gridPosition.y, CurrentSizeX, CurrentSizeY, null, false);
            }
        }

        private void OnDestroy()
        {
            UnregisterFromGrid();
        }
    }
}
