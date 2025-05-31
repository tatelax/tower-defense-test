using System;
using System.Collections.Generic;
using Types;

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
    static int[,] directions = new int[,] { {0,1}, {1,0}, {0,-1}, {-1,0} };

    public static List<(int x, int y)> FindPath(Tile[,] map, (int x, int y) start, (int x, int y) goal)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        var open = new SortedSet<(int priority, int count, Node node)>();
        int counter = 0;
        var closed = new HashSet<(int, int)>();

        var startNode = new Node(start.x, start.y, 0, Heuristic(start.x, start.y, goal.x, goal.y));
        open.Add((startNode.Priority, counter++, startNode));

        while (open.Count > 0)
        {
            var current = open.Min.node;
            open.Remove(open.Min);

            if ((current.X, current.Y) == goal)
                return ReconstructPath(current);

            closed.Add((current.X, current.Y));

            for (int i = 0; i < 4; i++)
            {
                int nx = current.X + directions[i,0];
                int ny = current.Y + directions[i,1];

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;
                if (!map[nx, ny].IsWalkable)
                    continue;
                if (closed.Contains((nx, ny)))
                    continue;

                int cost = current.Cost + 1;
                var neighbor = new Node(nx, ny, cost, cost + Heuristic(nx, ny, goal.x, goal.y), current);
                open.Add((neighbor.Priority, counter++, neighbor));
            }
        }
        return null;
    }

    static int Heuristic(int x, int y, int gx, int gy)
        => Math.Abs(x - gx) + Math.Abs(y - gy); // Manhattan distance

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
}