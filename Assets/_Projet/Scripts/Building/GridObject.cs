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
        public bool playerMovable = true;
        public int overrideSizeX = 1;
        public int overrideSizeY = 1;

        public int CurrentSizeX
        {
            get
            {
                int baseW = (furnitureData != null) ? furnitureData.sizeX : Mathf.Max(1, overrideSizeX);
                int baseH = (furnitureData != null) ? furnitureData.sizeY : Mathf.Max(1, overrideSizeY);
                return (rotationDegrees % 180 == 0) ? baseW : baseH;
            }
        }

        public int CurrentSizeY
        {
            get
            {
                int baseW = (furnitureData != null) ? furnitureData.sizeX : Mathf.Max(1, overrideSizeX);
                int baseH = (furnitureData != null) ? furnitureData.sizeY : Mathf.Max(1, overrideSizeY);
                return (rotationDegrees % 180 == 0) ? baseH : baseW;
            }
        }

        public virtual void Setup(FurnitureSO data, Vector2Int pos, int rotation = 0)
        {
            this.furnitureData = data;
            this.playerMovable = true;
            this.gridPosition = pos;
            this.rotationDegrees = rotation;

            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            UpdateWorldPosition();
            RegisterWithGrid();
        }

        public virtual void SetupStatic(Vector2Int pos, int sizeX, int sizeY, bool blocks = true)
        {
            this.furnitureData = null;
            this.playerMovable = false;
            this.gridPosition = pos;
            this.overrideSizeX = sizeX;
            this.overrideSizeY = sizeY;
            this.rotationDegrees = 0;
            this.blocksWalkability = blocks;

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
