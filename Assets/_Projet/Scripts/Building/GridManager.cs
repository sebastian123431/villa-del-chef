using System.Collections.Generic;
using UnityEngine;

namespace VillaDelChef.Building
{
    public enum ZoneType
    {
        Kitchen,
        Dining,
        Exterior,
        Farming,
        Market,
        Crafting,
        FarmingZone = Farming
    }

    public class GridCell
    {
        public int x;
        public int y;
        public bool isWalkable = true;
        public ZoneType zone = ZoneType.Dining;
        public GridObject occupyingObject = null;

        public GridCell(int x, int y, ZoneType zone = ZoneType.Dining)
        {
            this.x = x;
            this.y = y;
            this.zone = zone;
            this.isWalkable = true;
        }
    }


    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Dimensions")]
        public int gridWidth = 32;
        public int gridHeight = 24;
        public float cellSize = 1f;
        public Vector2 gridOrigin = new Vector2(-16f, -12f);

        [Header("Visual Debug")]
        public bool showDebugGrid = false;

        private GridCell[,] cells;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGrid();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGrid()
        {
            cells = new GridCell[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    ZoneType zone = ZoneType.Dining;
                    // Bottom 7 rows = Kitchen
                    if (y < 7)
                    {
                        zone = ZoneType.Kitchen;
                    }
                    // Top 8 rows = Exterior / Farming
                    else if (y >= 16)
                    {
                        zone = ZoneType.Exterior;
                    }

                    cells[x, y] = new GridCell(x, y, zone);
                }
            }
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
            int y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorld(Vector2Int gridPos, int sizeX = 1, int sizeY = 1)
        {
            float worldX = gridOrigin.x + (gridPos.x + sizeX * 0.5f) * cellSize;
            float worldY = gridOrigin.y + (gridPos.y + sizeY * 0.5f) * cellSize;
            return new Vector3(worldX, worldY, 0f);
        }

        public bool IsInsideGrid(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }

        public bool IsAreaAvailable(int startX, int startY, int sizeX, int sizeY, GridObject ignoreObject = null)
        {
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int y = startY; y < startY + sizeY; y++)
                {
                    if (!IsInsideGrid(x, y)) return false;
                    GridCell cell = cells[x, y];
                    if (cell.occupyingObject != null && cell.occupyingObject != ignoreObject)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void SetOccupancy(int startX, int startY, int sizeX, int sizeY, GridObject obj, bool blocksWalkability = true)
        {
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int y = startY; y < startY + sizeY; y++)
                {
                    if (IsInsideGrid(x, y))
                    {
                        cells[x, y].occupyingObject = obj;
                        cells[x, y].isWalkable = (obj == null) || !blocksWalkability;
                    }
                }
            }
        }

        public GridCell GetCell(int x, int y)
        {
            if (IsInsideGrid(x, y)) return cells[x, y];
            return null;
        }

        public bool IsWalkable(int x, int y)
        {
            if (!IsInsideGrid(x, y)) return false;
            return cells[x, y].isWalkable;
        }

        public ZoneType GetZoneAt(int x, int y)
        {
            if (!IsInsideGrid(x, y)) return ZoneType.Exterior;
            ZoneType z = cells[x, y].zone;
            return (z == ZoneType.FarmingZone) ? ZoneType.Farming : z;
        }

        public bool IsAreaInAllowedZones(int startX, int startY, int sizeX, int sizeY, List<ZoneType> allowedZones)
        {
            if (allowedZones == null || allowedZones.Count == 0) return true;

            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int y = startY; y < startY + sizeY; y++)
                {
                    if (!IsInsideGrid(x, y)) return false;
                    ZoneType current = GetZoneAt(x, y);

                    bool isMatch = false;
                    for (int i = 0; i < allowedZones.Count; i++)
                    {
                        ZoneType allowed = allowedZones[i];
                        if (allowed == ZoneType.FarmingZone) allowed = ZoneType.Farming;
                        if (allowed == current)
                        {
                            isMatch = true;
                            break;
                        }
                    }

                    if (!isMatch) return false;
                }
            }
            return true;
        }


        private void OnDrawGizmos()
        {
            if (!showDebugGrid) return;

            Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 from = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y, 0f);
                Vector3 to = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y + gridHeight * cellSize, 0f);
                Gizmos.DrawLine(from, to);
            }
            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 from = new Vector3(gridOrigin.x, gridOrigin.y + y * cellSize, 0f);
                Vector3 to = new Vector3(gridOrigin.x + gridWidth * cellSize, gridOrigin.y + y * cellSize, 0f);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}
