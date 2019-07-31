
/**
 * Created by dperez on 14/01/16.
 */

using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    public AStar astar;
    public StateObservation state;

    public bool VERBOSE = false;

    //All types are obstacles except the ones included in this array
    public List<string> obstacleStypes;

    public List<Observation>[][] grid;


    private static int[] x_arrNeig;
    private static int[] y_arrNeig;

    public PathFinder(List<string> obstacleStypes)
    {
        this.obstacleStypes = obstacleStypes;
    }

    public void run(StateObservation stateObs)
    {
        state = stateObs;
        grid = stateObs.getObservationGrid();
        astar = new AStar(this);

        init();
        runAll();

        if (VERBOSE)
        {
            foreach (var pathId in astar.pathCache.Keys)
            {
                var nodes = astar.pathCache[pathId];
                astar.printPath(pathId, nodes);
            }
        }
    }

    private void init()
    {
        if (x_arrNeig == null)
        {
            //TODO: This is a bit of a hack, it wouldn't work with other (new) action sets.
            var actions = state.getAvailableActions();
            if (actions.Count == 3)
            {
                //left, right
                x_arrNeig = new[] {-1, 1};
                y_arrNeig = new[] {0, 0};
            }
            else
            {
                //up, down, left, right
                x_arrNeig = new[] {0, 0, -1, 1};
                y_arrNeig = new[] {-1, 1, 0, 0};
            }
        }
    }

    private void runAll()
    {
        for (var i = 0; i < grid.Length; ++i)
        {
            for (var j = 0; j < grid[i].Length; ++j)
            {
                var obstacleCell = isObstacle(i, j);
                if (obstacleCell) continue;

                if (VERBOSE)
                {
                    Debug.Log("Running from (" + i + "," + j + ")");
                }
                runAll(i, j);
            }
        }
    }

    

    private void runAll(int i, int j)
    {
        var start = new PathNode(new Vector2(i, j));
        PathNode goal = null; //To get all routes.

        astar.findPath(start, goal);
    }

    public List<PathNode> getPath(Vector2 start, Vector2 end)
    {
        return astar.getPath(new PathNode(start), new PathNode(end));
    }

    private bool isObstacle(int row, int col)
    {
        if (row < 0 || row >= grid.Length) return true;
        if (col < 0 || col >= grid[row].Length) return true;

        foreach (var obs in grid[row][col])
        {
            if (obstacleStypes.Contains(obs.stype))
                return true;
        }

        return false;

    }

    public List<PathNode> getNeighbours(PathNode node)
    {
        var neighbours = new List<PathNode>();
        var x = (int) (node.position.x);
        var y = (int) (node.position.y);

        for (var i = 0; i < x_arrNeig.Length; ++i)
        {
            if (!isObstacle(x + x_arrNeig[i], y + y_arrNeig[i]))
            {
                neighbours.Add(new PathNode(new Vector2(x + x_arrNeig[i], y + y_arrNeig[i])));
            }
        }

        return neighbours;
    }
}
