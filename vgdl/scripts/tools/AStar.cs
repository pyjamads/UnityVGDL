
/**
 * Created by dperez on 13/01/16.
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar
{
    public static List<PathNode> closedList, openList;
    public Dictionary<int, List<PathNode>> pathCache;
    public PathFinder pathfinder;
    public bool[][] visited;

    public AStar(PathFinder pathfinder)
    {
        this.pathfinder = pathfinder;
        pathCache = new Dictionary<int, List<PathNode>>();
        visited = new bool[pathfinder.grid.Length][];
        for (int i = 0; i < visited.Length; i++)
        {
            visited[i] = new bool[pathfinder.grid[0].Length];
        }
    }

    public void emptyCache()
    {
        pathCache.Clear();

    }

    private static float heuristicEstimatedCost(PathNode curNode, PathNode goalNode)
    {
        //4-way: using Manhattan
        var xDiff = Mathf.Abs(curNode.position.x - goalNode.position.x);
        var yDiff = Mathf.Abs(curNode.position.y - goalNode.position.y);
        return xDiff + yDiff;

        //This is Euclidean distance(sub-optimal here).
        //return curNode.position.dist(goalNode.position);
    }


    private List<PathNode> calculatePath(PathNode node)
    {
        var path = new List<PathNode>();
        while(node != null)
        {
            if(node.parent != null) //to avoid adding the start node.
            {
                node.setMoveDir(node.parent);
                path.Insert(0, node);
            }
            node = node.parent;
        }
        return path;
    }

    public List<PathNode> getPath(PathNode start, PathNode goal)
    {
        var pathId = start.id * 10000 + goal.id;
        
        return pathCache.ContainsKey(pathId) ? pathCache[pathId] : null;
    }

    public List<PathNode> findPath(PathNode start, PathNode goal)
    {
        if(goal != null)
        {
            var pathId = start.id * 10000 + goal.id;
            if (pathCache.ContainsKey(pathId))
            {
                return pathCache[pathId];
            }
            var path = _findPath(start, goal);

            if (path != null)
            {
                pathCache.Add(pathId, path);
            }

            return path;
        }

        dijkstraa(start);
        return null;
    }


    private void dijkstraa(PathNode start)
    {

        var destinationsFromStart = new List<PathNode>();
        //All unvisited at the beginning.
        visited = new bool[pathfinder.grid.Length][];
        for (var i = 0; i < visited.Length; i++)
        {
            visited[i] = new bool[pathfinder.grid[0].Length];
        }
        
        //...except the starting node
        visited[(int)start.position.x][(int)start.position.y] = true;

        PathNode node = null;

        openList = new List<PathNode>();
        start.totalCost = 0.0f;

        openList.Enqueue(start);

        while(openList.Count != 0)
        {
            node = openList.Dequeue();
            //System.out.println("Remaining in list: " + openList.size());

            if(!destinationsFromStart.Contains(node) && node.Equals(start))
            {
                destinationsFromStart.Add(node);
            }

            var neighbours = pathfinder.getNeighbours(node);

            foreach (var neighbour in neighbours)
            {
                var curDistance = neighbour.totalCost;
                if (!visited[(int)neighbour.position.x][(int)neighbour.position.y])
                {
                    visited[(int)neighbour.position.x][(int)neighbour.position.y] = true;
                    neighbour.totalCost = curDistance + node.totalCost;
                    neighbour.parent = node;
                    openList.Enqueue(neighbour);

                }else if(curDistance + node.totalCost < neighbour.totalCost)
                {
                    neighbour.totalCost = curDistance + node.totalCost;
                    neighbour.parent = node;
                }
            }
        }


        foreach(var dest in destinationsFromStart)
        {
            var pathid = start.id * 10000 + dest.id;
            pathCache.Add(pathid, calculatePath(dest));
        }
    }

    private List<PathNode> _findPath(PathNode start, PathNode goal)
    {
        PathNode node = null;
        openList = new List<PathNode>();
        closedList = new List<PathNode>();

        start.totalCost = 0.0f;
        start.estimatedCost = heuristicEstimatedCost(start, goal);

        openList.Enqueue(start);

        while(openList.Count != 0)
        {
            node = openList.Dequeue();
            closedList.Enqueue(node);

            if(node.Equals(goal))
                return calculatePath(node);

            var neighbours = pathfinder.getNeighbours(node);

            foreach (var neighbour in neighbours)
            {
                var curDistance = neighbour.totalCost;

                if(!openList.Contains(neighbour) && !closedList.Contains(neighbour))
                {
                    neighbour.totalCost = curDistance + node.totalCost;
                    neighbour.estimatedCost = heuristicEstimatedCost(neighbour, goal);
                    neighbour.parent = node;

                    openList.Enqueue(neighbour);

                }else if(curDistance + node.totalCost < neighbour.totalCost)
                {
                    neighbour.totalCost = curDistance + node.totalCost;
                    neighbour.parent = node;

                    if (openList.Contains(neighbour))
                    {
                        openList.Remove(neighbour);
                    }

                    if (closedList.Contains(neighbour))
                    {
                        closedList.Remove(neighbour);
                    }

                    openList.Enqueue(neighbour);
                }
            }

        }

        if (!node.Equals(goal))
        {
            return null;
        }

        return calculatePath(node);

    }

    private int[][] uncompressPathId(int pathId)
    {
        var ends = new int[2][];
        for (var i = 0; i < ends.Length; i++)
        {
            ends[i] = new int[2];
        }

        var org =  pathId / 10000;
        var dest = pathId % 10000;

        ends[0] = new[]{org/100 , org%100};
        ends[1] = new[]{dest/100 , dest%100};
        return ends;
    }

    public void printPath(int pathId, List<PathNode> nodes)
    {
        if(nodes == null)
        {
            Debug.Log("No Path");
            return;
        }

        var endsIds = uncompressPathId(pathId);

        var ends = "(" + endsIds[0][0] + "," + endsIds[0][1] + ") -> (" + endsIds[1][0] + "," + endsIds[1][1] + ")";


        Debug.Log("Path " + ends + "; ("+ nodes.Count + "): ");
        foreach(var n in nodes)
        {
            Debug.Log(n.position.x + ":" + n.position.y + ", ");
        }
        Debug.Log("\n");
    }
}
