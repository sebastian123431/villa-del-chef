using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Building
{
    public class BuildManager : MonoBehaviour
    {
        public static BuildManager Instance { get; private set; }

        [Header("State")]
        public bool isBuildMode = false;
        public FurnitureSO selectedFurniture;
        public int currentRotation = 0; // 0, 90, 180, 270

        [Header("Ghost Preview")]
        public GameObject ghostPreviewInstance;
        public Material validPlacementMaterial;
        public Material invalidPlacementMaterial;

        [Header("Runtime Furniture Tracking")]
        public List<GridObject> activeFurniture = new List<GridObject>();

        private GridObject movingObject = null;
        private Vector2Int currentHoverGrid;
        private bool isCurrentPosValid = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ToggleBuildMode()
        {
            SetBuildMode(!isBuildMode);
        }

        public void SetBuildMode(bool enable)
        {
            isBuildMode = enable;
            if (!isBuildMode)
            {
                CancelSelection();
            }
            GameEvents.TriggerBuildModeToggled(isBuildMode);
        }

        public void SelectFurnitureToBuild(FurnitureSO furniture)
        {
            if (!isBuildMode) SetBuildMode(true);

            selectedFurniture = furniture;
            movingObject = null;
            currentRotation = 0;
            CreateOrUpdateGhost();
        }

        public void StartMovingObject(GridObject obj)
        {
            if (obj == null) return;
            if (!isBuildMode) SetBuildMode(true);

            movingObject = obj;
            selectedFurniture = obj.furnitureData;
            currentRotation = obj.rotationDegrees;

            obj.UnregisterFromGrid();
            obj.gameObject.SetActive(false);

            CreateOrUpdateGhost();
        }

        public void RotateSelection()
        {
            currentRotation = (currentRotation + 90) % 360;
            if (ghostPreviewInstance != null)
            {
                ghostPreviewInstance.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
                UpdateGhostPosition(currentHoverGrid);
            }
        }

        public void UpdateHoverPosition(Vector3 worldPos)
        {
            if (selectedFurniture == null && movingObject == null) return;
            if (GridManager.Instance == null) return;

            Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
            currentHoverGrid = gridPos;
            UpdateGhostPosition(gridPos);
        }

        private void UpdateGhostPosition(Vector2Int gridPos)
        {
            if (ghostPreviewInstance == null || selectedFurniture == null) return;

            int sizeX = (currentRotation % 180 == 0) ? selectedFurniture.sizeX : selectedFurniture.sizeY;
            int sizeY = (currentRotation % 180 == 0) ? selectedFurniture.sizeY : selectedFurniture.sizeX;

            ghostPreviewInstance.transform.position = GridManager.Instance.GridToWorld(gridPos, sizeX, sizeY);

            bool areaAvailable = GridManager.Instance.IsAreaAvailable(gridPos.x, gridPos.y, sizeX, sizeY, movingObject);
            bool zoneAllowed = GridManager.Instance.IsAreaInAllowedZones(gridPos.x, gridPos.y, sizeX, sizeY, selectedFurniture.allowedZones);
            bool isAreaUnlocked = GridManager.Instance.IsAreaUnlocked(gridPos.x, gridPos.y, sizeX, sizeY);
            isCurrentPosValid = areaAvailable && zoneAllowed && isAreaUnlocked;

            // Update ghost tint / color
            SpriteRenderer[] renderers = ghostPreviewInstance.GetComponentsInChildren<SpriteRenderer>();
            Color tintColor = isCurrentPosValid ? new Color(0.2f, 1f, 0.3f, 0.65f) : new Color(1f, 0.2f, 0.2f, 0.65f);
            foreach (var r in renderers)
            {
                r.color = tintColor;
            }
        }

        public bool TryPlaceObject()
        {
            if (selectedFurniture == null) return false;
            if (!isCurrentPosValid) return false;

            int sizeX = (currentRotation % 180 == 0) ? selectedFurniture.sizeX : selectedFurniture.sizeY;
            int sizeY = (currentRotation % 180 == 0) ? selectedFurniture.sizeY : selectedFurniture.sizeX;

            // Verify navigation safety: do not block critical restaurant pathways
            if (!ValidateNavigationSafety(currentHoverGrid.x, currentHoverGrid.y, sizeX, sizeY))
            {
                Debug.LogWarning("[BuildManager] Este objeto bloquearía el paso.");
                return false;
            }

            // If buying new object, check economy
            if (movingObject == null)
            {
                if (EconomyManager.Instance != null && !EconomyManager.Instance.HasCoins(selectedFurniture.cost))
                {
                    Debug.Log("[BuildManager] Not enough coins!");
                    return false;
                }
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.SpendCoins(selectedFurniture.cost);
                }
            }


            GameObject spawnedObj = null;
            GridObject gridObj = null;

            if (movingObject != null)
            {
                spawnedObj = movingObject.gameObject;
                spawnedObj.SetActive(true);
                gridObj = movingObject;
                movingObject = null;
            }
            else
            {
                if (selectedFurniture.prefab != null)
                {
                    spawnedObj = Instantiate(selectedFurniture.prefab);
                }
                else
                {
                    spawnedObj = new GameObject(selectedFurniture.furnitureName);
                    SpriteRenderer sr = spawnedObj.AddComponent<SpriteRenderer>();
                    sr.sprite = selectedFurniture.shopIcon;
                }

                gridObj = spawnedObj.GetComponent<GridObject>();
                if (gridObj == null)
                {
                    gridObj = spawnedObj.AddComponent<GridObject>();
                }
                activeFurniture.Add(gridObj);
            }

            gridObj.Setup(selectedFurniture, currentHoverGrid, currentRotation);
            GameEvents.TriggerFurniturePlaced(selectedFurniture, currentHoverGrid);
            GameEvents.TriggerQuestProgressMade(QuestType.BuyFurniture, selectedFurniture.furnitureID, 1);

            CancelSelection();
            return true;
        }

        public void SellObject(GridObject obj)
        {
            if (obj == null || obj.furnitureData == null) return;

            int refund = obj.furnitureData.sellPrice;
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddCoins(refund);
            }

            activeFurniture.Remove(obj);
            obj.UnregisterFromGrid();
            GameEvents.TriggerFurnitureSold(obj.furnitureData);
            Destroy(obj.gameObject);
        }

        public void CancelSelection()
        {
            if (movingObject != null)
            {
                movingObject.gameObject.SetActive(true);
                movingObject.RegisterWithGrid();
                movingObject.UpdateWorldPosition();
                movingObject = null;
            }

            selectedFurniture = null;
            if (ghostPreviewInstance != null)
            {
                Destroy(ghostPreviewInstance);
                ghostPreviewInstance = null;
            }
        }

        private void CreateOrUpdateGhost()
        {
            if (ghostPreviewInstance != null)
            {
                Destroy(ghostPreviewInstance);
            }

            if (selectedFurniture == null) return;

            ghostPreviewInstance = new GameObject("GhostPreview");
            SpriteRenderer sr = ghostPreviewInstance.AddComponent<SpriteRenderer>();
            sr.sprite = selectedFurniture.shopIcon;
            sr.sortingOrder = 100;
            ghostPreviewInstance.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
        }

        private bool ValidateNavigationSafety(int startX, int startY, int sizeX, int sizeY)
        {
            if (GridManager.Instance == null) return true;

            // Temporarily mark candidate cells as unwalkable
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int y = startY; y < startY + sizeY; y++)
                {
                    var cell = GridManager.Instance.GetCell(x, y);
                    if (cell != null) cell.isWalkable = false;
                }
            }

            bool isSafe = true;

            // If DeliveryCounter exists, ensure it can still reach all active tables
            if (VillaDelChef.Restaurant.DeliveryCounter.Instance != null)
            {
                Vector2Int counterPos = VillaDelChef.Restaurant.DeliveryCounter.Instance.gridPosition;
                foreach (var obj in activeFurniture)
                {
                    if (obj is VillaDelChef.Restaurant.Table table && obj.gameObject.activeInHierarchy)
                    {
                        var path = Utilities.GridPathfinding.FindPath(counterPos, table.gridPosition);
                        if (path == null || path.Count == 0)
                        {
                            isSafe = false;
                            break;
                        }
                    }
                }
            }

            // Restore candidate cells
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int y = startY; y < startY + sizeY; y++)
                {
                    var cell = GridManager.Instance.GetCell(x, y);
                    if (cell != null) cell.isWalkable = true;
                }
            }

            return isSafe;
        }
    }
}

