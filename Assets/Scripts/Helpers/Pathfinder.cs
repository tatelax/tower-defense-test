using System;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class Node
{
    public int X, Y, Cost, Priority;
    public Node Parent;

    public Node(int x, int y, int cost, int priority, Node parent = null)
    {
        X = x; Y = y; Cost = cost; Priority = priority; Parent = parent;
    }
}

public class Pathfinder
{
    static int[,] directions = new int[,]
    {
        { 0, 1 },  { 1, 1 },  { 1, 0 },  { 1, -1 },
        { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }
    };

    public static List<(int x, int y)> FindPath(Unit unit, Tile[,] map, (int x, int y) start, (int x, int y) goal)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        var open = new SortedSet<(int priority, int count, Node node)>();
        int counter = 0;
        var closed = new HashSet<(int, int)>();

        var startNode = new Node(start.x, start.y, 0, Heuristic(start.x, start.y, goal.x, goal.y));
        open.Add((startNode.Priority, counter++, startNode));

        Node furthestNode = null;
        int bestHeuristic = int.MaxValue;

        while (open.Count > 0)
        {
            var current = open.Min.node;
            open.Remove(open.Min);

            if ((current.X, current.Y) == goal)
            {
                if (IsAreaOpen(map, goal.x, goal.y, unit.Radius)) // Goal is open
                {
                    return ReconstructPath(current);
                }
            }

            closed.Add((current.X, current.Y));

            int h = Heuristic(current.X, current.Y, goal.x, goal.y);
            if (h < bestHeuristic)
            {
                bestHeuristic = h;
                furthestNode = current;
            }

            for (int i = 0; i < 8; i++)
            {
                int nx = current.X + directions[i, 0];
                int ny = current.Y + directions[i, 1];

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                bool isSelf = (nx == start.x && ny == start.y);
                if (!IsAreaOpen(map, nx, ny, unit.Radius, isSelf ? unit : null))
                    continue;

                if (closed.Contains((nx, ny)))
                    continue;

                // Prevent diagonal corner cutting
                if (i % 2 == 1)
                {
                    int sx = current.X + directions[i, 0];
                    int sy = current.Y;
                    int ex = current.X;
                    int ey = current.Y + directions[i, 1];
                    if (!IsInBounds(sx, sy, width, height) || !IsInBounds(ex, ey, width, height)) continue;
                    if (!IsAreaOpen(map, sx, sy, unit.Radius, isSelf ? unit : null) || !IsAreaOpen(map, ex, ey, unit.Radius, isSelf ? unit : null))
                        continue;
                }

                int cost = current.Cost + 1;
                var neighbor = new Node(nx, ny, cost, cost + Heuristic(nx, ny, goal.x, goal.y), current);
                open.Add((neighbor.Priority, counter++, neighbor));
            }
        }

        return furthestNode != null ? ReconstructPath(furthestNode) : null;
    }

    static int Heuristic(int x, int y, int gx, int gy)
        => Math.Max(Math.Abs(x - gx), Math.Abs(y - gy));

    static List<(int x, int y)> ReconstructPath(Node node)
    {
        var path = new List<(int, int)>();
        while (node != null)
        {
            path.Add((node.X, node.Y));
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    static bool IsInBounds(int x, int y, int width, int height)
        => x >= 0 && x < width && y >= 0 && y < height;

    static bool IsAreaOpen(Tile[,] map, int cx, int cy, int radius, Unit ignoreUnit = null)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int r2 = radius * radius;
        for (int dx = -radius + 1; dx < radius; dx++)
        {
            for (int dy = -radius + 1; dy < radius; dy++)
            {
                int tx = cx + dx;
                int ty = cy + dy;
                if (tx < 0 || tx >= width || ty < 0 || ty >= height)
                    return false;
                if (dx * dx + dy * dy >= r2)
                    continue;
                if (!map[tx, ty].IsWalkable)
                    return false;
                if (map[tx, ty].Unit != null && map[tx, ty].Unit != ignoreUnit)
                    return false;
            }
        }
        return true;
    }
}