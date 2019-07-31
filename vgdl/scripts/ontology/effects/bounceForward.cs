using System;
using UnityEngine;

public class bounceForward : VGDLEffect
{   
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {   
        if (sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with BounceForward interaction.");
        }

        
        Vector2 dir = sprite2.lastDirection();
        dir.Normalize();

        if (sprite2.lastDirection().x * sprite2.orientation.x < 0)
        {
            dir.x *= -1;
        }

        if (sprite2.lastDirection().y * sprite2.orientation.y < 0)
        {
            dir.y *= -1;
        }

        sprite1.physics.activeMovement(sprite1, new Vector2(dir.x, dir.y), sprite2.speed);
        sprite1.orientation = new Vector2(dir.x, dir.y);
    }
}