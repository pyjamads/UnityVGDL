using System;
using UnityEngine;

public class ContinuousPhysics : GridPhysics
{
    public override VGDLMovementTypes passiveMovement(VGDLSprite sprite)
    {	
    	if(sprite.isFirstTick)
        {
            sprite.isFirstTick = false;
            return VGDLMovementTypes.STILL;
        }

        //This needs to be thoroughly tested when continuous physics are added
        //Specially the returned type.
    	
        if(sprite.speed != 0)
        {
            sprite.updatePos(sprite.orientation, (int) sprite.speed);

            if(sprite.gravity > 0 && sprite.mass > 0 && !sprite.on_ground)
            {
                Vector2 gravityAction = new Vector2(0, sprite.gravity * sprite.mass);
                activeMovement(sprite, gravityAction, 0);
            }
            sprite.speed *= (1-sprite.friction);
            return VGDLMovementTypes.MOVE;
        }
        return VGDLMovementTypes.STILL;
    }

    public override VGDLMovementTypes activeMovement(VGDLSprite sprite, Vector2 action, float speed = -1)
    {
        //Here the assumption is that the controls determine the direction of
        //acceleration of the sprite.
    	
        if(speed == 0)
            speed = sprite.speed;
        
        if(speed == -1)
            speed = sprite.speed;
    	
        var v1 = (action.x / (float)sprite.mass) + (sprite.orientation.x * speed);
        var v2 = (action.y / (float)sprite.mass) + (sprite.orientation.y * speed);

        var dir = new Vector2(v1, v2);

        var speedD = dir.magnitude;
        if(sprite.max_speed != -1) {
            speedD = Mathf.Min(dir.magnitude, sprite.max_speed);
        }

        dir.Normalize();
        var d = new Vector2(dir.x, dir.y);

        sprite.orientation = d;
        sprite.speed = speedD;

        if (action.Equals(VGDLUtils.VGDLDirections.NONE.getDirection()))
        {
            return VGDLMovementTypes.STILL;
        }
        return VGDLMovementTypes.MOVE;
    }


    /**
     * Euclidean distance between two rectangles.
     * @param r1 rectangle 1
     * @param r2 rectangle 2
     * @return Euclidean distance between the top-left corner of the rectangles.
     */
    public double distance(Rect r1, Rect r2)
    {
        var topDiff = r1.min.y - r2.min.y;
        var leftDiff = r1.min.x - r2.min.x;
        return Mathf.Sqrt(topDiff*topDiff + leftDiff*leftDiff);
    }
}