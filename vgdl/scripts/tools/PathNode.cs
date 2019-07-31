
/**
 * Created by dperez on 13/01/16.
 * Ported to C# by pyjamads 11/07/19
 */

using System;
using UnityEngine;

public class PathNode : IComparable<PathNode>
{
    public float totalCost;
    public float estimatedCost;
    public PathNode parent;
    public Vector2 position;
    public Vector2 comingFrom;
    public int id;

    public PathNode(Vector2 pos)
    {
        estimatedCost = 0.0f;
        totalCost = 1.0f;
        parent = null;
        position = pos;
        id = ((int)(position.x) * 100 + (int)(position.y));
    }

    public int CompareTo(PathNode n)
    {
        if (estimatedCost + totalCost < n.estimatedCost + n.totalCost)
        {
            return -1;
        }

        if (estimatedCost + totalCost > n.estimatedCost + n.totalCost)
        {
            return 1;
        }
        
        return 0;
    }

    public override bool Equals(object obj)
    {
        return Vector2.Distance(position, ((PathNode)obj).position) < 0.01f;
    }

    public void setMoveDir(PathNode pre) {

        //TODO: New types of actions imply a change in this method.
        var action = VGDLUtils.VGDLDirections.NONE.getDirection();

        if(pre.position.x < position.x)
            action = VGDLUtils.VGDLDirections.RIGHT.getDirection();
        if(pre.position.x > position.x)
            action = VGDLUtils.VGDLDirections.LEFT.getDirection();

        if(pre.position.y < position.y)
            action = VGDLUtils.VGDLDirections.DOWN.getDirection();
        if(pre.position.y > position.y)
            action = VGDLUtils.VGDLDirections.UP.getDirection();

        comingFrom = new Vector2(action.x, action.y);
    }
}
