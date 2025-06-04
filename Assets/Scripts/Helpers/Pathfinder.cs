using System;
using System.Collections.Generic;
using Types;
using Systems;
using UnityEngine;
using System.Linq;

public static class FastPathfinder
{
    private const int MaxNodes = 4096;

    struct Node
    {
        public int x, y, g, h, f, parentIndex;
        public void Set(int x, int y, int g, int h, int parentIndex)
        {
            this.x = x; this.y = y; this.g = g; this.h = h;
            this.f = g + h;
            this.parentIndex = parentIndex;
        }
    }

    private static Node[] nodePool = new Node[MaxNodes];
    private static int[] openHeap = new int[MaxNodes];
    private static int openCount;
    private static bool[,] closed;
    private static int[,] cameFrom;

    // 8-way
    private static readonly (int dx, int dy)[] Directions = new (int, int)[]
    {
        (0,1), (1,1), (1,0), (1,-1), (0,-1), (-1,-1), (-1,0), (-1,1)
    };

    public static int FindPath(
        MapSystem mapSystem,
        Unit unit,
        (int x, int y) start,
        (int x, int y) goal,
        (int x, int y)[] outPath,
        bool excludeLastTile = false
    )
    {
        int width = MapSystem.SizeX;
        int height = MapSystem.SizeY;

        // --- New logic: Is goal tile occupied by another unit? If so, find closest edge tile instead ---
        Unit goalUnit = null;
        if (goal.x >= 0 && goal.x < width && goal.y >= 0 && goal.y < height)
            goalUnit = mapSystem.Map[goal.x, goal.y].Unit;
        bool goalBlockedByOtherUnit = goalUnit != null && goalUnit != unit;

        (int x, int y) newGoal = goal;
        if (goalBlockedByOtherUnit)
        {
            // Get all tiles covered by the goal unit
            var covered = mapSystem.GetTilesCovered(goalUnit.CurrTile, goalUnit.Radius);
            var coveredSet = new HashSet<(int, int)>(covered);

            // Find all unique neighbor tiles around the covered tiles
            var candidateTiles = new HashSet<(int, int)>();
            foreach (var (x, y) in covered)
            {
                foreach (var (dx, dy) in Directions)
                {
                    (int x, int y) neighbor = (x + dx, y + dy);
                    if (neighbor.x < 0 || neighbor.x >= width || neighbor.y < 0 || neighbor.y >= height)
                        continue;
                    // Must NOT be inside the covered area, must be open for our unit
                    if (!coveredSet.Contains(neighbor) && mapSystem.IsTileOpen(neighbor, unit.Radius))
                    {
                        candidateTiles.Add(neighbor);
                    }
                }
            }

            // If no candidates found, pathfinding will fail as expected
            if (candidateTiles.Count > 0)
            {
                // Find the candidate closest to the original goal tile (can change to closest to start if you want)
                newGoal = candidateTiles
                    .OrderBy(tile => Heuristic(tile.Item1, tile.Item2, goal.x, goal.y))
                    .First();
            }
            // else newGoal remains goal, but there is nowhere legal to stand
        }

        // --- Normal A* logic below ---
        if (closed == null || closed.GetLength(0) != width || closed.GetLength(1) != height)
        {
            closed = new bool[width, height];
            cameFrom = new int[width, height];
        }
        Array.Clear(closed, 0, closed.Length);

        openCount = 0;
        int nodeCount = 0;

        int goalX = newGoal.x, goalY = newGoal.y;
        int unitRadius = unit.Radius;

        nodePool[0].Set(start.x, start.y, 0, Heuristic(start.x, start.y, goalX, goalY), -1);
        openHeap[openCount++] = 0;
        cameFrom[start.x, start.y] = -1;
        nodeCount = 1;

        while (openCount > 0)
        {
            int currentIndex = PopMinHeap();
            Node current = nodePool[currentIndex];

            if (current.x == goalX && current.y == goalY)
            {
                int len = 0;
                int n = currentIndex;
                while (n >= 0 && len < outPath.Length)
                {
                    outPath[len++] = (nodePool[n].x, nodePool[n].y);
                    n = nodePool[n].parentIndex;
                }
                // Reverse
                for (int i = 0; i < len / 2; i++)
                {
                    var tmp = outPath[i];
                    outPath[i] = outPath[len - i - 1];
                    outPath[len - i - 1] = tmp;
                }
                if (excludeLastTile && len > 1)
                {
                    len -= 1;
                }
                return len;
            }

            closed[current.x, current.y] = true;

            for (int d = 0; d < Directions.Length; d++)
            {
                int dx = Directions[d].dx, dy = Directions[d].dy;
                int nx = current.x + dx, ny = current.y + dy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;
                if (closed[nx, ny])
                    continue;

                // Diagonal movement: check for corner cutting
                if (dx != 0 && dy != 0)
                {
                    int x1 = current.x + dx, y1 = current.y;
                    int x2 = current.x, y2 = current.y + dy;
                    if (!(IsWalkableWithGoalCheck(mapSystem, (x1, y1), unit, (goalX, goalY)) &&
                          IsWalkableWithGoalCheck(mapSystem, (x2, y2), unit, (goalX, goalY))))
                        continue;
                }

                if (!IsWalkableWithGoalCheck(mapSystem, (nx, ny), unit, (goalX, goalY)))
                    continue;

                int g = current.g + ((dx == 0 || dy == 0) ? 10 : 14);
                int h = Heuristic(nx, ny, goalX, goalY);
                int f = g + h;

                // Already in open with lower f?
                bool skip = false;
                for (int i = 0; i < openCount; i++)
                {
                    int idx = openHeap[i];
                    if (nodePool[idx].x == nx && nodePool[idx].y == ny && nodePool[idx].f <= f)
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip) continue;

                if (nodeCount >= MaxNodes)
                    break;

                nodePool[nodeCount].Set(nx, ny, g, h, currentIndex);
                openHeap[openCount++] = nodeCount;
                nodeCount++;
            }
        }

        UnityEngine.Debug.LogWarning($"[Pathfinder] No path found from {start} to {goal}.");
        return 0;
    }

    /// <summary>
    /// Returns false if the candidate pos is not open for this unit's radius,
    /// OR if the candidate is the goal tile and overlaps another unit.
    /// </summary>
    private static bool IsWalkableWithGoalCheck(MapSystem mapSystem, (int x, int y) pos, Unit movingUnit, (int x, int y) goal)
    {
        if (!mapSystem.IsTileOpen(pos, movingUnit.Radius))
            return false;
        if (pos == goal)
        {
            var tiles = mapSystem.GetTilesCovered(pos, movingUnit.Radius);
            foreach (var tile in tiles)
            {
                var u = mapSystem.Map[tile.x, tile.y].Unit;
                if (u != null && u != movingUnit)
                    return false;
            }
        }
        return true;
    }

    private static int Heuristic(int x1, int y1, int x2, int y2)
    {
        int dx = Math.Abs(x1 - x2), dy = Math.Abs(y1 - y2);
        return 10 * Math.Max(dx, dy);
    }

    private static int PopMinHeap()
    {
        int best = 0;
        int bestF = nodePool[openHeap[0]].f;
        for (int i = 1; i < openCount; i++)
        {
            int f = nodePool[openHeap[i]].f;
            if (f < bestF)
            {
                best = i;
                bestF = f;
            }
        }
        int result = openHeap[best];
        openCount--;
        openHeap[best] = openHeap[openCount];
        return result;
    }
}