using System;
using Types;
using Systems;

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
        Unit unit,
        Tile[,] map,
        (int x, int y) start,
        (int x, int y) goal,
        (int x, int y)[] outPath,
        bool excludeLastTile = false
    )
    {
        int width = MapSystem.SizeX;
        int height = MapSystem.SizeY;

        if (closed == null || closed.GetLength(0) != width || closed.GetLength(1) != height)
        {
            closed = new bool[width, height];
            cameFrom = new int[width, height];
        }
        Array.Clear(closed, 0, closed.Length);

        openCount = 0;
        int nodeCount = 0;

        int goalX = goal.x, goalY = goal.y;

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
                // Exclude last tile (goal) if requested
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
                    if (!(IsWalkable(map, x1, y1, unit) && IsWalkable(map, x2, y2, unit)))
                        continue;
                }

                if (!IsWalkable(map, nx, ny, unit))
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

    private static bool IsWalkable(Tile[,] map, int x, int y, Unit self)
    {
        var t = map[x, y];
        return t.IsWalkable && (t.Unit == null || t.Unit == self);
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