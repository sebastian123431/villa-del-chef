using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;

namespace VillaDelChef.Utilities
{
    public class PathNode
    {
        public int x;
        public int y;
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;
        public PathNode parent;

        public PathNode(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public static class GridPathfinding
    {
        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            GridManager grid = GridManager.Instance;
            if (grid == null) return null;

            if (!grid.IsInsideGrid(start.x, start.y) || !grid.IsInsideGrid(end.x, end.y))
            {
                return null;
            }

            // If target is blocked, find closest neighbor
            if (!grid.IsWalkable(end.x, end.y))
            {
                Vector2Int? alternate = GetWalkableNeighbor(end, grid);
                if (alternate.HasValue)
                {
                    end = alternate.Value;
                }
                else
                {
                    return null;
                }
            }

            List<PathNode> openList = new List<PathNode>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, PathNode> allNodes = new Dictionary<Vector2Int, PathNode>();

            PathNode startNode = new PathNode(start.x, start.y) { gCost = 0, hCost = CalculateDistance(start, end) };
            openList.Add(startNode);
            allNodes[start] = startNode;

            int iterations = 0;
            const int maxIterations = 800; // Mobile safety guard against infinite loops

            while (openList.Count > 0 && iterations++ < maxIterations)
            {
                // Get node with lowest fCost
                PathNode current = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].fCost < current.fCost || (openList[i].fCost == current.fCost && openList[i].hCost < current.hCost))
                    {
                        current = openList[i];
                    }
                }

                if (current.x == end.x && current.y == end.y)
                {
                    return RetracePath(startNode, current);
                }

                openList.Remove(current);
                closedSet.Add(new Vector2Int(current.x, current.y));

                foreach (Vector2Int neighborPos in GetNeighbors(current.x, current.y, grid))
                {
                    if (closedSet.Contains(neighborPos)) continue;

                    int tentativeGCost = current.gCost + 10;
                    PathNode neighbor;
                    if (!allNodes.TryGetValue(neighborPos, out neighbor))
                    {
                        neighbor = new PathNode(neighborPos.x, neighborPos.y);
                        allNodes[neighborPos] = neighbor;
                    }

                    if (tentativeGCost < neighbor.gCost || !openList.Contains(neighbor))
                    {
                        neighbor.gCost = tentativeGCost;
                        neighbor.hCost = CalculateDistance(neighborPos, end);
                        neighbor.parent = current;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return null; // Path not found
        }

        private static List<Vector2Int> GetNeighbors(int x, int y, GridManager grid)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0)
            };

            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                if (grid.IsInsideGrid(nx, ny) && grid.IsWalkable(nx, ny))
                {
                    neighbors.Add(new Vector2Int(nx, ny));
                }
            }
            return neighbors;
        }

        private static Vector2Int? GetWalkableNeighbor(Vector2Int pos, GridManager grid)
        {
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0)
            };

            foreach (var dir in directions)
            {
                int nx = pos.x + dir.x;
                int ny = pos.y + dir.y;
                if (grid.IsInsideGrid(nx, ny) && grid.IsWalkable(nx, ny))
                {
                    return new Vector2Int(nx, ny);
                }
            }
            return null;
        }

        private static int CalculateDistance(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) * 10;
        }

        private static List<Vector2Int> RetracePath(PathNode start, PathNode end)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            PathNode curr = end;
            while (curr != null && (curr.x != start.x || curr.y != start.y))
            {
                path.Add(new Vector2Int(curr.x, curr.y));
                curr = curr.parent;
            }
            path.Reverse();
            return path;
        }
    }
}
