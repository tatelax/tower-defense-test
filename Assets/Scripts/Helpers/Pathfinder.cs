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
    // 8-way movement
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

        Node furthestNode = null; // Best node found closest to the goal
        int bestHeuristic = int.MaxValue;

        while (open.Count > 0)
        {
            var current = open.Min.node;
            open.Remove(open.Min);

            // If goal is reached and is walkable, return path
            if ((current.X, current.Y) == goal)
            {
                if (map[goal.x, goal.y].Unit == null) // goal is open
                {
                    return ReconstructPath(current);
                }
                // else: fall through, treat as blocked and find closest
            }

            closed.Add((current.X, current.Y));

            // Always track the closest node we've gotten to the goal
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
                if (!map[nx, ny].IsWalkable)
                    continue;

                // Treat ANY occupied tile (except our own position) as blocked
                // This includes the goal if it's occupied
                if (map[nx, ny].Unit != null && (nx != start.x || ny != start.y))
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
                    if (!map[sx, sy].IsWalkable || !map[ex, ey].IsWalkable)
                        continue;
                }

                int cost = current.Cost + 1;
                var neighbor = new Node(nx, ny, cost, cost + Heuristic(nx, ny, goal.x, goal.y), current);
                open.Add((neighbor.Priority, counter++, neighbor));
            }
        }

        // If we could not reach the goal, return the path to the furthest node found
        return furthestNode != null ? ReconstructPath(furthestNode) : null;
    }

    static int Heuristic(int x, int y, int gx, int gy)
        => Math.Max(Math.Abs(x - gx), Math.Abs(y - gy)); // Chebyshev for 8-way

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
}