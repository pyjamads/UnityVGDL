using System;
using UnityEngine;

public class GridPhysics : VGDLPhysics
{
    /**
     * Default constructor, gridsize will be 10x10
     */
    public GridPhysics()
    {
        gridSize = new Vector2(10,10);
    }

    /**
     * Constructor of the physics, specifying the gridsize
     * @param gridsize Size of the grid.
     */
    public GridPhysics(Vector2 gridsize)
    {
        this.gridSize = gridsize;
    }
    
    public override VGDLMovementTypes passiveMovement(VGDLSprite sprite)
    {
        if (sprite.isFirstTick)
        {
            sprite.isFirstTick = false;
            return VGDLMovementTypes.STILL;
        }
        
        var speed = 1f;
        if (sprite.speed != -1)
        {
            speed = sprite.speed;
        }
        
        if(speed != 0 && sprite.is_oriented)
        {
            if (sprite.updatePos(sprite.orientation, (int)(speed * gridSize.x)))
            {
                return VGDLMovementTypes.MOVE;
            }
        }

        return VGDLMovementTypes.STILL;
    }

    public override VGDLMovementTypes activeMovement(VGDLSprite sprite, Vector2 action, float speed = -1)
    {
        if(!sprite.stationary){
            if(speed == 0)
            {
                if(sprite.speed <= 0)
                    speed = 1;
                else
                    speed = sprite.speed;
            }
	
            if(speed != 0 && !action.Equals(Vector2.negativeInfinity) && !(action.Equals(Vector2.zero)))
            {
                if(sprite.rotateInPlace)
                {
                    bool change = sprite.updateOrientation(action);
                    if(change)
                        return VGDLMovementTypes.ROTATE;
                }
	
                if(sprite.updatePos(action, (int) (speed * gridSize.x)))
                    return VGDLMovementTypes.MOVE;
            }
        }
        return VGDLMovementTypes.STILL;
    }

    public override float distance(Rect r1, Rect r2)
    {
        //NOTE: grid physics use Hamming distances between the top-left corners of sprites

        return Mathf.Abs(r1.yMin - r2.yMin) + 
               Mathf.Abs(r1.xMin - r2.xMin);
    }
}